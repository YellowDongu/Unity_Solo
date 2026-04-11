
#ifndef VOLUMETRIC_FOG_INCLUDED
#define VOLUMETRIC_FOG_INCLUDED

float sdCappedCone(float3 pos, float3 start, float3 end, float startRadius, float endRadius)
{
    float3 ba = end - start;
    float3 pa = pos - start;
    float l2 = dot(ba, ba);
    float h = saturate(dot(pa, ba) / l2); // 선분 위의 0~1 사이 위치
    float r = lerp(startRadius, endRadius, h); // 위치(높이)에 따른 반지름 선형 보간
    
    return length(pa - ba * h) - r; // 현재 지점에서 선분까지의 최단 거리 - 보간된 반지름
}

bool RaySphereIntersection(float3 rayOrigin, float3 rayDir, float3 spherePos, float radius, out float t0, out float t1)
{
    float3 L = rayOrigin - spherePos;
    float a = dot(rayDir, rayDir);
    float b = 2.0 * dot(rayDir, L);
    float c = dot(L, L) - radius * radius;
    float delta = b * b - 4.0 * a * c;
    
    if (delta < 0) return false;

    float sqrtDelta = sqrt(delta);
    t0 = (-b - sqrtDelta) / (2.0 * a);
    t1 = (-b + sqrtDelta) / (2.0 * a);

    if (t1 < 0) return false;
    t0 = max(t0, 0); // 카메라가 구체 안에 있을 때 처리
    return true;
}

// Ray-Box Intersection (Slab Method)
bool RayBoxIntersection(float3 rayOrigin, float3 rayDir, float3 boxPos, float3 boxSize, out float tMin, out float tMax)
{
    float3 invDir = 1.0 / rayDir;
    
    // 박스의 최소/최대 점 계산
    float3 boxMin = boxPos - boxSize;
    float3 boxMax = boxPos + boxSize;

    // 각 축(x, y, z)별로 입구와 출구 거리 계산
    float3 t0 = (boxMin - rayOrigin) * invDir;
    float3 t1 = (boxMax - rayOrigin) * invDir;

    // invDir이 음수일 경우를 대비해 작은 값과 큰 값을 정렬
    float3 tNear = min(t0, t1);
    float3 tFar = max(t0, t1);

    // 전체 구간 중 가장 늦게 들어오는 곳(tMin)과 가장 먼저 나가는 곳(tMax) 결정
    tMin = max(max(tNear.x, tNear.y), tNear.z);
    tMax = min(min(tFar.x, tFar.y), tFar.z);

    // tMin <= tMax이면 충돌한 것이며, 박스가 카메라 뒤에 있지 않아야 함(tMax > 0)
    return tMin <= tMax && tMax > 0;
}


// 박스 형태의 SDF (Signed Distance Field)
float sdBox(float3 p, float3 b)
{
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}

float sdRoundBox(float3 p, float3 b, float r)
{
    // 원리는 기존 박스 크기(b)에서 반지름(r)을 뺀 영역을 계산하고,
    // 그 결과에 r을 다시 빼주는 방식입니다.
    float3 q = abs(p) - (b - r);
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0) - r;
}

// 입체감을 위한 Noise (간단한 3D Noise 예시)
float SimpleNoise(float3 p)
{
    return frac(sin(dot(p, float3(12.9898, 78.233, 45.164))) * 43758.5453);
}
float GradientNoise(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    f = f * f * (3.0 - 2.0 * f); // Hermite Interpolation (부드러운 보간)

    // 주변 8개 정점의 랜덤값 혼합
    float n = i.x + i.y * 157.0 + 113.0 * i.z;
    return lerp(lerp(lerp(frac(sin(n + 0.0) * 43758.5453), frac(sin(n + 1.0) * 43758.5453), f.x),
                   lerp(frac(sin(n + 157.0) * 43758.5453), frac(sin(n + 158.0) * 43758.5453), f.x), f.y),
               lerp(lerp(frac(sin(n + 113.0) * 43758.5453), frac(sin(n + 114.0) * 43758.5453), f.x),
                   lerp(frac(sin(n + 270.0) * 43758.5453), frac(sin(n + 271.0) * 43758.5453), f.x), f.y), f.z);
}

// ==========================================
// 핵심 레이마칭 함수
// ==========================================

