Shader "DecentM/VideoPlayer/Screen"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _TargetAspectRatio("Target Aspect Ratio", Float) = 1.7777777
        [Toggle(_)]_IsAVPro("Is AVPro", Int) = 0
        _EmissionStrength("Emission Strength", Float) = 1
        _BackgroundColour("Background Colour", Color) = (0, 0, 0, 1)
        [Toggle(_)]_EnableAntialisaing("Enable Letterbox Antialiasing", Int) = 1
        _AVProContrastOffset("AVPro Contrast Offset", Float) = 2.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityStandardInput.cginc"

            int _IsAVPro;
            float _EmissionStrength;
            float _TargetAspectRatio;
            float4 _MainTex_TexelSize;
            float4 _BackgroundColour;
            int _EnableAntialisaing;
            float _AVProContrastOffset;

            fixed4 VideoTex(float2 uv)
            {
                float2 emissionRes = _MainTex_TexelSize.zw;
                float currentAspectRatio = emissionRes.x / emissionRes.y;
                float visibility = 1.0;

                // If the aspect ratio does not match the target ratio, then we fit the UVs to maintain the aspect ratio while fitting the range 0-1
                if (abs(currentAspectRatio - _TargetAspectRatio) > 0.001)
                {
                    float2 normalizedVideoRes = float2(emissionRes.x / _TargetAspectRatio, emissionRes.y);
                    float2 correctiveScale;

                    // Find which axis is greater, we will clamp to that
                    if (normalizedVideoRes.x > normalizedVideoRes.y)
                        correctiveScale = float2(1, normalizedVideoRes.y / normalizedVideoRes.x);
                    else
                        correctiveScale = float2(normalizedVideoRes.x / normalizedVideoRes.y, 1);

                    uv = ((uv - 0.5) / correctiveScale) + 0.5;

                    if (_EnableAntialisaing)
                    {
                        // Antialiasing on UV clipping
                        float2 uvPadding = (1 / emissionRes) * 0.1;
                        float2 uvfwidth = fwidth(uv.xy);
                        float2 maxFactor = smoothstep(uvfwidth + uvPadding + 1, uvPadding + 1, uv.xy);
                        float2 minFactor = smoothstep(-uvfwidth - uvPadding, -uvPadding, uv.xy);

                        visibility = maxFactor.x * maxFactor.y * minFactor.x * minFactor.y;
                    }
                }

                fixed4 tex = tex2D(_MainTex, _IsAVPro ? float2(uv.x, 1 - uv.y) : uv);

                tex.rgb = tex.rgb * visibility * _EmissionStrength;
                if (_IsAVPro) tex.rgb = pow(tex.rgb, _AVProContrastOffset);

                return tex;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = VideoTex(i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, tex);
                return tex;
            }
            ENDCG
        }
    }
}
