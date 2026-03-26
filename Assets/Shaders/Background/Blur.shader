Shader "Unlit/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100

        CGINCLUDE
        #include "UnityCG.cginc"

        UNITY_DECLARE_TEX2D(_MainTex);
        float4 _MainTex_TexelSize;
        float _BlurSize;
        static const float gaussWeights[5] = { 0.227027, 0.194596, 0.121621, 0.054054, 0.016216 };

        struct MeshData
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Interpolators
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        Interpolators vert(MeshData v)
        {
            Interpolators o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        float4 Blur(float2 direction, float2 uv, float blurSize, float4 texelSize)
        {
            float2 texOffset = direction * texelSize.xy * blurSize;

            // Sample Center Pixel
            float4 result = UNITY_SAMPLE_TEX2D(_MainTex, uv) * gaussWeights[0];

            // Sample Surrounding Pixels
            for (int i = 1; i < 5; i++) {
                result += UNITY_SAMPLE_TEX2D(_MainTex, uv + texOffset * i) * gaussWeights[i];
                result += UNITY_SAMPLE_TEX2D(_MainTex, uv - texOffset * i) * gaussWeights[i];
            }

            return result;
        }
        ENDCG

        // Horizontal Pass
        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(Interpolators i) : SV_Target
            {
                return Blur(float2(1, 0), i.uv, _BlurSize, _MainTex_TexelSize);
            }
            ENDCG
        }

        // Vertical Pass
        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(Interpolators i) : SV_Target
            {
                return Blur(float2(0, 1), i.uv, _BlurSize, _MainTex_TexelSize);
            }
            ENDCG
        }
    }
}
