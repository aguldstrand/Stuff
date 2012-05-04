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
}