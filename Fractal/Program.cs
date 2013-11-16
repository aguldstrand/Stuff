using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        private static unsafe void Precompute()
        {
            for (var i = 11; i < 12; i++)
            {
                var side = (int)Math.Pow(2, i);
                var path = string.Format("mandelcube-{0}.bin", side);
                Stopwatch sw = new Stopwatch();
                sw.Start();

                if (!File.Exists(path))
                {
                    byte[] buffer = new byte[(int)Math.Ceiling((side * side * side) / 8f)];
                    fixed (byte* pBuffer = buffer)
                    {
                        compute(side, -6.5f, 13f, new IntPtr(pBuffer));
                    }

                    File.WriteAllBytes(path, buffer);
                }
                sw.Stop();
                Console.WriteLine(path + " - " + sw.Elapsed);
            }
        }

        private static void Mandelcube()
        {
            MandelcubeSlice();
            return;

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

        [DllImport("raytrace.dll")]
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
            int side,
            float boundsOffset,
            float boundsSide,
            IntPtr buffer);


        private static void MandelcubeSlice()
        {
            var width = 8;
            var height = 8;
            var bounds = new RectangleF(-6.5f, -6.5f, 13.0f, 13.0f);

            Rgb zero = new Rgb { r = 0, g = 61, b = 245 };
            Rgb one = new Rgb { r = 245, g = 184, b = 0 };

            Rgb insideColor = new Rgb();
            Rgb[] outsideColorTable = Enumerable.Range(0, 255)
                .Select(i =>
                    i <= 20 ? insideColor :
                    new Rgb
                    {
                        r = (byte)i,
                        g = (byte)i,
                        b = (byte)i,
                    })
                .ToArray();

            var fractal = new MandelCube(255);

            Enumerable.Range(0, height)
                .AsParallel()
                .AsOrdered()
                .ForAll(z =>
                {
                    var start = DateTime.Now;
                    Console.WriteLine(z);
                    using (var outp = new Bitmap(width, height))
                    {
                        var bits = outp.LockBits(new Rectangle(Point.Empty, outp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                        var ptr = bits.Scan0;
                        slice(width, height, bounds.X, bounds.Y, bounds.Width, bounds.Height, z, ptr);

                        outp.UnlockBits(bits);
                        outp.Save(string.Format(@"..\Render\hd\{0}.png", z));
                    }

                    Console.WriteLine(DateTime.Now - start);
                });
        }
    }

    struct Rgb
    {
        public byte r, g, b;
    }
}