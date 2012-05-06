#include "stdafx.h"
#include <math.h>

struct Rgb {
	BYTE r, g, b;

	Rgb()
		: r(0), g(0), b(0) {
	}

	Rgb(BYTE r, BYTE g, BYTE b)
		: r(r), g(g), b(b) {
	}
};

struct Vector3 {
	float x, y, z;

	Vector3()
		: x(0), y(0), z(0) {
	}

	Vector3(const float x, const float y, const float z)
		: x(x), y(y), z(z) {
	}

	Vector3 operator+(const Vector3& v) const {
		return Vector3(x + v.x, y + v.y, z + v.z);
	}

	Vector3 operator-(const Vector3& v) const {
		return Vector3(x - v.x, y - v.y, z - v.z);
	}

	Vector3 operator*(const float s) const {
		return Vector3(x * s, y * s, z * s);
	}

	Vector3 operator/(const float s) const {
		return Vector3(x / s, y / s, z / s);
	}

	float len() const {
		return sqrt(x * x + y * y + z * z);
	}

    Vector3 normalize(float scalar = 1.0f) const {
        float len = this->len();

        return Vector3(
            x / len * scalar,
            y / len * scalar,
            z / len * scalar
        );
    }
};

struct Vector2 {
	const float x, y;

	Vector2()
		: x(0), y(0) {
	}

	Vector2(const float x, const float y)
		: x(x), y(y) {
	}
};
struct Plane3 {
	const Vector3 topLeft, topRight, bottomLeft, bottomRight;
	const float width, height;

	Plane3(const Vector3 &topLeft, const Vector3 &topRight, const Vector3 &bottomLeft, const Vector3 &bottomRight)
		: topLeft(topLeft), topRight(topRight), bottomLeft(bottomLeft), bottomRight(bottomRight), width((topRight - topLeft).len()), height((bottomRight - topRight).len()) {
	}
};

struct Bounds3 {
	const Vector3 pos, size;

	Bounds3(const Vector3 &pos, const Vector3 &size)
		: pos(pos), size(size) {
	}

	bool intersects(const Vector3 &v) const {
		return
			pos.x <= v.x && v.x < pos.x + size.x &&
			pos.y <= v.y && v.y < pos.y + size.y &&
			pos.z <= v.z && v.z < pos.z + size.z;
	}
};

struct Bounds2 {
	const Vector2 pos, size;

	Bounds2(const Vector2 &pos, const Vector2 &size)
		: pos(pos), size(size) {
	}

	bool intersects(const Vector2 &v) const {
		return
			pos.x <= v.x && v.x < pos.x + size.x &&
			pos.y <= v.y && v.y < pos.y + size.y;
	}
};

struct Camera {
	const Vector3 pos;
	const Plane3 frustrumFrontPlane;

	Camera(const Vector3 &pos, const Vector3 &lookAt, const Vector2 &frustrumSize)
		: pos(pos), frustrumFrontPlane(Plane3(
                Vector3(-frustrumSize.x / 2, frustrumSize.y / 2, 0),
                Vector3(frustrumSize.x / 2, frustrumSize.y / 2, 0),
                Vector3(-frustrumSize.x / 2, -frustrumSize.y / 2, 0),
                Vector3(frustrumSize.x / 2, -frustrumSize.y / 2, 0))) {

	}
};