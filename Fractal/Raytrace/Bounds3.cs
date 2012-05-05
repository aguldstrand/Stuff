namespace Fractal
{
    struct Bounds3
    {
        public Vector3 pos;
        public Vector3 size;

        public Bounds3(Vector3 pos, Vector3 size)
        {
            this.pos = pos;
            this.size = size;
        }
    }
}
