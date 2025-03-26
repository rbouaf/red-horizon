Shader "Custom/PanelEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0,1)) = 0.5
        _ScanlineCount ("Scanline Count", Float) = 800.0
        _AberrationAmount ("Aberration Amount", Range(0, 0.1)) = 0.02
        _GlitchStrength ("Glitch Strength", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Pass
        {
            // Enable blending for transparency and disable depth writes
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _AberrationAmount;
            float _GlitchStrength;
            // Removed redundant _Time definition; Unity provides _Time automatically

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            // Simple random function based on UV coordinates
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Apply a glitch effect by adding a time-based random offset to UVs
                float glitch = (rand(uv + _Time) - 0.5) * _GlitchStrength;
                uv.x += glitch;
                uv.y += glitch;

                // Chromatic aberration: sample each color channel with slight offsets
                float2 offset = _AberrationAmount * float2(1.0, 0.0);
                fixed red   = tex2D(_MainTex, uv + offset).r;
                fixed green = tex2D(_MainTex, uv).g;
                fixed blue  = tex2D(_MainTex, uv - offset).b;
                // Sample the alpha without offset
                fixed alpha = tex2D(_MainTex, uv).a;
                fixed4 col = fixed4(red, green, blue, alpha);

                // CRT scanlines: use a sine wave based on the vertical UV coordinate
                float scanline = sin(uv.y * _ScanlineCount * 3.14159);
                col.rgb *= lerp(1.0, 1.0 - _ScanlineIntensity, abs(scanline));

                return col;
            }
            ENDCG
        }
    }
}
