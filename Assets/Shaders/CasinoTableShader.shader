Shader "Custom/CasinoTableShader"
{
    Properties
    {
        [Header(Felt Base)]
        _BaseColor ("Table Felt Color", Color) = (0.0, 0.4, 0.15, 1.0)
        _FeltNoiseTex ("Felt Noise (Tiling)", 2D) = "white" {}
        _NoiseStrength ("Fuzziness Strength", Range(0, 1)) = 0.25
        _NoiseScale ("Felt Texture Scale", Float) = 10.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="SimpleLit"
        }
        LOD 100

        // Lit felt: the original unlit colour/noise look, now lit by the URP main light,
        // additional lights (spot/point, including Forward+) and ambient light.
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            // Same lighting keywords as URP's Simple Lit shader.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_FeltNoiseTex);
            SAMPLER(sampler_FeltNoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _FeltNoiseTex_ST;
                half4 _BaseColor;
                half _NoiseStrength;
                float _NoiseScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uvNoise : TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS);

                o.positionCS = positionInputs.positionCS;
                o.positionWS = positionInputs.positionWS;
                o.normalWS = normalInputs.normalWS;
                o.uvNoise = v.uv * _NoiseScale;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // 1. Sample the seamless grayscale noise texture
                half4 noise = SAMPLE_TEXTURE2D(_FeltNoiseTex, sampler_FeltNoiseTex, i.uvNoise);

                // 2. Blend the noise over the base colour (same as the old unlit shader)
                half3 albedo = lerp(_BaseColor.rgb, _BaseColor.rgb * noise.rgb, _NoiseStrength);

                // 3. Light it
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalWS = NormalizeNormalPerPixel(i.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                #if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    inputData.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                #else
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif
                inputData.fogCoord = 0;
                inputData.vertexLighting = half3(0, 0, 0);
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.alpha = 1;
                surfaceData.specular = half3(0, 0, 0); // felt has no shine
                surfaceData.smoothness = 0;
                surfaceData.occlusion = 1;
                surfaceData.normalTS = half3(0, 0, 1);

                half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);
                return half4(color.rgb, 1);
            }
            ENDHLSL
        }
    }
}
