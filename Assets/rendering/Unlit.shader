Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Tint("Tint",Color)=(1,1,1,1)
        _MainTex("Albedo",2D)="white"{}
        [Gamma] _Metallic("Metallic",Range(0,1))=0
        _Smoothness("Smoothness",Range(0,1))=0.5
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
            #pragma target 3.5
            
            #pragma multi_compile _ VERTEXLIGHT_ON
            
            #pragma vertex vert
            #pragma fragment frag

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

            #pragma multi_compile_fwdadd

            #pragma vertex vert
            #pragma fragment frag

            #include "MyLighting.cginc"

            ENDCG
        }
    }
}
