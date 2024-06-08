// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//https://forum.unity.com/threads/help-to-find-an-asset-solution.755273/#post-5246960
Shader "Custom/BackObjectsOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", Range(0, 5)) = 1
        _RefNumber("Reference number", Int) = 255
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            //LOD 100
            //ZWrite Off
            //Blend OneMinusDstAlpha DstAlpha
            //ColorMask RGB

            // ZTest[unity_GUIZTestMode] // can only see thru portal & shader ZTest mode : unity_GUIZTestMode
            ZTest Always // can only see thru portal & always show on the bottom 
            LOD 200
            ZWrite Off
            Blend OneMinusDstAlpha DstAlpha
            ColorMask RGB
            //Cull Off // for two-sides rendering

            Stencil {
                Ref[_RefNumber]
                Comp equal
            }

            CGINCLUDE
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _OutlineColor;
            float _OutlineWidth;

            struct v2fOutline
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2fOutline vertOutline(appdata_base v, float2 offset)
            {
                v2fOutline o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos.xy += offset * 2 * o.pos.w * _OutlineWidth / _ScreenParams.xy;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 fragOutlineMask(v2fOutline i) : SV_Target
            {
                fixed alpha = tex2D(_MainTex, i.uv).a;
                fixed4 col = _OutlineColor;
                col.a *= alpha;
                return 1 - col.a;
            }

            fixed4 fragOutline(v2fOutline i) : SV_Target
            {
                fixed alpha = tex2D(_MainTex, i.uv).a;
                fixed4 col = _OutlineColor;
                col.a *= alpha;
                return col;
            }
            ENDCG

            Pass
            {
                Name "OUTLINEALPHA"
                BlendOp Min
                ColorMask A

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment fragOutlineMask

                v2fOutline vert(appdata_base v)
                {
                    return vertOutline(v, float2(1, 1));
                }
                ENDCG
            }

            Pass
            {
                Name "OUTLINEALPHA"
                BlendOp Min
                ColorMask A

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment fragOutlineMask

                v2fOutline vert(appdata_base v)
                {
                    return vertOutline(v, float2(-1, 1));
                }
                ENDCG
            }

            Pass
            {
                Name "OUTLINEALPHA"
                BlendOp Min
                ColorMask A

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment fragOutlineMask

                v2fOutline vert(appdata_base v)
                {
                    return vertOutline(v, float2(1,-1));
                }
                ENDCG
            }

            Pass
            {
                Name "OUTLINEALPHA"
                BlendOp Min
                ColorMask A

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment fragOutlineMask

                v2fOutline vert(appdata_base v)
                {
                    return vertOutline(v, float2(-1,-1));
                }
                ENDCG
            }

            Pass
            {
                Name "CENTERMASK"
                ColorMask A
                Blend Zero One, One OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata_full v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    return tex2D(_MainTex, i.uv);
                }
                ENDCG
            }

            Pass
            {
                Name "OUTLINECOLOR"
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment fragOutline

                v2fOutline vert(appdata_base v)
                {
                    return vertOutline(v, float2(1, 1));
                }
                ENDCG
            }

            Pass
            {
                Name "OUTLINECOLOR"
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment fragOutline

                v2fOutline vert(appdata_base v)
                {
                    return vertOutline(v, float2(-1, 1));
                }
                ENDCG
            }

            Pass
            {
                Name "OUTLINECOLOR"
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment fragOutline

                v2fOutline vert(appdata_base v)
                {
                    return vertOutline(v, float2(1,-1));
                }
                ENDCG
            }

            Pass
            {
                Name "OUTLINECOLOR"
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment fragOutline

                v2fOutline vert(appdata_base v)
                {
                    return vertOutline(v, float2(-1,-1));
                }
                ENDCG
            }
        }
}