// 핵심 레이마칭 함수 요약
float4 Raymarch(float3 rayOrigin, float3 rayDir, float3 spherePos, float radius, float density, float3 cloudColor)
{
    float t0, t1;
    if (!RaySphereIntersection(rayOrigin, rayDir, spherePos, radius, t0, t1)) 
        return 0; // 구체와 안 부딪히면 통과

    float3 p = rayOrigin + rayDir * t0;
    float stepSize = (t1 - t0) / 32.0; // 32단계 샘플링
    float totalDensity = 0;

    for (int i = 0; i < 32; i++)
    {
        float distToCenter = length(p - spherePos);
        if (distToCenter < radius)
        {
            // 중심에 가까울수록 밀도가 높아지는 공식
            float localDensity = (1.0 - distToCenter / radius) * density;
            totalDensity += localDensity * stepSize;
        }
        p += rayDir * stepSize;
    }

    float transmittance = exp(-totalDensity); // Beer's Law 적용
    return float4(cloudColor, 1.0 - transmittance); // 배경과 섞일 알파값 반환
}

float4 RaymarchVolume(float3 ro, float3 rd, float3 spherePos, float radius, float density, float scattering, float4 cloudColor)
{
    float t0, t1;
    if (!RaySphereIntersection(ro, rd, spherePos, radius, t0, t1)) 
        return float4(0, 0, 0, 0);

    const int numSteps = 32;
    float stepSize = (t1 - t0) / (float) numSteps;
    
    float3 p = ro + rd * t0;
    
    // --- [수정 포인트 1: 누적 변수 선언] ---
    float3 accumulatedColor = 0; // 누적할 빛의 양
    float transmittance = 1.0; // 남은 빛의 투과율 (1.0 = 완전 투명)

    for (int i = 0; i < numSteps; i++)
    {
        float distToCenter = length(p - spherePos);
        
        if (distToCenter < radius)
        {
            // 중심에 가까울수록 밀도가 높아지는 기본 공식
            float normalizedDist = 1.0 - saturate(distToCenter / radius);
            
            // --- [수정 포인트 2: 노이즈 한 줄 추가 (선택 사항)] ---
            // float noiseVal = SimpleNoise(p * 2.0 + _Time.y); // 움직이는 노이즈
            // normalizedDist *= noiseVal;

            // 현재 지점에서의 밀도와 흡수량(alpha)
            float localDensity = normalizedDist * density;
            float alpha = saturate(localDensity * stepSize);

            // --- [수정 포인트 3: 루프 내 색상 누적] ---
            // 현재 지점의 입자가 산란시키는 빛의 양을 더합니다.
            // (남은 투과율만큼만 뒤의 색이 보임)
            accumulatedColor += cloudColor.rgb * scattering * alpha * transmittance;
            
            // 빛이 입자를 통과하며 감쇄됨
            transmittance *= (1.0 - alpha);

            // [최적화] 투과율이 거의 0이면(완전 불투명) 루프를 일찍 종료
            if (transmittance < 0.01)
                break;
        }
        
        p += rd * stepSize;
    }

    // 최종 결과: 누적된 RGB와 전체 차폐도(Alpha) 반환
    return float4(accumulatedColor, 1.0 - transmittance);
}


