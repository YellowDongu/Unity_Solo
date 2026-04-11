Shader "Custom/VolumetricTrailFog"
{




    Properties
    {
        [Header(Appearance)]
        _Color ("Cloud Color", Color) = (1, 1, 1, 1)
        _NoiseScale ("Noise Scale", Float) = 3.5
        _Density ("Density Multiplier", Range(0, 50)) = 15.0
        _Threshold ("Density Threshold", Range(0, 1)) = 0.2
        
        [Header(Raymarching)]
        _StepCount ("Step Count", Int) = 24
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Cull Off 
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // [중요] 파티클 인스턴싱을 지원하기 위한 지시자
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:ParticleInstancingSetup
            // Direct3D 11을 사용하는 경우
            #pragma enable_d3d11_debug_symbols
            
            // Direct3D 12를 사용하는 경우
            #pragma enable_d3d12_debug_symbols
            
            // Vulkan/치환된 플랫폼의 경우 (컴파일러 최적화 방지)
            #pragma optimize(off)

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes {
                float3 positionOS : POSITION;  // Position (POSITION.xyz)
                float3 normal     : NORMAL;    // Normal (NORMAL.xyz)
                float4 color      : COLOR;     // Color (COLOR.xyzw)
                float3 centerWS   : TEXCOORD0; // Custom1.xyz (TEXCOORD0.xyz)
                float2 sizeData   : TEXCOORD1; // Size (TEXCOORD1.y)를 읽기 위해 float2로 선언 (x는 더미, y가 실제 Size)
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0; // 픽셀의 월드 좌표
                float3 centerWS   : TEXCOORD1; // 파티클의 월드 중심점
                float  radius     : TEXCOORD2; // 구름의 반지름
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _NoiseScale, _Density, _Threshold;
                int _StepCount;
            CBUFFER_END

            // --- Optimized 3D Noise for Clouds ---
            float hash(float3 p) {
                p = frac(p * float3(443.897, 441.423, 437.195));
                p += dot(p, p.yzx + 19.19);
                return frac((p.x + p.y) * p.z);
            }

            float noise(float3 x) {
                float3 i = floor(x); float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(hash(i), hash(i+float3(1,0,0)), f.x), 
                            lerp(hash(i+float3(0,1,0)), hash(i+float3(1,1,0)), f.x), f.y),
                            lerp(lerp(hash(i+float3(0,0,1)), hash(i+float3(1,0,1)), f.x), 
                            lerp(hash(i+float3(0,1,1)), hash(i+float3(1,1,1)), f.x), f.y), f.z);
            }

Varyings vert(Attributes input) {
    Varyings output;

    // 1. 이미 월드 좌표이므로 그대로 전달
    output.positionWS = input.positionOS;
    output.positionCS = TransformWorldToHClip(output.positionWS);
    
    // 2. 월드 중심점 (Custom1.xyz) 추출
    output.centerWS = input.centerWS.xyz;
    
    // 3. 반지름 추출 (TEXCOORD1.y가 Size라고 하셨으므로)
    // 만약 안 나오면 input.sizeData.x도 테스트해보세요.
    output.radius = input.sizeData.y * 0.5; 
    
    output.color = input.color;
    return output;
}

            float2 rayBoxIntersection(float3 ro, float3 rd) {
                float3 invRd = 1.0 / rd;

                float3 t0 = (-0.5 - ro) * invRd;
                float3 t1 = (0.5 - ro) * invRd;

                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);

                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(min(tmax.x, tmax.y), tmax.z);

                return float2(max(0, dstA), max(0, dstB - max(0, dstA)));
            }

