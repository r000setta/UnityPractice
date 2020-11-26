#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

float4 _Tint;
sampler2D _MainTex,_DetailTex;
float4 _MainTex_ST,_DetailTex_ST;
float _Smoothness;
float _Metallic;

sampler2D _NormalMap,_DetailNormalMap;
float _BumpScale,_DetailBumpScale;

struct a2v{
    float4 vertex:POSITION;
    float3 normal:NORMAL;
    float4 tangent:TANGENT;
    float2 uv:TEXCOORD0;
};

struct v2f{
    float4 pos:SV_POSITION;
    float4 uv:TEXCOORD0;
    float3 normal:TEXCOORD1;

    #if defined(BINORMAL_PER_FRAGMENT)
        float4 tangent:TEXCOORD2;
    #else
        float3 tangent:TEXCOORD2;
        float3 binormal:TEXCOORD3;
    #endif

    float3 worldPos:TEXCOORD4;

    SHADOW_COORDS(5)

    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor:TEXCOORD6;
    #endif
};

UnityLight CreateLight(v2f v){
    UnityLight light;
    #if defined(POINT) || defined(SPOT)
        light.dir=normalize(_WorldSpaceLightPos0.xyz-v.worldPos);
    #else
        light.dir=_WorldSpaceLightPos0.xyz;
    #endif

    UNITY_LIGHT_ATTENUATION(attenuation,v,v.worldPos);

    light.color=_LightColor0.rgb*attenuation;
    light.ndotl=DotClamped(v.normal,light.dir);
    return light;
}

float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}

void ComputeVertexLightColor(inout v2f v){
    #if defined(VERTEXLIGHT_ON)
        v.vertexLightColor=Shade4PointLights(
            unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
            unity_LightColor[0].rgb,unity_LightColor[1].rgb,
            unity_LightColor[2].rgb,unity_LightColor[3].rgb,
            unity_4LightAtten0,v.worldPos,v.normal
        );
    #endif
}

UnityIndirect CreateIndirectLight(v2f v){
    UnityIndirect indirectLight;
    indirectLight.diffuse=0;
    indirectLight.specular=0;

    #if defined(VERTEXLIGHT_ON)
        indirectLight.diffuse=v.vertexLightColor;
    #endif
    #if defined(FORWARD_BASE_PASS)
        indirectLight.diffuse+=max(0,ShadeSH9(float4(v.normal,1)));
    #endif
    return indirectLight;
}

void InitializeFragmentNormal(inout v2f v){
    // float2 du=float2(_HeightMap_TexelSize.x*0.5,0);
    // float u1=tex2D(_HeightMap,v.uv-du);
    // float u2=tex2D(_HeightMap,v.uv+du);

    // float2 dv=float2(0,_HeightMap_TexelSize.y*0.5);
    // float v1=tex2D(_HeightMap,v.uv-dv);
    // float v2=tex2D(_HeightMap,v.uv+dv);
    // v.normal=float3(u1-u2,1,v1-v2);
    float3 mainNormal=UnpackScaleNormal(tex2D(_NormalMap,v.uv.xy),_BumpScale);
    float3 detailNormal=UnpackScaleNormal(tex2D(_DetailNormalMap,v.uv.zw),_DetailBumpScale);
    float3 tangentSpaceNormal=BlendNormals(mainNormal,detailNormal);
    #if defined(BINORMAL_PER_FRAGMENT)
		float3 binormal = CreateBinormal(v.normal, v.tangent.xyz, v.tangent.w);
	#else
		float3 binormal = v.binormal;
	#endif
    v.normal=normalize(
        tangentSpaceNormal.x*v.tangent+
        tangentSpaceNormal.y*binormal+
        tangentSpaceNormal.z*v.normal
    );
    v.normal=v.normal.xzy;
}

v2f vert(a2v a){
    v2f o;
    o.uv.xy=TRANSFORM_TEX(a.uv,_MainTex);
    o.uv.zw=TRANSFORM_TEX(a.uv,_DetailTex);
    o.pos=UnityObjectToClipPos(a.vertex);
    o.normal=UnityObjectToWorldNormal(a.normal);

    #if defined(BINORMAL_PER_FRAGMENT)
		o.tangent = float4(UnityObjectToWorldDir(a.tangent.xyz), a.tangent.w);
	#else
		o.tangent = UnityObjectToWorldDir(a.tangent.xyz);
		o.binormal = CreateBinormal(o.normal, o.tangent, a.tangent.w);
	#endif
    o.tangent=float4(UnityObjectToWorldDir(a.tangent.xyz),a.tangent.w);
    o.worldPos=mul(unity_ObjectToWorld,a.vertex);
    
    TRANSFER_SHADOW(o);
    ComputeVertexLightColor(o);
    return o;
}

float4 frag(v2f v):SV_TARGET{
    InitializeFragmentNormal(v);
    float3 viewDir=normalize(_WorldSpaceCameraPos-v.worldPos);
    float3 albedo=tex2D(_MainTex,v.uv.xy).rgb*_Tint.rgb;
    albedo*=tex2D(_DetailTex,v.uv.zw)*unity_ColorSpaceDouble;
    float3 specularTint;
    float oneMinusReflectivity;
    albedo=DiffuseAndSpecularFromMetallic(
        albedo,_Metallic,specularTint,oneMinusReflectivity
    );

    return UNITY_BRDF_PBS(
        albedo,specularTint,
        oneMinusReflectivity,_Smoothness,
        v.normal,viewDir,
        CreateLight(v),CreateIndirectLight(v)
    );
}

#endif