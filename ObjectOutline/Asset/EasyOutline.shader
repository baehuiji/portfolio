Shader "EasyOutline/System"
{
    Properties {
        _MainTex("Texture", 2D)                         = "white" {}
        _IsolatedSceneTex("Isolated Scene Texture", 2D) = "white" {}
        _SceneTex("Scene Texture", 2D)                  = "white" {}
        _SampleCount("Sample Count", Int)               = 9
        _OutlineThickness("Outline Thickness", Float)   = 2.0
        _OutlineColor("Outline Color", Color)           = (1.0, 1.0, 1.0, 1.0)
        _FillColor("Fill Color", Color)                 = (1.0, 1.0, 1.0, 0.5)
        _OutlineMode("Outline Mode", Int)               = 0
        _FillMode("Fill Mode", Int)                     = 0
        _FlipUV("Flip UV", Int)                         = 0
        _OpenGLFix("Open GL Fix", Int)                  = 0
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _IsolatedSceneTex;
            sampler2D _SceneTex;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float4 _FillColor;
            float _OutlineThickness;
            float _SampleCount;
            int _OutlineMode;
            int _FillMode;
            int _FlipUV;
            int _OpenGLFix;

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
                o.uv = UnityStereoTransformScreenSpaceTex(v.uv);
                return o;
            }

            float processDepth(sampler2D depth, float2 uv) {
                if (_FlipUV) { uv.y = 1.0 - uv.y; }
                float col = tex2D(depth, uv).r;
                if (_OpenGLFix) { col = 1.0 - col; }
                return col;
            }

            float getPaintedTex(sampler2D tex, float2 uv) {
                float4 col = tex2D(tex, uv);
                if (col.r != 0.0) {
                    return 1.0;
                }
                return 0.0;
            }

            float getPaintedColor(float col) {
                return col;
            }

            float subtractForeground(float isolatedDepth, float sceneDepth) {
                if (sceneDepth > isolatedDepth) {
                    return 0.0;
                }
                return isolatedDepth;
            }

            float occludeFill(sampler2D isolatedTex, sampler2D nonIsolatedTex, float2 uv) {
                return subtractForeground(processDepth(isolatedTex, uv), processDepth(nonIsolatedTex, uv));
            }

            float blurOccludedFill(sampler2D isolatedTex, sampler2D nonIsolatedTex, float2 uv) {
                float2 resolution = float2(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                float depth = 0.0;
                for(int j = 0; j < _SampleCount; j++) {
                    float angle = 6.28319 * j / _SampleCount;
                    float2 direction = float2(cos(angle), sin(angle));
                    float2 off1 = float2(_OutlineThickness, _OutlineThickness) * direction; 
                    depth = max(depth, occludeFill(isolatedTex, nonIsolatedTex, uv + (off1 / resolution)));
                }
                float origDepth = occludeFill(isolatedTex, nonIsolatedTex, uv);
                if (origDepth == 0.0) {
                    return depth;
                }
                return origDepth;
            }

            float getBlurred(sampler2D tex, float2 uv, float thicknessModifier = 0.0) {
                float2 resolution = float2(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                float depth = 0.0;
                for(int j = 0; j < _SampleCount; j++) {
                    float angle = 6.28319 * j / _SampleCount;
                    float2 direction = float2(cos(angle), sin(angle));
                    float distance = _OutlineThickness + thicknessModifier;
                    float2 off1 = direction * distance;
                    depth = max(depth, processDepth(tex, uv + (off1 / resolution)));
                }
                float origDepth = processDepth(tex, uv);
                if (origDepth == 0.0) {
                    return depth;
                }
                return origDepth;
            }

            float renderOutline(float blurred, float nonBlurred) {
                if (blurred.r != nonBlurred.r) {
                    return 1.0;
                }
                return 0.0;
            }

            float4 clipOutline(float4 blurredIsolated, float4 nonOccludedOutline, float sceneDepth) {
                float4 col = nonOccludedOutline;
                if (blurredIsolated.r < sceneDepth + 0.0) {
                    col = 0.0;
                }
                return col;
            }

            float4 invertFill(float4 culledFill, float4 paintedIsolated) {
                float4 col = float4(0.0, 0.0, 0.0, 1.0);
                if (culledFill.r != paintedIsolated.r) {
                    col = paintedIsolated;
                }
                return col;
            }

            float invertOccludedOutline(float clippedOccludedOutline, float nonOccludedOutline) {
                if (nonOccludedOutline.r != clippedOccludedOutline.r) {
                   return 1.0;
                }
                return 0.0;
            }

            float4 finalOutput(float4 original, float outlineCol, float fillCol) {
                float4 col = original;
                if (fillCol > 0.0) {
                    col = lerp(col, _FillColor, _FillColor.a);
                }
                if (outlineCol > 0.0) {
                    col = lerp(col, _OutlineColor, _OutlineColor.a);
                }
                return col;
            }

            float4 frag(v2f i) : SV_Target {
                float4 color = tex2D(_MainTex, i.uv);
                float isolatedSceneDepth = processDepth(_IsolatedSceneTex, i.uv);
                float sceneDepth = processDepth(_SceneTex, i.uv);
                float fillCol = 0.0;
                float outlineCol = 0.0;
                switch(_OutlineMode) {
                    case 1:
                        outlineCol = renderOutline(getBlurred(_IsolatedSceneTex, i.uv), isolatedSceneDepth);
                        break;
                    case 2:
                        outlineCol = clipOutline(getBlurred(_IsolatedSceneTex, i.uv), renderOutline(getPaintedColor(getBlurred(_IsolatedSceneTex, i.uv)), getPaintedColor(isolatedSceneDepth)), sceneDepth);
                        break;
                    case 3:
                        float outline = renderOutline(getBlurred(_IsolatedSceneTex, i.uv), isolatedSceneDepth);
                        outlineCol = invertOccludedOutline(clipOutline(getBlurred(_IsolatedSceneTex, i.uv, 2.0), outline, sceneDepth), outline);
                        break;
                }
                switch(_FillMode) {
                    case 1:
                        fillCol = getPaintedColor(isolatedSceneDepth);
                        break;
                    case 2:
                        fillCol = occludeFill(_IsolatedSceneTex, _SceneTex, i.uv);
                        break;
                    case 3:
                        fillCol = invertFill(occludeFill(_IsolatedSceneTex, _SceneTex, i.uv), getPaintedColor(isolatedSceneDepth));
                        break;
                }
                return finalOutput(color, outlineCol, fillCol);
            }
            ENDCG
        }
    }
}