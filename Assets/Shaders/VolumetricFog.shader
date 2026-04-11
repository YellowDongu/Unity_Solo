Shader "Custom/VolumetricFog"
{
    Properties
    {
        [Header(Visuals)]
        _MainTex ("Noise Texture (3D)", 3D) = "white" {}
        _Color ("Cloud Color", Color) = (1, 1, 1, 1)
        _DensityMultiplier ("Density", Range(0, 100)) = 20
        _Threshold ("Cloud Cutoff", Range(0, 1)) = 0.3
        
        [Header(Performance)]
        _StepCount ("Ray Steps", Range(16, 256)) = 64
        _NoiseScale ("Noise Scale", Float) = 1.0
    }


    SubShader
    {

        Tags // 유니티 6 URP 표준 태그
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

            //Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite Off
            //Cull Off
            //ZTest LEqual

        Pass
        {
            Name "VolumetricFogPass"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vertexShader
            #pragma fragment pixelShader
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            float4 _Color;
            float _DensityMultiplier;
            float _Threshold;
            int _StepCount;
            float _NoiseScale;

            
            float hash(float n) // 실시간 3D 노이즈 함수 (Perlin/Pseudo-random 기반)
            {
                return frac(sin(n) * 43758.5453123);
            }
            
            float noise(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                float n = p.x + p.y * 57.0 + 113.0 * p.z;
                f = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x), lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y), lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x), lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
            }

            float fbm(float3 p) // FBM (여러 층의 노이즈를 겹쳐 구름 질감을 만듦)
            {
                float f = 0.5000 * noise(p);
                p = p * 2.02;
                f += 0.2500 * noise(p);
                p = p * 2.03;
                f += 0.1250 * noise(p);
                return f;
            }

            // --------------------------------------------------

            float2 rayBoxIntersection(float3 rayOrigin, float3 rayDirection)
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

            Varyings vertexShader(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                return output;
            }

            float4 pixelShader(Varyings input) : SV_Target
            {
                float3 worldRayOrigin = GetCameraPositionWS();
                float3 worldRayDir = normalize(input.positionWS - worldRayOrigin);

                float3 localRayOrigin = mul(unity_WorldToObject, float4(worldRayOrigin, 1.0)).xyz;
                float3 localRayDir = normalize(mul((float3x3)unity_WorldToObject, worldRayDir));

                float2 rayInfo = rayBoxIntersection(localRayOrigin, localRayDir);

                if (rayInfo.y <= 0)
                    discard;

                float stepSize = rayInfo.y / _StepCount;
                float3 currentLocalPos = localRayOrigin + localRayDir * (rayInfo.x + stepSize * 0.5);
                
                float accumulatedAlpha = 0;

                [loop]
                for (int i = 0; i < _StepCount; i++)
                {
                    float3 p = (currentLocalPos + 0.5) * _NoiseScale;
                    float d = fbm(p); // 실시간 노이즈 계산
                    
                    float cloudSample = saturate(d - _Threshold) * _DensityMultiplier;
                    if (cloudSample > 0)
                    {
                        accumulatedAlpha += cloudSample * stepSize * (1.0 - accumulatedAlpha);
                        if (accumulatedAlpha >= 0.95)
                            break;
                    }

                    currentLocalPos += localRayDir * stepSize;
                }

                return float4(_Color.rgb, accumulatedAlpha);
            }


            ENDHLSL
        }
    }
}
