struct GeoOut {
	float3 Pos : POSITION;
	uint PrimID : SV_PrimitiveID;
};

float4 VS(float4 position : POSITION) : SV_POSITION
{
	return position+1;
}

// https://github.com/ericrrichards/dx11/blob/master/DX11/TreeBillboardDemo/FX/TreeSprite.fx
[maxvertexcount(36)]
void GS(
	point float3 gins[1]: POSITION,
	uint primID : SV_PrimitiveID,
	inout TriangleStream<GeoOut> triStream) {

	float3 pos = gins[0];

	float size = 0.5f;

	float3
		topLeftRear = float3(pos.x - size, pos.y + size, pos.z - size),
		topRightRear = float3(pos.x + size, pos.y + size, pos.z - size),
		topLeftFront = float3(pos.x - size, pos.y + size, pos.z + size),
		topRightFront = float3(pos.x + size, pos.y + size, pos.z + size),
		bottomLeftRear = float3(pos.x - size, pos.y - size, pos.z - size),
		bottomRightRear = float3(pos.x + size, pos.y - size, pos.z - size),
		bottomLeftFront = float3(pos.x - size, pos.y - size, pos.z + size),
		bottomRightFront = float3(pos.x + size, pos.y - size, pos.z + size);

	GeoOut outp;
	outp.PrimID = primID;

	outp.Pos = pos; triStream.Append(outp);


	/*
	// Top
	outp.Pos = topLeftRear; triStream.Append(outp);
	outp.Pos = topRightRear; triStream.Append(outp);
	outp.Pos = topLeftFront; triStream.Append(outp);

	outp.Pos = topRightRear; triStream.Append(outp);
	outp.Pos = topRightFront; triStream.Append(outp);
	outp.Pos = topLeftFront; triStream.Append(outp);

	// Bottom
	outp.Pos = bottomRightRear; triStream.Append(outp);
	outp.Pos = bottomLeftFront; triStream.Append(outp);
	outp.Pos = bottomLeftRear; triStream.Append(outp);

	outp.Pos = bottomRightFront; triStream.Append(outp);
	outp.Pos = bottomLeftFront; triStream.Append(outp);
	outp.Pos = bottomRightRear; triStream.Append(outp);

	// Left
	outp.Pos = bottomLeftRear; triStream.Append(outp);
	outp.Pos = topLeftRear; triStream.Append(outp);
	outp.Pos = topLeftFront; triStream.Append(outp);

	outp.Pos = topLeftRear; triStream.Append(outp);
	outp.Pos = topLeftFront; triStream.Append(outp);
	outp.Pos = bottomLeftFront; triStream.Append(outp);

	// Right
	outp.Pos = topRightRear; triStream.Append(outp);
	outp.Pos = topRightFront; triStream.Append(outp);
	outp.Pos = bottomRightRear; triStream.Append(outp);

	outp.Pos = topRightFront; triStream.Append(outp);
	outp.Pos = bottomRightFront; triStream.Append(outp);
	outp.Pos = topRightRear; triStream.Append(outp);

	// Front
	outp.Pos = topLeftFront; triStream.Append(outp);
	outp.Pos = topRightFront; triStream.Append(outp);
	outp.Pos = bottomRightFront; triStream.Append(outp);

	outp.Pos = bottomRightFront; triStream.Append(outp);
	outp.Pos = bottomLeftFront; triStream.Append(outp);
	outp.Pos = topLeftFront; triStream.Append(outp);

	// Back
	outp.Pos = topRightFront; triStream.Append(outp);
	outp.Pos = bottomRightFront; triStream.Append(outp);
	outp.Pos = topLeftFront; triStream.Append(outp);

	outp.Pos = bottomLeftFront; triStream.Append(outp);
	outp.Pos = topLeftFront; triStream.Append(outp);
	outp.Pos = bottomRightFront; triStream.Append(outp);
	*/
}

float4 PS(GeoOut pin) : SV_Target {
	return float4(0.5f, 0.5f, 0.0f, 1.0f);
}