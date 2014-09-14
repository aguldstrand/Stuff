using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;

namespace Fractal
{
    class Program
    {
        static void Main(string[] args)
        {
            // MandelcubeSlice();
            // Mandelcube();
            Precompute();
        }

        private static void MandelcubeSlice()
        {
            var width = 1024 * 1;
            var height = width;
            var globalBounds = new RectangleF(-6.5f, -6.5f, 13.0f, 13.0f);

            var numTiles = 16;
            var tileWidth = globalBounds.Width / numTiles;
            var tiles = Enumerable.Range(0, numTiles)
                .SelectMany(x => Enumerable.Range(0, numTiles)
                    .Select(y => new
                    {
                        x = x,
                        y = y,
                        bounds = new RectangleF(
                            globalBounds.X + x * tileWidth,
                            globalBounds.Y + y * tileWidth,
                            tileWidth,
                            tileWidth)
                    }))
                .ToArray();

            Enumerable.Range(width / 2, 1)
                .AsParallel()
                .AsOrdered()
                .ForAll(z =>
                {
                    tiles.AsParallel()
                        .ForAll(tile =>
                        {
                            var localBounds = tile.bounds;

                            var start = DateTime.Now;
                            Console.WriteLine(z);
                            using (var outp = new Bitmap(width, height))
                            {
                                var bits = outp.LockBits(new Rectangle(Point.Empty, outp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                                var ptr = bits.Scan0;
                                slice(width, height, localBounds.X, localBounds.Y, localBounds.Width, localBounds.Height, z, ptr);

                                outp.UnlockBits(bits);
                                var path = string.Format(@"..\Render\hd\{0} {1}x{2}.png", z, tile.x, tile.y);
                                Directory.CreateDirectory(Path.GetDirectoryName(path));
                                outp.Save(path);
                            }

                            Console.WriteLine(DateTime.Now - start);
                        });
                });
        }

        private static unsafe void Precompute()
        {
            var samples = 128;
            var divideBy = 8;

            var globalBounds = new Cube(-6.5f, -6.5f, -6.5f, 13.0f);

            var tileWidth = globalBounds.Side / divideBy;
            var tiles = Enumerable.Range(0, divideBy)
                .SelectMany(x => Enumerable.Range(0, divideBy)
                    .SelectMany(y => Enumerable.Range(0, divideBy)
                        .Select(z => new
                        {
                            x = x,
                            y = y,
                            z = z,
                            bounds = new Cube(
                                globalBounds.X + x * tileWidth,
                                globalBounds.Y + y * tileWidth,
                                globalBounds.Z + z * tileWidth,
                                tileWidth)
                        })))
                .ToArray();

            Directory.CreateDirectory("data");

            tiles.AsParallel()
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .ForAll(tile =>
                {
                    Console.WriteLine("x:{0} y:{1} z:{2}", tile.x, tile.y, tile.z);
                    var buffer = new byte[samples * samples * samples]; // 1 sample per bit
                    unsafe
                    {
                        fixed (byte* bufferPtr = buffer)
                        {
                            compute(tile.bounds, samples, (IntPtr)bufferPtr);
                        }
                    }

                    using (var stream = new GZipStream(File.Create(string.Format("data/tile-{0}-{1}-{2}.bin", tile.x, tile.y, tile.z)), CompressionLevel.Fastest))
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }
                });
        }

        private static void Mandelcube()
        {
            var start = DateTime.Now;

            int width = 512;
            int height = 512;

            var rayTracer = new RayTracer(
                bounds: new Bounds3(
                    pos: new Vector3(-6.5f, -6.5f, -6.5f),
                    size: new Vector3(13f, 13f, 13f)),
                fractal: new MandelCube(
                    maxIterations: 255),
                camera: new Camera(
                    pos: new Vector3(0, 0, -10),
                    lookAt: new Vector3(0, 0, 0),
                    frustrumSize: new Vector2(18f, 18f)));

            using (var outp = new Bitmap(width, height))
            {
                var bits = outp.LockBits(new Rectangle(Point.Empty, outp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                /*
                render(
                    -6.5f, -6.5f, -6.5f,
                    13f, 13f, 13f,
                    255,
                    0, 0, -30,
                    0, 0, 0,
                    18, 18,
                    bits.Scan0, width, height);
                 * */
                rayTracer.Render(bits.Scan0, width, height);

                outp.UnlockBits(bits);

                var folder = @"..\Render\";
                Directory.CreateDirectory(folder);
                outp.Save(Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd hhmmss") + ".png"));
            }

            Console.WriteLine(DateTime.Now - start);
        }

        [DllImport("raytrace.dll")]
        private static extern void render(
            float boundPosX,
            float boundPosY,
            float boundPosZ,
            float boundSizeX,
            float boundSizeY,
            float boundSizeZ,
            int fractalMaxIterations,
            float camPosX,
            float camPosY,
            float camPosZ,
            float camLookAtX,
            float camLookAtY,
            float camLookAtZ,
            float camFrustrumSizeX,
            float camFrustrumSizeY,
            IntPtr ptr,
            int width,
            int height);

        [DllImport("raytrace.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void slice(
            int width,
            int height,
            float bx,
            float by,
            float bw,
            float bh,
            int z,
            IntPtr ptr);

        [DllImport("raytrace.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void compute(
            Cube bounds,
            int samples,
            IntPtr buffer);
    }

    struct Rgb
    {
        public byte r, g, b;
    }

    struct Cube
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public readonly float Side;

        public Cube(float x, float y, float z, float side)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;

            this.Side = side;
        }
    }
}