float4 RaymarchVolumeWithNoise(float3 rayOrigin, float3 rayDir, float3 spherePos, float radius, float density, float scattering, float4 cloudColor)
{
    float t0, t1;
    if (!RaySphereIntersection(rayOrigin, rayDir, spherePos, radius, t0, t1)) 
        return float4(0, 0, 0, 0);

    const int numSteps = 32;
    float stepSize = (t1 - t0) / (float) numSteps;
    
    float3 p = rayOrigin + rayDir * t0;
    
    // --- [수정 포인트 1: 누적 변수 선언] ---
    float3 accumulatedColor = 0; // 누적할 빛의 양
    float transmittance = 1.0; // 남은 빛의 투과율 (1.0 = 완전 투명)

    for (int i = 0; i < numSteps; i++)
    {
        float distToCenter = length(p - spherePos);
        
        if (distToCenter < radius)
        {
            // 중심에 가까울수록 밀도가 높아지는 기본 공식
            float normalizedDist = 1.0 - saturate(distToCenter / radius);
            normalizedDist = pow(normalizedDist, 2.0);
            
            
            // --- [수정 포인트 2: 노이즈 한 줄 추가 (선택 사항)] ---
            // 레이마칭 루프 내부 적용
            //float noiseVal = GradientNoise(p * 0.5 + _Time.y * 0.2);
            float noiseVal = GradientNoise(p * 0.5);
            // 0~1 범위를 좀 더 다이내믹하게 조정 (Contrast 조절)
            noiseVal = saturate(noiseVal * 1.5 - 0.2);
            normalizedDist *= noiseVal;
            
            
            
            //float noiseVal = SimpleNoise(p * 2.0); // 움직이는 노이즈
            //normalizedDist *= noiseVal;

            // 현재 지점에서의 밀도와 흡수량(alpha)
            float localDensity = normalizedDist * density;
            float alpha = saturate(localDensity * stepSize);

            // --- [수정 포인트 3: 루프 내 색상 누적] ---
            // 현재 지점의 입자가 산란시키는 빛의 양을 더합니다.
            // (남은 투과율만큼만 뒤의 색이 보임)
            
            float3 currentParticleColor = lerp(cloudColor.rgb, float3(1, 1, 1), pow(1.0 - transmittance, 1.0));
            
            //float3 currentParticleColor = lerp(float3(1, 1, 1), cloudColor.rgb, 1.0 - transmittance);
            
            //accumulatedColor += currentParticleColor * scattering * alpha * transmittance;
            accumulatedColor += currentParticleColor * scattering * transmittance;
            
            //accumulatedColor += cloudColor.rgb * scattering * alpha * transmittance;
            
            // 빛이 입자를 통과하며 감쇄됨
            transmittance *= (1.0 - alpha);

            // [최적화] 투과율이 거의 0이면(완전 불투명) 루프를 일찍 종료
            if (transmittance < 0.01)
                break;
        }
        
        p += rayDir * stepSize;
    }

    // 최종 결과: 누적된 RGB와 전체 차폐도(Alpha) 반환
    return float4(accumulatedColor, 1.0 - transmittance);
}

float4 RaymarchVolume(float3 ro, float3 rd, float3 spherePos, float radius, float density, float scattering, float4 cloudColor, float2 _Time)
{
    float t0, t1;
    if (!RaySphereIntersection(ro, rd, spherePos, radius, t0, t1)) 
        return float4(0, 0, 0, 0);

    const int numSteps = 32;
    float stepSize = (t1 - t0) / (float) numSteps;
    
    float3 p = ro + rd * t0;
    
    // --- [수정 포인트 1: 누적 변수 선언] ---
    float3 accumulatedColor = 0; // 누적할 빛의 양
    float transmittance = 1.0; // 남은 빛의 투과율 (1.0 = 완전 투명)

    for (int i = 0; i < numSteps; i++)
    {
        float distToCenter = length(p - spherePos);
        
        if (distToCenter < radius)
        {
            // 중심에 가까울수록 밀도가 높아지는 기본 공식
            float normalizedDist = 1.0 - saturate(distToCenter / radius);
            
            // --- [수정 포인트 2: 노이즈 한 줄 추가 (선택 사항)] ---
            float noiseVal = SimpleNoise(p * 2.0 + _Time.y); // 움직이는 노이즈
            normalizedDist *= noiseVal;

            // 현재 지점에서의 밀도와 흡수량(alpha)
            float localDensity = normalizedDist * density;
            float alpha = saturate(localDensity * stepSize);

            // --- [수정 포인트 3: 루프 내 색상 누적] ---
            // 현재 지점의 입자가 산란시키는 빛의 양을 더합니다.
            // (남은 투과율만큼만 뒤의 색이 보임)
            accumulatedColor += cloudColor.rgb * scattering * alpha * transmittance;
            
            // 빛이 입자를 통과하며 감쇄됨
            transmittance *= (1.0 - alpha);

            // [최적화] 투과율이 거의 0이면(완전 불투명) 루프를 일찍 종료
            if (transmittance < 0.01)
                break;
        }
        
        p += rd * stepSize;
    }

    // 최종 결과: 누적된 RGB와 전체 차폐도(Alpha) 반환
    return float4(accumulatedColor, 1.0 - transmittance);
}
//float4 RaymarchVolume(float3 ro, float3 rd, float3 spherePos, float radius, float density, float scattering, float4 cloudColor)
//{
//    float t0, t1;
//    if (!RaySphereIntersection(ro, rd, spherePos, radius, t0, t1)) 
//        return float4(0, 0, 0, 0); // 안 부딪히면 투명하게 pass
//
//    // 2. 샘플링 설정 (성능과 퀄리티의 타협점)
//    const int numSteps = 32; // DX11 엔진 하실 때처럼 이 값을 조절하며 최적화하세요.
//    float stepSize = (t1 - t0) / (float) numSteps;
//    
//    float3 p = ro + rd * t0; // 광선의 시작점 설정
//    float totalDensity = 0;
//    
//    // 3. 레이마칭 루프 시작
//    for (int i = 0; i < numSteps; i++)
//    {
//        float distToCenter = length(p - spherePos);
//        
//        // 4. 구체 내부에 있을 때만 밀도 적분
//        if (distToCenter < radius)
//        {
//            // 중심에 가까울수록 밀도가 높아지는 공식 (Soft Edge 구현)
//            float normalizedDist = 1.0 - saturate(distToCenter / radius);
//            
//            // 여기에 나중에 Noise를 곱해서 구름 질감을 낼 수 있습니다.
//            // float noiseVal = SimpleNoise(p * scale);
//            // normalizedDist *= noiseVal;
//
//            float localDensity = normalizedDist * density;
//            totalDensity += localDensity * stepSize;
//        }
//        
//        // 5. 다음 샘플링 지점으로 이동
//        p += rd * stepSize;
//    }
//
//    // 6. 물리 기반 빛의 차폐 계산 (Beer's Law)
//    // 투과율(Transmittance) 계산: 0에 가까울수록 빛이 차단됨
//    float transmittance = exp(-totalDensity);
//    
//    // 7. 산란광(Scattering) 표현: 투과율의 역수를 이용해 뽀얀 느낌 추가
//    // 텍스처 없이 "구름값에 가까워지는" 핵심 로직입니다.
//    float scatterValue = 1.0 - transmittance;
//    
//    // 최종 색상 = 구름 색상 * 산란 세기 (알파값은 차폐된 정도)
//    float3 finalColor = cloudColor.rgb * scatterValue * scattering;
//    
//    // 최종 결과 반환 (RGB 색상, A는 불투명도)
//    return float4(finalColor, scatterValue);
//}


