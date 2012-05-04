namespace Fractal
{
    class MandelCube : Fractal.IRenderable
    {
        private readonly int maxIterations;

        public MandelCube(int maxIterations)
        {
            this.maxIterations = maxIterations;
        }

        public float HitTest(Vector3 c)
        {
            const float scale = 2f;
            const float r = .5f;
            const float f = 1.0f;

            var z = c;

            for (int i = 0; i < maxIterations; i++)
            {
                z = BoxFold(z).Mul(f);
                z = BallFold(r, z).Mul(scale);
                z = z.Add(ref c);

                if (z.Len() > 40000000.0)
                {
                    return i / (float)maxIterations;
                }
            }

            return 0;
        }

        static Vector3 BoxFold(Vector3 v)
        {
            return new Vector3
            {
                x = v.x > 1 ? 2 - v.x : v.x < -1 ? -2 - v.x : v.x,
                y = v.y > 1 ? 2 - v.y : v.y < -1 ? -2 - v.y : v.y,
                z = v.z > 1 ? 2 - v.z : v.z < -1 ? -2 - v.z : v.z,
            };
        }

        private static Vector3 BallFold(float r, Vector3 v)
        {
            float len = v.Len();
            if (len < r) return v.Normalize(len / (r * r));
            else if (len < 1) return v.Normalize(1 / len);
            return v;
        }

    }
}
