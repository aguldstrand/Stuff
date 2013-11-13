// raytrace.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "raytrace.h"

#define threshold 0.1f

Vector3 renderer::MandelCube::boxFold(const Vector3 &v)
{
    return Vector3(
		v.x > 1 ? 2 - v.x : v.x < -1 ? -2 - v.x : v.x,
        v.y > 1 ? 2 - v.y : v.y < -1 ? -2 - v.y : v.y,
        v.z > 1 ? 2 - v.z : v.z < -1 ? -2 - v.z : v.z);
}

Vector3 renderer::MandelCube::ballFold(const float r, const Vector3 &v)
{
    float len = v.len();
    if (len < r) {
		return v.normalize(len / (r * r));
	} else if (len < 1) {
		return v.normalize(1 / len);
	}
    return v;
}

renderer::MandelCube::MandelCube(const int maxIterations)
	: maxIterations(maxIterations) {
}

float renderer::MandelCube::hitTest(const Vector3 &c) const {
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


bool renderer::RayTracer::hitTest(const Vector3 &origin, const Vector3 &direction, const float maxDistance, const float stepSize, const MandelCube &fractal, const Bounds3 &bounds, Vector3 &hitPos, float &iterations) {
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

renderer::RayTracer::RayTracer(const Bounds3 &bounds, const MandelCube &fractal, const Camera &camera)
	: bounds(bounds), fractal(fractal), camera(camera) {
}

void renderer::RayTracer::render(Rgb *ptr, int width, int height) {

	Rgb insideColor;
    Rgb outsideColorTable[255];
	for(int i = 0; i < 255; i++) {
		if(i < 20) {
			outsideColorTable[i] = insideColor;
		} else {
			Rgb &color = outsideColorTable[i];
			color.r = (BYTE)(i % (256 / 3));
			color.g = (BYTE)(i % (256 / 3));
			color.b = (BYTE)(i % (256 / 3));
		}
	}

	const Camera &camera = this->camera;
	const Bounds3 &bounds = this->bounds;
	const MandelCube &fractal = this->fractal;

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

void renderer::RayTracer::startRender(
	Rgb *ptr,
	int width,
	int height,
	std::function<void(const Bounds2&)> blockCompleteCallback,
	std::function<void(void)> imageCompleteCallback) {
}



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

		renderer::RayTracer rayTracer(
			Bounds3(
				Vector3(boundPosX, boundPosY, boundPosZ),
				Vector3(boundSizeX, boundSizeY, boundSizeZ)),
			renderer::MandelCube(fractalMaxIterations),
			Camera(
				Vector3(camPosX, camPosY, camPosZ),
				Vector3(camLookAtX, camLookAtY, camLookAtZ),
				Vector2(camFrustrumSizeX, camFrustrumSizeY)));

		rayTracer.render(ptr, width, height);
	}

	RAYTRACE_API void slice(int width, int height, float bx, float by, float bw, float bh, int z, Rgb *ptr)
	{
		renderer::MandelCube mandelCube(255);

		Rgb insideColor;
		Rgb outsideColorTable[255];
		for (int i = 0; i < 255; i++) {
			if (i < 20) {
				outsideColorTable[i] = insideColor;
			}
			else {
				Rgb &color = outsideColorTable[i];
				color.r = (BYTE)(i % (256 / 3));
				color.g = (BYTE)(i % (256 / 3));
				color.b = (BYTE)(i % (256 / 3));
			}
		}

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int i = width * y + x;

				Vector3 coordinate(
					x / (float)width * bw + bx,
					y / (float)height * bw + by,
					z / (float)height * bw + by);

				ptr[i] = outsideColorTable[(int)(mandelCube.hitTest(coordinate) * 255)];
			}
		}
	}
}