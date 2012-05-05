// raytrace.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "raytrace.h"

#define threshold 0.1f

template<class TFractal>
class RayTracer {
private:
	const Bounds3 bounds;
	const TFractal &fractal;
	const Camera camera;

	static bool hitTest(const Vector3 &origin, const Vector3 &direction, const float maxDistance, const float stepSize, const TFractal &fractal, const Bounds3 &bounds, Vector3 &hitPos, float &iterations) {
        const Vector3 step = direction * stepSize;
        hitPos = origin + step;
        iterations = 0;

        while ((hitPos - origin).len() < maxDistance)
        {
			if(bounds.intersects(hitPos)) {
				iterations = fractal.hitTest(hitPos);
				if (iterations >= threshold)
				{
					return true;
				}
			}

            hitPos = hitPos + step;
        }

        return false;
	}

public:
	RayTracer(const Bounds3 &bounds, const TFractal &fractal, const Camera &camera)
		: bounds(bounds), fractal(fractal), camera(camera) {
	}

	void render(Rgb *ptr, int width, int height) {

		Rgb insideColor;
        Rgb outsideColorTable[255];
		for(int i = 0; i < 255; i++) {
			if(i < 20) {
				outsideColorTable[i] = insideColor;
			} else {
				Rgb &color = outsideColorTable[i];
				color.r = (BYTE)i;
				color.g = (BYTE)i;
				color.b = (BYTE)i;
			}
		}

		const Camera &camera = this->camera;
		const Bounds3 &bounds = this->bounds;
		const TFractal &fractal = this->fractal;

        Vector3 xStep = (camera.frustrumFrontPlane.topRight - camera.frustrumFrontPlane.topLeft) / width;
        Vector3 yStep = (camera.frustrumFrontPlane.bottomLeft - camera.frustrumFrontPlane.topLeft) / height;


		Concurrency::parallel_for(0, height, [&width, &xStep, &yStep, &ptr, &outsideColorTable, &camera, &bounds, &fractal](int y) {
		// for(int y = 0; y < height; y++) {
			std::cout <<"y:" <<y <<std::endl;

			// Concurrency::parallel_for(0, width, [&y, &width, &xStep, &yStep, &ptr, &outsideColorTable, &camera, &bounds, &fractal](int x) {
            for (int x = 0; x < width; x++) {
                const int pixelIndex = width * y + x;

                const Vector3 target = camera.frustrumFrontPlane.topLeft +
					xStep * x +
					yStep * y;

				const Vector3 ray = target - camera.pos;

				Vector3 hit;
                float iterations;
                if (hitTest(
					camera.pos,
                    ray.normalize(),
                    ray.len(),
					bounds.size.x / width,
					fractal,
					bounds,
                    hit,
                    iterations)) {
					ptr[pixelIndex] = outsideColorTable[(int)((hit - camera.pos).len() / 27 * 255)];
                }
            }
        });

	}
};

class MandelCube {
private:
	const int maxIterations;

	static Vector3 boxFold(const Vector3 &v)
    {
        return Vector3(
			v.x > 1 ? 2 - v.x : v.x < -1 ? -2 - v.x : v.x,
            v.y > 1 ? 2 - v.y : v.y < -1 ? -2 - v.y : v.y,
            v.z > 1 ? 2 - v.z : v.z < -1 ? -2 - v.z : v.z);
    }

    static Vector3 ballFold(const float r, const Vector3 &v)
    {
        float len = v.len();
        if (len < r) {
			return v.normalize(len / (r * r));
		} else if (len < 1) {
			return v.normalize(1 / len);
		}
        return v;
    }

public:
	MandelCube(const int maxIterations)
		: maxIterations(maxIterations) {
	}

	float hitTest(const Vector3 &c) const {
        const float scale = 2;
        const float r = .5f;
        const float f = 1.0f;

		Vector3 z = c;

        for (int i = 0; i < maxIterations; i++) {
            z = ballFold(r, boxFold(z) * f) * scale + c;

            if (z.len() > 40000000.0) {
                return i / (float)maxIterations;
            }
        }

        return 0;
	}
};


extern "C" {

	const int FRACTAL_MANDEL_CUBE = 1;

	RAYTRACE_API void render(
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
		Rgb *ptr,
		int width,
		int height) {

		std::cout <<"Native" <<std::endl;

		RayTracer<MandelCube> rayTracer(
			Bounds3(
			Vector3(boundPosX, boundPosY, boundPosZ),
			Vector3(boundSizeX, boundSizeY, boundSizeZ)),
			MandelCube(fractalMaxIterations),
			Camera(
				Vector3(camPosX, camPosY, camPosZ),
				Vector3(camLookAtX, camLookAtY, camLookAtZ),
				Vector2(camFrustrumSizeX, camFrustrumSizeY)));

		rayTracer.render(ptr, width, height);

	}
}