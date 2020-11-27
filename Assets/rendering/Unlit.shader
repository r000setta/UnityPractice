Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Tint("Tint",Color)=(1,1,1,1)
        _MainTex("Albedo",2D)="white"{}
        [NoScaleOffset] _NormalMap("Heights",2D)="bump"{}
        _BumpScale("Bump Scale",Float)=1
        [Gamma] _Metallic("Metallic",Range(0,1))=0
        _Smoothness("Smoothness",Range(0,1))=0.5
        _DetailTex("Detail Texture",2D)="gray"{}
        [NoScaleOffset] _DetailNormalMap("Detail Normal",2D)="bump"{}
        _DetailBumpScale("Detail Bump Scale",Float)=1
    }



    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags{
                "LightMode"="ForwardBase"
            }
            CGPROGRAM
            #pragma target 3.0

            #pragma multi_compile _ SHADOWS_SCREEN
            #pragma multi_compile _ VERTEXLIGHT_ON
            #pragma multi_compile_fwdbase
            #pragma vertex vert
            #pragma fragment frag

            #define FORWARD_BASE_PASS

            #include "MyLighting.cginc"
           
            ENDCG
        }

        Pass{
            Tags{
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off

            CGPROGRAM
            #pragma target 3.5

            #pragma multi_compile_fwdadd_fullshadows

            #pragma vertex vert
            #pragma fragment frag

            #include "MyLighting.cginc"

            ENDCG
        }

        Pass{
            Tags{
                "LightMode"="ShadowCaster"
            }

            CGPROGRAM

            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            #include "MyShadow.cginc"
            
            ENDCG
        }
    }
}
