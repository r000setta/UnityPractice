#if !defined(FLAT_WIREFRAME_INCLUDED)
#define FLAT_WIREFRAME_INCLUDED

#define CUSTOM_GEOMETRY_INTERPOLATORS \
	float2 barycentricCoordinates : TEXCOORD9;

#include "My Lighting Input.cginc"

float3 _WireframeColor;
float _WireframeSmoothing;
float _WireframeThickness;

float3 GetAlbedoWithWireframe(Interpolators i){
    float3 albedo=GetAlbedo(i);
    float3 barys;
    barys.xy=i.barycentricCoordinates;
    barys.z=1-barys.x-barys.y;
    float deltas=fwidth(barys);
    float3 smoothing=deltas*_WireframeSmoothing;
    float3 thickness=deltas*_WireframeThickness;
    barys=smoothstep(thickness,thickness+smoothing,barys);
    float minBary=min(barys.x,min(barys.y,barys.z));
    return lerp(_WireframeColor,albedo,minBary);
}

#define ALBEDO_FUNCTION GetAlbedoWithWireframe

#include "My Lighting.cginc"

struct InterpolatorsGeometry {
	InterpolatorsVertex data;
    CUSTOM_GEOMETRY_INTERPOLATORS
};

[maxvertexcount(3)]
void MyGeometryProgram (triangle InterpolatorsVertex i[3],
                        inout TriangleStream<InterpolatorsGeometry> stream)
{
    InterpolatorsGeometry g0,g1,g2;
    g0.data = i[0];
	g1.data = i[1];
	g2.data = i[2];

    g0.barycentricCoordinates = float2(1, 0);
	g1.barycentricCoordinates = float2(0, 1);
	g2.barycentricCoordinates = float2(0, 0);

	stream.Append(g0);
	stream.Append(g1);
	stream.Append(g2);
}

#endif