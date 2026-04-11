Shader "Custom/RadialFog"
{
    Properties
    {
        _FogColor("Fog Color", Color) = (1, 1, 1, 1)
        _FogStart("Fog Start Distance", Float) = 10
        _FogEnd("Fog End Distance", Float) = 100
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off
        ZTest Always
        Blend Off

        Pass // Blit pass임 PostProcess 역할, 내가 이전에 짜봤던 렌더타깃 기반 안개와 비슷하다.
        {
            Name "RadialFogPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            //===================================================
            //Debug For RenderDoc
            //#pragma enable_d3d11_debug_symbols // RenderDoc Debug - Direct3D 11
            //#pragma enable_d3d12_debug_symbols// RenderDoc Debug - Direct3D 12
            //#pragma optimize(off) // RenderDoc Debug - Vulkan/치환된 플랫폼의 경우 (컴파일러 최적화 방지)
            //===================================================

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;
                float _FogStart;
                float _FogEnd;
            CBUFFER_END

            struct VS_Output
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VS_Output vert(Attributes input)
            {
                VS_Output output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }
            
            float4 frag(VS_Output input) : SV_Target
            {
                float3 rawColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.uv).rgb;// blit 전용
                float depth = SampleSceneDepth(input.uv);
                float ndcZ;

                // exept skybox
                #if defined(UNITY_REVERSED_Z) // reversed Z-buffer에서는 깊이버퍼가 반대라 이거 고려
                    if (depth <= 1e-7)
                        return float4(rawColor, 1.0);
                #else
                    if (depth >= 1.0 - 1e-7)
                        return float4(rawColor, 1.0);
                #endif

                // depth를 NDC Z로 변환
                #if defined(UNITY_REVERSED_Z)
                    ndcZ = depth;
                #else
                    ndcZ = depth * 2.0 - 1.0;
                #endif

                float4 worldPosition = mul(UNITY_MATRIX_I_VP,  float4((input.uv * 2.0 - 1.0), ndcZ, 1.0)/*Clip(NDC) position*/);
                worldPosition.xyz /= worldPosition.w;

                float fogFactor = saturate((distance(worldPosition.xyz, _WorldSpaceCameraPos.xyz) - _FogStart) / (_FogEnd - _FogStart));
                return float4(lerp(rawColor, _FogColor.rgb, fogFactor), 1.0);
            }

            ENDHLSL
        }
    }
}


            //float4 frag(VS_Output input) : SV_Target
            //{
            //    // 1. 현재 화면의 원래 색상 샘플링 (_BlitTexture는 유니티 6 예약어)
            //    float3 rawColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.uv).rgb;
            //
            //    // 2. 깊이 버퍼에서 값 읽기
            //    float depth = SampleSceneDepth(input.uv);
            //    
            //    // 3. 선형 거리(카메라로부터의 수직 거리) 계산
            //    float linearDepth = LinearEyeDepth(depth, _ZBufferParams);
            //
            //    // 4. 안개 계산 (0~1 사이 값)
            //    float fogFactor = saturate((linearDepth - _FogStart) / (_FogEnd - _FogStart));
            //    
            //    // 5. 하늘(Far Plane)은 안개에 영향을 받지 않도록 처리 (옵션)
            //    // 유니티 6 역투영 기준 깊이가 0에 가까우면 하늘임
            //    if (depth <= 1e-7) return float4(rawColor, 1.0);
            //
            //    // 6. 원래 색상과 안개 색상 합성
            //    float3 finalColor = lerp(rawColor, _FogColor.rgb, fogFactor);
            //
            //    return float4(finalColor, 1.0);
            //}