Shader "Xiexe/Additive_GodRay"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[Space]
		[HDR]_Color("Color Tint", Color) = (1,1,1,1)
		[ToggleUI]_UseSunColour("Use Sun Colour", Float) = 1.0
		[Space]
		_PulseSpeed("Pulse Speed", Range(0,2)) = 0
		_FadeStrength("Edge Fade", Range(1,5)) = 1
		_DistFade("Distance Fade", Range(0,1)) = 0.7
		[Space]
		_FadeAmt("Depth Blending", Range(0, 1)) = 0.1
		_FadePow("Depth Blending Power", Range(0, 10)) = 1
		[Space]
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 0
	}
	SubShader
	{
		Tags 
		{ 
			"Queue"="Transparent" 
			"DisableBatching"="True" 
			"ForceNoShadowCasting"="True"
			"IgnoreProjector"="True"
		}
		Blend One One
		Cull[_CullMode]
        //Lighting Off
		ZWrite Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float4 normal : NORMAL;
    			UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			// Dj Lukis LT's method for retrieving linear depth that's correct in mirrors.
			// https://github.com/lukis101/VRCUnityStuffs
			#define PM UNITY_MATRIX_P
			inline float4 CalculateObliqueFrustumCorrection()
			{
				float x1 = -PM._31/(PM._11*PM._34);
				float x2 = -PM._32/(PM._22*PM._34);
				return float4(x1, x2, 0, PM._33/PM._34 + x1*PM._13 + x2*PM._23);
			}
			static float4 ObliqueFrustumCorrection = CalculateObliqueFrustumCorrection();
			inline float CorrectedLinearEyeDepth(float z, float B)
			{
				return 1.0 / (z/PM._34 + B);
			}
			#undef PM

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD2;
				float4 color : TEXCOORD3;
				float4 normal : TEXCOORD4;
				float4 viewDir : TEXCOORD5;
				float4 screenPos : TEXCOORD6;
    			UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex, _CameraDepthTexture;
			float4 _MainTex_ST, _Color;
			float _UseSunColour;
			float _PulseSpeed;
			float _FadeStrength, _FadeAmt, _DistFade, _FadePow;
			
			v2f vert (appdata v)
			{
				v2f o;
    			UNITY_SETUP_INSTANCE_ID(v);
    			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);

		        #if !defined(UNITY_REVERSED_Z)
		        // when using reversed-Z, make the Z be just a tiny
		        // bit above 0.0
		        //o.vertex.z = 1.0e-9f;
		        o.vertex.z = max(o.vertex.z , 1.0e-8f);
		        #else
		        // when not using reversed-Z, make Z/W be just a tiny
		        // bit below 1.0
		        //o.vertex.z = o.vertex.w - 1.0e-6f;
		        o.vertex.z = min(o.vertex.w - 1.0e-5f, o.vertex.z);
		        #endif

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.color = v.color * _Color * lerp(1, _LightColor0, _UseSunColour);
				o.normal = v.normal;
				o.viewDir.xyz = ObjSpaceViewDir(v.vertex);
				o.viewDir.w = dot(o.vertex, ObliqueFrustumCorrection); // Oblique frustrum correction factor
				o.screenPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.screenPos.z);
				UNITY_TRANSFER_FOG(o,o.vertex);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float fadeAmt = 1-(_FadeAmt);
				float sceneZ = CorrectedLinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, 
					UNITY_PROJ_COORD(i.screenPos)), (i.viewDir.w/i.screenPos.w));
				float partZ = i.screenPos.z;
				float depthFade = saturate(fadeAmt * (sceneZ-partZ));
				depthFade = saturate(pow(depthFade, _FadePow));

				depthFade *= (1-LinearEyeDepth(partZ));

				float distFade = saturate(distance(i.worldPos.rgb,_WorldSpaceCameraPos) * _DistFade) ;
				float fade = saturate(abs(dot(normalize(i.normal), normalize(i.viewDir))));
				fade = pow(fade, _FadeStrength);

				float2 uv = i.uv.xy; 
				fixed4 col = tex2D(_MainTex, uv);
				col *= ((sin(_Time.y * _PulseSpeed) * 0.5 + 1));
				col *= i.color;
				col *= col.a;
				col *= fade;
				col *= depthFade;
				col *= distFade;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
				return col;
			}
			ENDCG
		}
	}
}
