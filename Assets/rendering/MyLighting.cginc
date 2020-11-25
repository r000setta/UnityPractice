#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

float4 _Tint;
sampler2D _MainTex;
float4 _MainTex_ST;
float _Smoothness;
float _Metallic;

struct a2v{
    float4 position:POSITION;
    float3 normal:NORMAL;
    float2 uv:TEXCOORD0;
};

struct v2f{
    float4 position:SV_POSITION;
    float2 uv:TEXCOORD0;
    float3 normal:TEXCOORD1;
    float3 worldPos:TEXCOORD2;

    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor:TEXCOORD3;
    #endif
};

UnityLight CreateLight(v2f v){
    UnityLight light;
    #if defined(POINT) || defined(SPOT)
        light.dir=normalize(_WorldSpaceLightPos0.xyz-v.worldPos);
    #else
        light.dir=_WorldSpaceLightPos0.xyz;
    #endif

    UNITY_LIGHT_ATTENUATION(atten,0,v.worldPos);
    light.color=_LightColor0.rgb*atten;
    light.ndotl=DotClamped(v.normal,light.dir);
    return light;
}

void ComputeVertexLightColor(inout v2f v){
    #if defined(VERTEXLIGHT_ON)
        // float3 lightPos=float3(
        //     unity_4LightPosX0.x,unity_4LightPosY0.x,unity_4LightPosZ0.x
        // );
        // float3 lightVec=lightPos-v.worldPos;
        // float3 lightDir=normalize(lightVec);
        // float3 ndotl=DotClamped(v.normal,lightDir);
        // float atten=1/1+(dot(lightVec,lightVec)*unity_4LightAtten0.x);
        // v.vertexLightColor=unity_LightColor[0].rgb*ndotl*atten;
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
    return indirectLight;
}

v2f vert(a2v a){
    v2f o;
    o.uv=TRANSFORM_TEX(a.uv,_MainTex);
    o.position=UnityObjectToClipPos(a.position);
    o.normal=UnityObjectToWorldNormal(a.normal);
    o.worldPos=mul(unity_ObjectToWorld,a.position);
    ComputeVertexLightColor(o);
    return o;
}

float4 frag(v2f v):SV_TARGET{
    v.normal=normalize(v.normal);
    float3 viewDir=normalize(_WorldSpaceCameraPos-v.worldPos);
    float3 albedo=tex2D(_MainTex,v.uv).rgb*_Tint.rgb;
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