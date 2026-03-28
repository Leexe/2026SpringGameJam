Shader "Custom/BoxAudioVisualizer"
{
    Properties
    {
        [Tooltip(The color gradient applied to the visualizer)]
        _ColorRamp ("Color Ramp", 2D) = "white" {}

        [Tooltip(How many bars the visualizer has)]
        _Bars ("Bar Count", Integer) = 64

        [Tooltip(How many transparent the visualizer has)]
        _Opacity ("Opacity", Float) = 0.6
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            CBUFFER_START(UnityPerMaterial)
            float _Bars;
            float _Opacity;
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

            UNITY_DECLARE_TEX2D(_ColorRamp);

            Interpolators vert(MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
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
                int index = (int)(i.uv.x * _Bars);
                float h = _Frequency[index];
                float bar = Bars(i.uv, h);
                float3 col = bar * UNITY_SAMPLE_TEX2D(_ColorRamp, float2(i.uv.y, 0.5)).rgb;
                return float4(col, bar * _Opacity);
            }
            ENDCG
        }
    }
}