//float4 RaymarchVolume_BOX(float3 rayOrigin, float3 rayDir, float3 boxPos, float3 boxSize, float scattering, float boxRoundness, float density, float4 cloudColor)
//{
//    float tNear, tFar;
//    
//    // 1. 박스와 부딪히지 않았다면 즉시 탈출 (Early Exit)
//    if (!RayBoxIntersection(rayOrigin, rayDir, boxPos, boxSize, tNear, tFar))
//    {
//        return float4(0, 0, 0, 0);
//    }
//    
//    //if (tNear < tFar)
//    //    return float4(1, 0, 0, 1);
//    
//    // 2. 샘플링 구간 최적화
//    // 카메라가 박스 안에 있을 경우 tNear가 음수일 수 있으므로 0으로 보정
//    float tStart = max(0.0, tNear);
//    float tEnd = tFar;
//
//    // [Optional] 씬의 깊이(벽)에 가려지는 처리
//    // float sceneDepth = SampleSceneDepth(uv);
//    // tEnd = min(tEnd, sceneDepth);
//    // if (tStart >= tEnd) return float4(0,0,0,0);
//        
//    const int numSteps = 32;
//    float stepSize = (tEnd - tStart) / (float) numSteps;
//    float3 p = rayOrigin + rayDir * (tStart + stepSize * 0.5); // 미세 오프셋 추가
//
//    float3 accumulatedColor = 0;
//    float transmittance = 1.0;
//    for (int i = 0; i < numSteps; i++)
//    {
//        // 박스 로컬 공간으로 변환
//        float3 localP = p - boxPos;
//        float d = sdRoundBox(localP, boxSize, boxRoundness);
//
//        if (d < 0.0) // 박스 내부에 있다면
//        {
//            float edgeSoftness = 0.5; // 외곽을 얼마나 부드럽게 깎을지
//            float distRatio = saturate(abs(d) / edgeSoftness);
//            
//            float localDensity = distRatio * density * GradientNoise(p * 0.5);
//            //float localDensity = distRatio * density * GradientNoise(p * 0.5 + _Time.y);
//            
//            
//            float alpha = saturate(localDensity * stepSize);
//            accumulatedColor += cloudColor.rgb * scattering * alpha * transmittance;
//            transmittance *= (1.0 - alpha);
//
//            // [최적화] 투과율이 거의 0이면(완전 불투명) 루프를 일찍 종료
//            if (transmittance < 0.01)
//                break;
//            
//        }
//        
//        p += rayDir * stepSize;
//    }
//    
//    return float4(accumulatedColor, 1.0 - transmittance);
//}



#endif


