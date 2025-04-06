Shader "Custom/TerrainTrackShader" {
    Properties {
        _MainTex ("Base Sand Texture", 2D) = "white" {}
        _TrackTex ("Track Texture", 2D) = "white" {}
        _TrackMask ("Track Mask", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _TrackTex;
            sampler2D _TrackMask;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Sample the base sand texture.
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                // Sample the track texture.
                fixed4 trackColor = tex2D(_TrackTex, i.uv);
                // Sample the track mask. (Assume the red channel contains our blend factor.)
                float mask = tex2D(_TrackMask, i.uv).r;
                // Blend the base and track textures.
                fixed4 finalColor = lerp(baseColor, trackColor, mask);
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