float4 frag(Varyings input) : SV_Target {
    // [데이터 검증용] 만약 아무것도 안 보인다면 아래 주석을 풀어 색상을 확인하세요.
    return float4(input.centerWS * 0.1, 1); // 파티클 위치마다 색이 달라야 함
    return float4(input.radius.xxx, 1);     // 흰색 덩어리가 보여야 함

    float3 ro = GetCameraPositionWS(); // 카메라 월드 좌표
    float3 rd = normalize(input.positionWS - ro); // 카메라에서 픽셀로 향하는 방향

    // --- 월드 공간 Sphere-Ray Intersection ---
    float3 oc = ro - input.centerWS;
    float b = dot(oc, rd);
    float c = dot(oc, oc) - (input.radius * input.radius);
    float h = b * b - c;

    // 구체와 충돌하지 않으면 바로 discard (일직선 문제 해결)
    if (h < 0.0) discard;

    h = sqrt(h);
    float dstA = max(0, -b - h); // 진입점 거리
    float dstB = -b + h;         // 퇴장점 거리
    float dstInside = dstB - dstA;

    // --- 레이마칭 루프 (월드 좌표 기반) ---
    float stepSize = dstInside / 16.0;
    float3 p = ro + rd * (dstA + stepSize * 0.5);
    float alpha = 0;

    for (int i = 0; i < 16; i++) {
        // 월드 좌표 p를 그대로 노이즈 함수에 주입
        // _Time.y를 더해 연기가 피어오르는 효과 추가
        float n = noise(p * _NoiseScale - _Time.y * 0.5);
        
        // 구체 중심에서 멀어질수록 밀도를 낮춤 (부드러운 경계)
        float distToCenter = length(p - input.centerWS) / input.radius;
        float softMask = saturate(1.0 - distToCenter);
        
        float d = saturate(n - _Threshold) * _Density * softMask;
        alpha += d * stepSize * (1.0 - alpha);
        
        if (alpha >= 0.95) break;
        p += rd * stepSize;
    }

    return float4(_Color.rgb * input.color.rgb, alpha * input.color.a);
}

            //float4 frag(Varyings input) : SV_Target {
            //    UNITY_SETUP_INSTANCE_ID(input);
            //
            //    float3 worldRayOrigin = GetCameraPositionWS();
            //    float3 worldRayDir = normalize(input.positionWS - worldRayOrigin);
            //
            //    // [핵심] 인스턴싱된 파티클 각각의 World-To-Object 행렬을 사용
            //    float3 localRayOrigin = mul(unity_WorldToObject, float4(worldRayOrigin, 1.0)).xyz;
            //    float3 localRayDir = normalize(mul((float3x3)unity_WorldToObject, worldRayDir));
            //
            //    float2 rayInfo = rayBoxIntersection(localRayOrigin, localRayDir);
            //    //if (rayInfo.y <= 0) discard;
            //
            //    float stepSize = rayInfo.y / _StepCount;
            //    float3 currentLocalPos = localRayOrigin + localRayDir * (rayInfo.x + stepSize * 0.5);
            //    float accumulatedAlpha = 0;
            //
            //    [loop]
            //    for (int i = 0; i < _StepCount; i++) {
            //        float d = fbm((currentLocalPos + 0.5) * _NoiseScale);
            //        float cloudSample = saturate(d - _Threshold) * _DensityMultiplier;
            //        if (cloudSample > 0) {
            //            accumulatedAlpha += cloudSample * stepSize * (1.0 - accumulatedAlpha);
            //            if (accumulatedAlpha >= 0.95) break;
            //        }
            //        currentLocalPos += localRayDir * stepSize;
            //    }
            //
            //    //return float4(1,1,1, accumulatedAlpha);
            //    return float4(_Color.rgb, 1);
            //    return float4(_Color.rgb, accumulatedAlpha);
            //}


            



//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
//
//#pragma multi_compile _ _ADDITIONAL_LIGHTS
//#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
//
//struct Attributes
//{
//    float4 positionOS : POSITION;
//    float2 uv : TEXCOORD0;   // 안 씀
//    float4 color : COLOR;    // 파티클 색
//};
//
//struct Varyings
//{
//    float4 positionHCS : SV_POSITION;
//    float4 color : COLOR;
//    float2 uv : TEXCOORD0;
//    float3 worldPos : TEXCOORD1;
//};
//
//float3 MyLightingFunction(float3 normalWS, Light light)
//{
//    float NdotL = saturate(dot(normalWS, normalize(light.direction)));
//    return NdotL * light.color * light.distanceAttenuation * light.shadowAttenuation;
//}
//
//half4 frag(Varyings input) : SV_Target
//{
//    InputData inputData = (InputData)0;
//    inputData.positionWS = input.positionWS;
//    inputData.normalWS = input.normalWS;
//    inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
//    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
//
//    float3 lighting = 0;
//
//    // 메인 라이트
//    Light mainLight = GetMainLight();
//    lighting += MyLightingFunction(inputData.normalWS, mainLight);
//
//    // 추가 라이트
//    #if defined(_ADDITIONAL_LIGHTS)
//        #if USE_CLUSTER_LIGHT_LOOP
//            UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
//            {
//                Light additionalLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
//                lighting += MyLightingFunction(inputData.normalWS, additionalLight);
//            }
//        #endif
//
//        uint pixelLightCount = GetAdditionalLightsCount();
//        LIGHT_LOOP_BEGIN(pixelLightCount)
//            Light additionalLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
//            lighting += MyLightingFunction(inputData.normalWS, additionalLight);
//        LIGHT_LOOP_END
//    #endif
//
//    return half4(lighting, 1);
//}



            ENDHLSL
        }
    }
}
