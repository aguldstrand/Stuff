using System;

namespace Fractal
{
    struct Camera
    {
        public Vector3 pos;
        public Plane3 frustrumFrontPlane;

        public Camera(Vector3 pos, Vector3 lookAt, Vector2 frustrumSize)
        {
            this.pos = pos;

            var dir = lookAt.Sub(ref pos);
            frustrumFrontPlane.topLeft.x = -frustrumSize.x / 2;
            frustrumFrontPlane.topLeft.y = frustrumSize.y / 2;
            frustrumFrontPlane.bottomRight.x = frustrumSize.x / 2;
            frustrumFrontPlane.bottomRight.y = -frustrumSize.y / 2;

            this.frustrumFrontPlane = new Plane3(
                topLeft: new Vector3(-frustrumSize.x / 2, frustrumSize.y / 2, 0),
                topRight: new Vector3(frustrumSize.x / 2, frustrumSize.y / 2, 0),
                bottomLeft: new Vector3(-frustrumSize.x / 2, -frustrumSize.y / 2, 0),
                bottomRight: new Vector3(frustrumSize.x / 2, -frustrumSize.y / 2, 0));
        }
    }
}
