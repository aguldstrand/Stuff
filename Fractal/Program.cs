using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Fractal
{
    class Program
    {
        static void Main(string[] args)
        {
            // MandelcubeSlice();
            Mandelcube();
        }

        private static void Mandelcube()
        {
            var start = DateTime.Now;

            int width = 400;
            int height = 400;

            var rayTracer = new RayTracer(
                bounds: new Bounds3(
                    pos: new Vector3(-6.5f, -6.5f, -6.5f),
                    size: new Vector3(13f, 13f, 13f)),
                fractal: new MandelCube(
                    maxIterations: 255),
                camera: new Camera(
                    pos: new Vector3(0, 0, -30),
                    lookAt: new Vector3(0, 0, 0),
                    frustrumSize: new Vector2(18f, 18f)));

            using (var outp = new Bitmap(width, height))
            {
                var bits = outp.LockBits(new Rectangle(Point.Empty, outp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                rayTracer.Render(bits.Scan0, width, height);

                outp.UnlockBits(bits);

                var folder = @"..\..\..\Render\";
                Directory.CreateDirectory(folder);
                outp.Save(Path.Combine(folder, "outp.png"));
            }

            Console.WriteLine(DateTime.Now - start);
        }

        private static void MandelcubeSlice()
        {
            var width = 2048;
            var height = 2048;
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
                .ForAll(z =>
                {
                    var start = DateTime.Now;
                    Console.WriteLine(z);
                    using (var outp = new Bitmap(width, height))
                    {
                        var bits = outp.LockBits(new Rectangle(Point.Empty, outp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                        unsafe
                        {
                            Rgb* ptr = (Rgb*)bits.Scan0.ToPointer();

                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    var i = width * y + x;

                                    ptr[i] = outsideColorTable[(int)(fractal.HitTest(new Vector3
                                    {
                                        x = x / (float)width * bounds.Width + bounds.X,
                                        y = y / (float)height * bounds.Width + bounds.Y,
                                        z = z / (float)height * bounds.Width + bounds.Y,
                                    }) * outsideColorTable.Length)];
                                }
                            }
                        }

                        outp.UnlockBits(bits);
                        outp.Save(string.Format(@"C:\Users\Anders\Desktop\mandelbox\apa_{0}.png", z));
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