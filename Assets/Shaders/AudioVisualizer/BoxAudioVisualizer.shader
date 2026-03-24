Shader "Custom/BoxAudioVisualizer"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _Bars ("Bar Count", Integer) = 64
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _Bars;
            float _Frequency[256];
            CBUFFER_END

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpolators vert(MeshData v)
            {
                Interpolators o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float Bars(float2 uv, float height)
            {
                float f = frac(uv.x * _Bars);
                return step(uv.y, height) * step(0.1, f) * step(f, 0.9);
            }

            float4 frag(Interpolators i) : SV_Target
            {
                float h = _Frequency[i.uv.x * _Bars];
                float bar = Bars(i.uv, h);
                float3 col = bar * float3(sin(i.uv.x + 0.02), cos(i.uv.y + 2), .3);
                return float4(col, bar);
            }
            ENDHLSL
        }
    }
}
