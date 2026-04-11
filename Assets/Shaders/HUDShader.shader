Shader "Custom/HUDShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        //[MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainTexture] _MainTex("HUD Texture (Source Image)", 2D) = "white" {} // _MainTex라는 이름을 써야 유니티 UI 시스템이 Source Image에 넣은 텍스쳐를 자동적으로 줌
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5 // alpha clipping        또한 이름도 _MainTex_ST같이 MainTex로 넣어야 URP의 TRANSFORM_TEX 매크로가 제대로 작동
        
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
            //"CanUseSpriteAtlas"="True"
            //"Queue" = "AlphaTest"
            //"LightMode"="Universal2D"
        }

        Pass
        {
            Name "Unlit"

            Cull Off
            //ZWrite On
            //ZTest LEqual

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma prefer_hlsl_cc
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes // input
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct Varyings // output
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _MainTex_ST;
                half _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes IN) // vertex shader
            {
                Varyings OUT = (Varyings)0;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;



               // Varyings output = (Varyings)0;
                
                // 오브젝트 공간 좌표를 클립 공간 좌표로 변환 (URP 함수 사용), GetVertexPositionInputs 안에 TransformObjectToHClip 있음
                //output.positionCS = GetVertexPositionInputs(IN.positionOS.xyz);
                //output.uv = TRANSFORM_TEX(IN.uv, _BaseMap); // UV 타일링과 오프셋 적용
                
                //return output;
            }

            half4 frag(Varyings IN) : SV_Target // pixel shader
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                if(color.a < _Cutoff)
                {
                    discard;
                }

                return color * IN.color;

                
                // 1. 텍스처 샘플링
                //half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // 2. 색상 곱하기
                //half4 finalColor = texColor * _BaseColor;
                
                // 3. 알파 클리핑
                // finalColor.a가 _Cutoff보다 작으면 픽셀을 버림
                //#if defined(_ALPHATEST_ON) // 성능을 위해 키워드로 제어하는 것이 정석이라고 하나, HUD용이므로 직접 작성
                //    if (finalColor.a < _Cutoff)
                //    {
                //        discard;
                //    }
                //#endif
                
                //return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward/Red"
}
