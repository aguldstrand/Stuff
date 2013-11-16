// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the RAYTRACE_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// RAYTRACE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef RAYTRACE_EXPORTS
#define RAYTRACE_API __declspec(dllexport)
#else
#define RAYTRACE_API __declspec(dllimport)
#endif

#include "rgba.h"
#include <iostream>
#include <ppl.h>

namespace renderer {


	RAYTRACE_API class MandelCube {
	private:
		const int maxIterations;

		static Vector3 boxFold(const Vector3 &v);

		static Vector3 ballFold(const float r, const Vector3 &v);

	public:
		RAYTRACE_API MandelCube(const int maxIterations);

		float hitTest(const Vector3 &c) const;
	};


	class RayTracer {
	private:
		const Bounds3 bounds;
		const MandelCube &fractal;
		const Camera camera;

		static bool hitTest(const Vector3 &origin, const Vector3 &direction, const float maxDistance, const float stepSize, const MandelCube &fractal, const Bounds3 &bounds, Vector3 &hitPos, float &iterations);
	public:
		RAYTRACE_API RayTracer(const Bounds3 &bounds, const MandelCube &fractal, const Camera &camera);

		RAYTRACE_API void render(Rgb *ptr, int width, int height);

		RAYTRACE_API void startRender(
			Rgb *ptr,
			int width,
			int height,
			std::function<void(const Bounds2&)> blockCompleteCallback,
			std::function<void(void)> imageCompleteCallback);
	};





	extern "C" {

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
			int height);

		RAYTRACE_API void slice(
			int width,
			int height,
			float bx,
			float by,
			float bw,
			float bh,
			int z,
			Rgb *ptr);
	
		RAYTRACE_API void compute(
			int side,
			float boundsOffset,
			float boundsSide,
			BYTE *buffer);

	}

};