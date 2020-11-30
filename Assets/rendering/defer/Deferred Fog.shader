Shader "Custom/Deferred Fog"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off
        ZTest Always
        ZWrite Off

        Pass{
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #define FOG_DISTANCE

            #include "UnityCG.cginc"

            sampler2D _MainTex,_CameraDepthTexture;

            float3 _FrustumCorners[4];

            struct a2v{
                float4 vertex:POSITION;
                float2 uv:TEXCOORD0;
            };
            
            struct v2f{
                float4 pos:SV_POSITION;
                float2 uv:TEXCOORD0;
                #if defined(FOG_DISTANCE)
					float3 ray : TEXCOORD1;
				#endif
            };

            v2f vert(a2v a){
                v2f v;
                v.pos=UnityObjectToClipPos(a.vertex);
                v.uv=a.uv;
                #if defined(FOG_DISTANCE)
					v.ray = _FrustumCorners[a.uv.x + 2 * a.uv.y];
				#endif
                return v;
            }

            float4 frag(v2f v):SV_TARGET{
                float depth=SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,v.uv);
                depth=Linear01Depth(depth);
                float viewDistance=depth*_ProjectionParams.z-_ProjectionParams.y;
                #if defined(FOG_DISTANCE)
					viewDistance = length(v.ray * depth);
				#endif
                UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
                unityFogFactor=saturate(unityFogFactor);
                if (depth > 0.9999) {
					unityFogFactor = 1;
				}
                #if !defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2)
					unityFogFactor = 1;
				#endif
                float3 sourceColor=tex2D(_MainTex,v.uv).rgb;
                float3 foggedColor=lerp(unity_FogColor.rgb,sourceColor,unityFogFactor);
                return float4(foggedColor,1);
            }

            ENDCG
        }
    }
}
