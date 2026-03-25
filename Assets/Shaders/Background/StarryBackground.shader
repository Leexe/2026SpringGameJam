Shader "Unlit/StarryBackground"
{
    Properties
    {
        _PixelResolution ("Pixel Resolution", Range(1.0, 2048.0)) = 128.0
        _Octaves ("Octaves", Integer) = 10
        _FogScale ("Fog Scale", Float) = 1
        _FogSpeed ("Fog Speed", Float) = 0.5
        _Color1 ("Color 1", Color) = (1,1,1,1)
        _Color2 ("Color 2", Color) = (0,0,0,1)
        _FogColorRamp ("Fog Color Ramp", 2D) = "white" {}
        _StarGrid ("Star Grid", Range(1, 1000)) = 700.0
        _StarSize ("Star Scale", Range(0.0, 1.0)) = 0.3
        _StarProbability ("Star Probability", Range(0.0, 1.0)) = 0.02
        _StarSpeed ("Star Speed", Float) = 3
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/Utility/Fbm.hlsl"
            #include "Assets/Shaders/Utility/keijiro/SimplexNoise2D.hlsl"

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

            UNITY_DECLARE_TEX2D(_FogColorRamp);
            float _PixelResolution;
            int _Octaves;
            float _StarGrid;
            float _StarSize;
            float _StarProbability;
            float4 _Color1;
            float4 _Color2;
            float _FogScale;
            float _FogSpeed;
            float _StarSpeed;

            Interpolators vert(MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(Interpolators i) : SV_Target
            {
                // Pixelation
                float2 pixelUV = floor(i.uv * _PixelResolution) / _PixelResolution;

                // FBM Fog
                float offset = fbm(float3(_FogScale * pixelUV, _Time.y * _FogSpeed), _Octaves);
                float fogNoise = fbm(float3(pixelUV + offset, _Time.y * _FogSpeed), _Octaves);
                // float3 fbmColor = lerp(_Color1.rgb, _Color2.rgb, f);
                // Sample Color From Color Ramp
                float3 fbmColor = UNITY_SAMPLE_TEX2D(_FogColorRamp, float2(fogNoise, 0.5)).rgb;

                // Stars
                // 1) Create Grid
                float2 starGrid = floor(pixelUV * _StarGrid);
                // 2) Rescale Simplex Noise to be in range [0, 1]
                float randomStar = SimplexNoise(starGrid * 0.314 + 0.5) * 0.5 + 0.5;
                // 3) Offset the center of the star within each cell
                float offsetX = SimplexNoise(starGrid * 0.314 + float2(13.5, 0.0)) * 0.4;
                float offsetY = SimplexNoise(starGrid * 0.314 + float2(0.0, 42.7)) * 0.4;
                // 4) Get the local position of the star within the cell
                float2 starLocal = frac(pixelUV * _StarGrid) - 0.5 - float2(offsetX, offsetY);
                // 5) Draw a small dot for star
                float starShape = smoothstep(_StarSize, 0.0, length(starLocal));
                float star = starShape * step(1.0 - _StarProbability, randomStar);
                // 6) Make stars twinkle
                float twinkle = sin(_Time.y * _StarSpeed + randomStar * 1000.0) * 0.5 + 0.5;
                float3 stars = star * twinkle;

                return float4(fbmColor + stars, 1.0);
            }
            ENDCG
        }
    }
}
