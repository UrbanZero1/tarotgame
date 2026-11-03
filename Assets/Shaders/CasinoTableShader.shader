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
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv_noise : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _FeltNoiseTex;
            float _NoiseStrength;
            float _NoiseScale;
            fixed4 _BaseColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv_noise = v.uv * _NoiseScale;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Sample the seamless grayscale noise texture
                fixed4 noise = tex2D(_FeltNoiseTex, i.uv_noise);
                
                // 2. Blend the noise over the base color
                fixed4 finalFelt = lerp(_BaseColor, _BaseColor * noise, _NoiseStrength);
                
                return finalFelt;
            }
            ENDCG
        }
    }
}