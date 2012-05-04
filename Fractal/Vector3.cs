using System;

namespace Fractal
{
    struct Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float Len()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        public float LenSq()
        {
            return x * x + y * y + z * z;
        }

        public float Dot(ref Vector3 v)
        {
            return x * v.x + y * v.y + v.z * z;
        }

        public Vector3 Add(Vector3 v)
        {
            return new Vector3(
                x: x + v.x,
                y: y + v.y,
                z: z + v.z
            );
        }

        public Vector3 Add(ref Vector3 v)
        {
            return new Vector3(
                x: x + v.x,
                y: y + v.y,
                z: z + v.z);
        }

        public void AddAssign(ref Vector3 v)
        {
            x += v.x;
            y += v.y;
            z += v.z;
        }

        public Vector3 Sub(ref Vector3 v)
        {
            return new Vector3(
                x: x - v.x,
                y: y - v.y,
                z: z - v.z
            );
        }

        public Vector3 Mul(float scalar)
        {
            return new Vector3(
                x: x * scalar,
                y: y * scalar,
                z: z * scalar
            );
        }

        public override string ToString()
        {
            return x + " " + y + " " + z;
        }

        public void MulAssign(float scalar)
        {
            x *= scalar;
            y *= scalar;
            z *= scalar;
        }

        public Vector3 Normalize(float scalar = 1.0f)
        {
            float len = Len();

            return new Vector3(
                x: x / len * scalar,
                y: y / len * scalar,
                z: z / len * scalar
            );
        }

        public Vector3 Div(float scalar)
        {
            return new Vector3(
                x: x / scalar,
                y: y / scalar,
                z: z / scalar);
        }
    }
}
