Shader "EasyOutline/RenderDepth"
{
	Properties {
		_MainTex("Texture", 2D) = "white" {}
        _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
	}
	SubShader {
        Tags {
            "RenderType"="Transparent"
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "PerformanceChecks"="False"
		}
        Cull Off
        Lighting Off

        CGINCLUDE
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _AlphaCutoff;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 projPos : TEXCOORD1;
				float4 color : COLOR;
            };

            v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.color = v.color;
                o.projPos = ComputeScreenPos(o.vertex);
                return o;
            }
        ENDCG

		Pass {
            BlendOp Max
            CGPROGRAM
            #pragma fragment frag
            #pragma vertex vert
            
            float4 frag(v2f i) : SV_Target {
                if (tex2D(_MainTex, i.uv).a < _AlphaCutoff) { return float4(0.0, 0.0, 0.0, 0.0); }
                return i.projPos.z;
            }
            ENDCG
		}

		Pass {
            BlendOp Min
            CGPROGRAM
            #pragma fragment frag
            #pragma vertex vert

            float4 frag(v2f i) : SV_Target {
                if (tex2D(_MainTex, i.uv).a < _AlphaCutoff) { return float4(1.0, 1.0, 1.0, 0.0); }
                return i.vertex.z;
            }
            ENDCG
		}
	}
}