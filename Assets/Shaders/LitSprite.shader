Shader "Custom/LitSprite"
{
    // A sprite shader that reacts to normal URP 3D lights (main light, additional lights incl.
    // Forward+, and ambient). URP's own Sprite-Lit-Default only reacts to 2D lights, so in the
    // 3D renderer it falls back to unlit.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="SimpleLit"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half4 color : COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);

                o.positionCS = positionInputs.positionCS;
                o.positionWS = positionInputs.positionWS;
                // Sprite meshes have no normals: a sprite faces -Z in its own space, toward the camera.
                o.normalWS = TransformObjectToWorldNormal(float3(0, 0, -1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;

                // Sprites are double sided, so turn the normal toward the viewer on the back face.
                half3 normalWS = NormalizeNormalPerPixel(i.normalWS);
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                if (dot(normalWS, viewDirWS) < 0)
                    normalWS = -normalWS;

                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
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
                surfaceData.albedo = tex.rgb;
                surfaceData.alpha = tex.a;
                surfaceData.specular = half3(0, 0, 0);
                surfaceData.smoothness = 0;
                surfaceData.occlusion = 1;
                surfaceData.normalTS = half3(0, 0, 1);

                half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);
                return half4(color.rgb, tex.a);
            }
            ENDHLSL
        }
    }
}
