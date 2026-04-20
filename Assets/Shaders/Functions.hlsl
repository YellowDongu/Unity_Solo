#ifndef pch_INCLUDED
#define pch_INCLUDED

float3 gradiant(int hash, float3 p)  // 그래디언트 벡터 선택 (12방향 — 정육면체 모서리쪽)
{
    // Ken Perlin 원본은 float 반환이지만 3D 확장을 위해 float3으로 래핑 후 dot() 사용한 버전이 아래임
    int h = hash & 15;
    float u = h < 8 ? p.x : p.y;
    float v = h < 4 ? p.y : (h == 12 || h == 14 ? p.x : p.z);
    return float3((h & 1) == 0 ? u : -u, (h & 2) == 0 ? v : -v, 0);
}

float gradDot(int hash, float3 diff)// 내적 기반 그래디언트 기여
{
    int h = hash & 15;
    float u = h < 8 ? diff.x : diff.y;
    float v = h < 4 ? diff.y : (h == 12 || h == 14 ? diff.x : diff.z);
    return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
}

// 6차 페이드 함수 (격자 경계를 부드럽게 만들기 위해 2차 미분 연속)
inline float3 fade(float3 t) { return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }

float perlinNoise(float3 p)
{
    static const int perm[512] = // 256개 순열 테이블 (Ken Perlin 원본 기반)
    {
    151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
    140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
    247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
    57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
    74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
    60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
    65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
    200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
    52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
    207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
    119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
    129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
    218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
    81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
    184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
    222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
    
    151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
    140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
    247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
    57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
    74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
    60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
    65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
    200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
    52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
    207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
    119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
    129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
    218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
    81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
    184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
    222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };
    
    int xi = (int) floor(p.x) & 255; // 격자 셀 정수 좌표
    int yi = (int) floor(p.y) & 255;
    int zi = (int) floor(p.z) & 255;
    
    float xf = frac(p.x); // 셀 내 소수 좌표
    float yf = frac(p.y);
    float zf = frac(p.z);
    
    float3 f = fade(float3(xf, yf, zf)); // 페이드 (6차 에르미트 보간)
    
    int aaa = perm[perm[perm[xi] + yi] + zi]; // 8개 코너 해시
    int aba = perm[perm[perm[xi] + yi + 1] + zi];
    int aab = perm[perm[perm[xi] + yi] + zi + 1];
    int abb = perm[perm[perm[xi] + yi + 1] + zi + 1];
    int baa = perm[perm[perm[xi + 1] + yi] + zi];
    int bba = perm[perm[perm[xi + 1] + yi + 1] + zi];
    int bab = perm[perm[perm[xi + 1] + yi] + zi + 1];
    int bbb = perm[perm[perm[xi + 1] + yi + 1] + zi + 1];

    // 8코너 그래디언트 내적 -> 삼선형 보간
    float x1, x2, y1, y2;
    x1 = lerp(gradDot(aaa, float3(xf, yf, zf)), gradDot(baa, float3(xf - 1.0, yf, zf)), f.x);
    x2 = lerp(gradDot(aba, float3(xf, yf - 1.0, zf)), gradDot(bba, float3(xf - 1.0, yf - 1.0, zf)), f.x);
    y1 = lerp(x1, x2, f.y);

    x1 = lerp(gradDot(aab, float3(xf, yf, zf - 1.0)), gradDot(bab, float3(xf - 1.0, yf, zf - 1.0)), f.x);
    x2 = lerp(gradDot(abb, float3(xf, yf - 1.0, zf - 1.0)), gradDot(bbb, float3(xf - 1.0, yf - 1.0, zf - 1.0)), f.x);
    y2 = lerp(x1, x2, f.y);

                
    return (lerp(y1, y2, f.z) + 1.0) * 0.5; // 01 정규화까지 포함, 안하면 그냥 lerp(y1, y2, f.z)으로 해서 -1~1로 나옴
}

inline float Hash(float n) { return frac(sin(n) * 43758.5453123); } // 실시간 3D 노이즈 함수 (Perlin/Pseudo-random 기반)
            
float Noise(float3 x)
{
    float3 p = floor(x);
    float3 f = frac(x);
    float n = p.x + p.y * 57.0 + 113.0 * p.z;
    f = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(lerp(Hash(n + 0.0), Hash(n + 1.0), f.x), lerp(Hash(n + 57.0), Hash(n + 58.0), f.x), f.y), lerp(lerp(Hash(n + 113.0), Hash(n + 114.0), f.x), lerp(Hash(n + 170.0), Hash(n + 171.0), f.x), f.y), f.z);
}

