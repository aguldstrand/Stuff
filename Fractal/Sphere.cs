namespace Fractal
{
    class Sphere: IRenderable
    {
        private float radius;

        public Sphere(float radius)
        {
            this.radius = radius;
        }

        public float HitTest(Vector3 c)
        {
            return c.Len() < radius ? 40 : 0;
        }
    }
}
