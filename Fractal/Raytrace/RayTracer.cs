using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fractal
{
    class RayTracer
    {
        private Bounds3 bounds;
        private IRenderable fractal;
        private Camera camera;
        private const float threshold = .1f;

        public RayTracer(Bounds3 bounds, IRenderable fractal, Camera camera)
        {
            this.bounds = bounds;
            this.fractal = fractal;
            this.camera = camera;
        }

        public unsafe void Render(IntPtr bits, int width, int height)
        {
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


            Vector3 xStep = camera.frustrumFrontPlane.topRight.Sub(ref camera.frustrumFrontPlane.topLeft).Div(width);
            Vector3 yStep = camera.frustrumFrontPlane.bottomLeft.Sub(ref camera.frustrumFrontPlane.topLeft).Div(height);

            Rgb* ptr = (Rgb*)bits;
            Enumerable.Range(0, height)
                .AsParallel()
                .WithDegreeOfParallelism(2)
                .ForAll(y =>
                {
                    Console.WriteLine("y:{0} ", y);

                    for (int x = 0; x < width; x++)
                    {
                        int pixelIndex = width * y + x;

                        Vector3 target = camera.frustrumFrontPlane.topLeft
                            .Add(xStep.Mul(x))
                            .Add(yStep.Mul(y));

                        Vector3 ray = target.Sub(ref camera.pos);

                        Vector3 hit;
                        float iterations;
                        if (HitTest(
                            origin: camera.pos,
                            direction: ray.Normalize(),
                            maxDistance: ray.Len(),
                            stepSize: bounds.size.x / width,
                            hitPos: out hit,
                            iterations: out iterations))
                        {
                            // ptr[pixelIndex] = outsideColorTable[(int)(hit.Len() / 12 * 255)];
                            ptr[pixelIndex] = outsideColorTable[(int)(hit.Sub(ref camera.pos).Len() / 27 * 255)];
                        }
                    }
                });
        }

        private bool HitTest(Vector3 origin, Vector3 direction, float maxDistance, float stepSize, out Vector3 hitPos, out float iterations)
        {
            Vector3 step = direction.Mul(stepSize);
            hitPos = origin.Add(step);
            iterations = 0;

            while (hitPos.Sub(ref origin).Len() < maxDistance)
            {
                iterations = fractal.HitTest(hitPos);
                if (iterations >= threshold)
                {
                    return true;
                }

                hitPos.AddAssign(ref step);
            }

            return false;
        }
    }
}