// Ray-Box Intersection
float2 RayBoxIntersection(float3 rayOrigin, float3 rayDirection)
{
    float3 invRaydirection = 1.0 / rayDirection;
    float3 t0 = (-0.5 - rayOrigin) * invRaydirection;
    float3 t1 = (0.5 - rayOrigin) * invRaydirection;

    float3 tmin = min(t0, t1);
    float3 tmax = max(t0, t1);

    float dstA = max(max(tmin.x, tmin.y), tmin.z);
    float dstB = min(min(tmax.x, tmax.y), tmax.z);

    return float2(max(0, dstA), max(0, dstB - max(0, dstA)));
}
// Slab Method
bool RayBoxIntersection(float3 rayOrigin, float3 rayDir, float3 boxPos, float3 boxSize, out float tMin, out float tMax)
{
    float3 invDir = 1.0 / rayDir;
    
    float3 boxMin = boxPos - boxSize;
    float3 boxMax = boxPos + boxSize;
    
    float3 t0 = (boxMin - rayOrigin) * invDir;
    float3 t1 = (boxMax - rayOrigin) * invDir;
    
    float3 tNear = min(t0, t1);
    float3 tFar = max(t0, t1);
    
    tMin = max(max(tNear.x, tNear.y), tNear.z);
    tMax = min(min(tFar.x, tFar.y), tFar.z);
    
    return tMin <= tMax && tMax > 0 /*(카메라가 박스 뒤에 있는지)*/;
}














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
    f = f * f * (3.0 - 2.0 * f); // Hermite Interpolation

    // 주변 8개 정점의 랜덤값 혼합
    float n = i.x + i.y * 157.0 + 113.0 * i.z;
    return lerp(lerp(lerp(frac(sin(n + 0.0) * 43758.5453), frac(sin(n + 1.0) * 43758.5453), f.x),
                   lerp(frac(sin(n + 157.0) * 43758.5453), frac(sin(n + 158.0) * 43758.5453), f.x), f.y),
               lerp(lerp(frac(sin(n + 113.0) * 43758.5453), frac(sin(n + 114.0) * 43758.5453), f.x),
                   lerp(frac(sin(n + 270.0) * 43758.5453), frac(sin(n + 271.0) * 43758.5453), f.x), f.y), f.z);
}

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
    
    float3 accumulatedColor = 0; // 누적할 빛의 양
    float transmittance = 1.0; // 남은 빛의 투과율 (1.0 = 완전 투명)

    for (int i = 0; i < numSteps; i++)
    {
        float distToCenter = length(p - spherePos);
        
        if (distToCenter < radius)
        {
            // 중심에 가까울수록 밀도가 높아지는 기본 공식
            float normalizedDist = 1.0 - saturate(distToCenter / radius);
            
            // 노이즈 추가 (선택 사항)
            // float noiseVal = SimpleNoise(p * 2.0 + _Time.y); // 움직이는 노이즈
            // normalizedDist *= noiseVal;

            // 현재 지점에서의 밀도와 흡수량(alpha)
            float localDensity = normalizedDist * density;
            float alpha = saturate(localDensity * stepSize);
            
            // 현재 지점의 입자가 산란시키는 빛의 양을 더함 (남은 투과율만큼만 뒤의 색이 보임)
            accumulatedColor += cloudColor.rgb * scattering * alpha * transmittance;
            transmittance *= (1.0 - alpha); // 빛이 입자를 통과하며 감쇄됨
            
            if (transmittance < 0.01)
                break;
        }
        
        p += rd * stepSize;
    }
    
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
    
    float3 accumulatedColor = 0; // 누적할 빛의 양
    float transmittance = 1.0; // 남은 빛의 투과율 (1.0 = 완전 투명)

    for (int i = 0; i < numSteps; i++)
    {
        float distToCenter = length(p - spherePos);
        
        if (distToCenter < radius)
        {
            float normalizedDist = 1.0 - saturate(distToCenter / radius);
            normalizedDist = pow(normalizedDist, 2.0);
            
            //float noiseVal = GradientNoise(p * 0.5 + _Time.y * 0.2);
            float noiseVal = GradientNoise(p * 0.5);
            noiseVal = saturate(noiseVal * 1.5 - 0.2);
            normalizedDist *= noiseVal;
            
            //float noiseVal = SimpleNoise(p * 2.0); // 움직이는 노이즈
            //normalizedDist *= noiseVal;
            
            float localDensity = normalizedDist * density;
            float alpha = saturate(localDensity * stepSize);
            
            float3 currentParticleColor = lerp(cloudColor.rgb, float3(1, 1, 1), pow(1.0 - transmittance, 1.0));
            //float3 currentParticleColor = lerp(float3(1, 1, 1), cloudColor.rgb, 1.0 - transmittance);
            //accumulatedColor += currentParticleColor * scattering * alpha * transmittance;
            accumulatedColor += currentParticleColor * scattering * transmittance;
            //accumulatedColor += cloudColor.rgb * scattering * alpha * transmittance;
            
            transmittance *= (1.0 - alpha);
            
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
    
    float3 accumulatedColor = 0; // 누적할 빛의 양
    float transmittance = 1.0; // 남은 빛의 투과율 (1.0 = 완전 투명)

    for (int i = 0; i < numSteps; i++)
    {
        float distToCenter = length(p - spherePos);
        
        if (distToCenter < radius)
        {
            float normalizedDist = 1.0 - saturate(distToCenter / radius);
            
            float noiseVal = SimpleNoise(p * 2.0 + _Time.y); // 움직이는 노이즈
            normalizedDist *= noiseVal;
            
            float localDensity = normalizedDist * density;
            float alpha = saturate(localDensity * stepSize);
            
            accumulatedColor += cloudColor.rgb * scattering * alpha * transmittance;
            transmittance *= (1.0 - alpha);
            
            if (transmittance < 0.01)
                break;
        }
        
        p += rd * stepSize;
    }
    
    return float4(accumulatedColor, 1.0 - transmittance);
}
//float4 RaymarchVolume(float3 ro, float3 rd, float3 spherePos, float radius, float density, float scattering, float4 cloudColor)
//{
//    float t0, t1;
//    if (!RaySphereIntersection(ro, rd, spherePos, radius, t0, t1)) 
//        return float4(0, 0, 0, 0); // 안 부딪히면 투명하게 pass
//
//    const int numSteps = 32; // 이 값을 조절하며 성능과 퀄리티 타협
//    float stepSize = (t1 - t0) / (float) numSteps;
//    
//    float3 p = ro + rd * t0; // 광선의 시작점 설정
//    float totalDensity = 0;
//    
//    for (int i = 0; i < numSteps; i++)
//    {
//        float distToCenter = length(p - spherePos);
//        
//        // 구체 내부에 있을 때만 밀도 적분
//        if (distToCenter < radius)
//        {
//            // 중심에 가까울수록 밀도가 높아지는 공식 (Soft Edge 구현)
//            float normalizedDist = 1.0 - saturate(distToCenter / radius);
//            
//            float localDensity = normalizedDist * density;
//            totalDensity += localDensity * stepSize;
//        }
//        
//        p += rd * stepSize;
//    }
//
//    // 물리 기반 빛의 차폐 계산 (Beer's Law)
//    float transmittance = exp(-totalDensity); // 투과율(Transmittance) 계산: 0에 가까울수록 빛이 차단됨
//    float scatterValue = 1.0 - transmittance;// 산란광(Scattering) 표현: 투과율의 역수를 이용해 뽀얀 느낌 추가
//    float3 finalColor = cloudColor.rgb * scatterValue * scattering; //최종 색상 = 구름 색상 * 산란 세기 (알파값은 차폐된 정도)
//    
//    return float4(finalColor, scatterValue);
//}


//float4 RaymarchVolume_BOX(float3 rayOrigin, float3 rayDir, float3 boxPos, float3 boxSize, float scattering, float boxRoundness, float density, float4 cloudColor)
//{
//    float tNear, tFar;
//    
//    if (!RayBoxIntersection(rayOrigin, rayDir, boxPos, boxSize, tNear, tFar)) // 박스 내부 확인 => 차폐
//    {
//        return float4(0, 0, 0, 0);
//    }
//    
//    //if (tNear < tFar)
//    //    return float4(1, 0, 0, 1);
//    
//    float tStart = max(0.0, tNear); // 카메라가 박스 안에 있을 경우 tNear가 음수일 수 있으므로 0으로 보정
//    float tEnd = tFar;
//
//    // 깊이 테스트
//    // float sceneDepth = SampleSceneDepth(uv);
//    // tEnd = min(tEnd, sceneDepth);
//    // if (tStart >= tEnd) return float4(0,0,0,0);
//        
//    const int numSteps = 32;
//    float stepSize = (tEnd - tStart) / (float) numSteps;
//    float3 p = rayOrigin + rayDir * (tStart + stepSize * 0.5);
//
//    float3 accumulatedColor = 0;
//    float transmittance = 1.0;
//    for (int i = 0; i < numSteps; i++)
//    {
//        // 박스 로컬 공간으로 변환
//        float3 localP = p - boxPos;
//        float d = sdRoundBox(localP, boxSize, boxRoundness);
//
//        if (d < 0.0) // 박스 내부
//        {
//            float edgeSoftness = 0.5; // 외곽을 얼마나 부드럽게 깎을지 => 변수화
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


