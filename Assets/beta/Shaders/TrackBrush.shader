Shader "Custom/TrackBrush" {
    Properties {
        _BrushTex ("Brush Texture", 2D) = "white" {}
        _BrushPos ("Brush Position", Vector) = (0.5, 0.5, 0, 0)
        _BrushSize ("Brush Size", Float) = 0.1
        _Opacity ("Opacity", Float) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _BrushTex;
            float4 _BrushPos;
            float _BrushSize;
            float _Opacity;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // Calculate distance from brush center (in UV space).
                float2 diff = i.uv - _BrushPos.xy;
                float dist = length(diff);
                // Create a soft edge using smoothstep.
                float brushFactor = smoothstep(_BrushSize, _BrushSize - 0.05, dist);
                // Optionally sample the brush texture.
                float brushSample = tex2D(_BrushTex, i.uv).r;
                // Combine factors and apply opacity.
                float finalAlpha = (1.0 - brushFactor) * _Opacity * brushSample;
                return fixed4(finalAlpha, finalAlpha, finalAlpha, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
