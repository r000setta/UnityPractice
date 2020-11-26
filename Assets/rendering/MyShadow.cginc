// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#if !defined(MY_SHADOWS_INCLUDED)
#define MY_SHADOWS_INCLUDED

#include "UnityCG.cginc"

struct a2v {
	float4 position : POSITION;
    float3 normal:NORMAL;
};

float4 vert (a2v a) : SV_POSITION {
    float4 position = UnityClipSpaceShadowCasterPos(a.position.xyz, a.normal);
	return UnityApplyLinearShadowBias(position);
}

half4 frag () : SV_TARGET {
	return 0;
}

#endif