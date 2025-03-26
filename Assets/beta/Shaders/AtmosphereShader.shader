Shader "Custom/AtmosphereRimGlow"
{
    Properties
    {
        _RimColor ("Rim Color", Color) = (0.3, 0.6, 1, 1)
        _RimPower ("Rim Power", Range(1, 10)) = 3
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.3
        _Intensity ("Intensity", Range(0, 5)) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Front              // Render back faces only
        ZWrite Off
        Blend SrcAlpha One      // Additive blending

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            fixed4 _RimColor;
            float _RimPower;
            float _EdgeThreshold;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate view direction.
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                // Rim factor: higher when the surface is nearly perpendicular to the view.
                float rim = 1.0 - saturate(dot(i.worldNormal, viewDir));
                rim = pow(rim, _RimPower);
                
                // Only output glow if the rim factor is above our threshold.
                if(rim < _EdgeThreshold)
                    discard;

                fixed4 col;
                col.rgb = _RimColor.rgb * rim * _Intensity;
                col.a = rim;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}
