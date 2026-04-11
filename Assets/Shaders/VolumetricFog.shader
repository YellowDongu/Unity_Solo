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
            #pragma vertex VS_Main
            #pragma fragment PS_Main
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Functions.hlsl"

            struct VS_INPUT
            {
                float4 positionOS : POSITION;
            };
            struct VS_OUTPUT
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

            
            
            float fbm(float3 p) // FBM (여러 층의 노이즈를 겹쳐 구름 질감을 만듦)
            {
                //float f = 0.5000 * Noise(p);
                float f = 0.5000 * perlinNoise(p);
                p = p * 2.02;
                f += 0.2500 * perlinNoise(p);
                //f += 0.2500 * Noise(p);
                p = p * 2.03;
                f += 0.1250 * perlinNoise(p);
                //f += 0.1250 * Noise(p);
                return f;
            }

            // --------------------------------------------------

            VS_OUTPUT VS_Main(VS_INPUT input)
            {
                VS_OUTPUT output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                return output;
            }

            float4 PS_Main(VS_OUTPUT input) : SV_Target
            {
                float3 worldRayOrigin = GetCameraPositionWS();
                float3 worldRayDir = normalize(input.positionWS - worldRayOrigin);

                float3 localRayOrigin = mul(unity_WorldToObject, float4(worldRayOrigin, 1.0)).xyz;
                float3 localRayDir = normalize(mul((float3x3)unity_WorldToObject, worldRayDir));

                float2 rayInfo = RayBoxIntersection(localRayOrigin, localRayDir);

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
