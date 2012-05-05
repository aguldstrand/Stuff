using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractal
{
    struct Vector2
    {
        public float x, y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float Len()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        public float Dot(ref Vector2 v)
        {
            return x * v.x + y * v.y;
        }

        public Vector2 Add(ref Vector2 v)
        {
            return new Vector2
            {
                x = x + v.x,
                y = y + v.y,
            };
        }

        public Vector2 Mul(float scalar)
        {
            return new Vector2
            {
                x = x * scalar,
                y = y * scalar,
            };
        }

        public override string ToString()
        {
            return x + " " + y;
        }

        public void MulAssign(int scalar)
        {
            x *= scalar;
            y *= scalar;
        }

        public Vector2 Normalize(float scalar = 1.0f)
        {
            float len = Len();

            return new Vector2
            {
                x = x / len * scalar,
                y = y / len * scalar,
            };
        }
    }
}
