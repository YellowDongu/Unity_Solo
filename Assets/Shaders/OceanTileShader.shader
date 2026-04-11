Shader "Custom/OceanTileShader"
{
    Properties
    {
        [Header(Main)]
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [Normal] _NormalMap("Normal Map", 2D) = "bump" {}
        //[MainTexture] _MainTex ("Texture", 2D) = "white" {}
        _Tiling ("World Tiling", Vector) = (1, 1, 1, 1)
        [Header(Additional)]
        _Smoothness ("Specular Power", Float)  = 100     // 스펙큘러 강도 (pow)
        _SpecIntensity ("Speculer Intensity", Float)  = 5  // 스펙 세기
        _FresnelPower ("FresnelPower", Float) = 100
        _FresnelIntensity ("FresnelIntensity", Float) = 0.1
        _NormalScale ("NormalMap Scale", Float) = 1.8
        _Speed ("NormalMap Speed", Vector) = (-0.002, -0.002, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue" = "Geometry-10"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            ZWrite On
            Cull Back
            ZTest LEqual

            HLSLPROGRAM
            
            #pragma vertex VS_Main
            #pragma fragment FS_Main
            
            //==========================================================
            // includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            //https://docs.unity3d.com/Manual/urp/use-built-in-shader-methods-additional-lights-fplus.html
            //==========================================================
            
            //==========================================================
            // 옵션
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile _ _FORWARD_PLUS
            
            // Unity 6.0 문서 예시는 _FORWARD_PLUS / USE_FORWARD_PLUS,
            // 최신 문서 예시는 _CLUSTER_LIGHT_LOOP / USE_CLUSTER_LIGHT_LOOP 이라
            // 프로젝트 버전에 맞는 쪽을 쓰면 된다는데 아직 모르겠음
            
            
            //===================================================
            //Debug For RenderDoc
            //#pragma enable_d3d11_debug_symbols // RenderDoc Debug - Direct3D 11
            //#pragma enable_d3d12_debug_symbols// RenderDoc Debug - Direct3D 12
            //#pragma optimize(off) // RenderDoc Debug - Vulkan/치환된 플랫폼의 경우 (컴파일러 최적화 방지)
            //===================================================
            //==========================================================
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS   : SV_POSITION;  // Clip Space, 투영 차원까지 간 거
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float2 uv           : TEXCOORD2;
                float4 tangentWS    : TEXCOORD3;
                //float fog           : TEXCOORD4;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float2 _Tiling;
                float _Smoothness;      // 스펙큘러 강도 (pow)
                float _SpecIntensity;   // 스펙 세기
                float _FresnelPower;
                float _FresnelIntensity;
                float _NormalScale;
                float2 _Speed;
            CBUFFER_END
            
            //==========================================================
            // Vertex Shader
            Varyings VS_Main(Attributes input)
            {   
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.normalOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.tangentWS.xyz = TransformObjectToWorldDir(input.tangentOS.xyz);
                output.tangentWS.w = input.tangentOS.w; // Bitangent 계산을 위한 부호값
                output.uv = input.uv;
                //output.fog = ComputeFogFactor(output.positionCS.z);  // 전역 fog를 유니티꺼로 사용할 경우(Windows>Rendering>Lighting>Enviorment>Fog) 안개 계산 대행
                return output;
            }
            //==========================================================
            
            
            float3 LightCalculation(float3 normalWS, float3 viewDir, Light light) // Blinn-Phong LightCalculation
            {
                float3 L = normalize(-light.direction);
                float3 H = normalize(L + viewDir);
            
                float NdotL = saturate(dot(normalWS, L)); // Diffuse
                float spec = pow(saturate(dot(normalWS, H)), _Smoothness) * _SpecIntensity; // Specular (Blinn-Phong)
            
                return (NdotL + spec) * light.color * light.distanceAttenuation * light.shadowAttenuation;
            }
            
            float FresnelCalculation(float3 normalWS, float3 viewDir)
            {
                return pow(1.0 - saturate(dot(normalWS, -viewDir)), _FresnelPower) * _FresnelIntensity;
            }
            
            
            
            //==========================================================
            // Piexl Shader
            half4 FS_Main(Varyings input) : SV_Target
            {
            //struct InputData   // Forward+ 라이팅 계산에 씀, input.hlsl에 정의됨
            //{
            //    float3  positionWS;
            //    half3   normalWS;
            //    half3   viewDirectionWS;
            //    float4  shadowCoord;
            //    half    fogCoord;
            //    half3   vertexLighting;
            //    half3   bakedGI;
            //    float2  normalizedScreenSpaceUV;
            //    half4   shadowMask;
            //}; https://chulin28ho.tistory.com/633
            
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalize(input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
            
                float3 viewDir = inputData.viewDirectionWS;
                float2 uv = input.uv * _Tiling;
                float3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb;
            
                // normalMap
                uv = input.uv + _Time.y * _Speed.xy;
                //float3 normalTS = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, scrollingUV).xyz * 2 - 1;
                //float3 normalTS = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv).xyz * 2 - 1;
                float3 normalTS = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv).xyz * _NormalScale - 1;
                //float3 bitangentWS = cross(inputData.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                //float3 normalWS = TransformTangentToWorld(normalTS, float3x3(input.tangentWS.xyz, bitangentWS, inputData.normalWS));
                float3 bitangentWS = cross(inputData.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                float3 normalWS = TransformTangentToWorld(normalTS, float3x3(input.tangentWS.xyz, bitangentWS, inputData.normalWS));
            
            
                float3 lighting = 0;
                Light mainLight = GetMainLight();// Main Lights
                lighting += LightCalculation(normalWS, inputData.viewDirectionWS, mainLight);
            
                #if defined(_ADDITIONAL_LIGHTS) // Forward+ Additional Lights
            
                    // Forward+ Non Main Directional Lights
                    //#if USE_CLUSTER_LIGHT_LOOP
                    #if USE_FORWARD_PLUS
                    UNITY_LOOP for (uint i = 0; i < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); i++)
                    {
                        Light light = GetAdditionalLight(i, inputData.positionWS, half4(1,1,1,1));
                        lighting += LightCalculation(normalWS, viewDir, light);
                    }
                    #endif
            
                    // Additional Light
                    uint count = GetAdditionalLightsCount(); 
                    LIGHT_LOOP_BEGIN(count)
                        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
                        lighting += LightCalculation(normalWS, viewDir, light);
                    LIGHT_LOOP_END
            
                #endif
            
                float fresnel = FresnelCalculation(normalWS, viewDir); // 프리넬 반사 계산
                //return half4(baseColor * lighting + fresnel , 1); // 일단 알파 1
                return half4(baseColor * lighting + fresnel * lighting , 1);
                //result.xyz = MixFog(result.xyz, input.fog); // 안개 섞기, 유니티 전용 전역 안개를 사용할 경우 사용 가능
            
                //return half4(baseColor * lighting + fresnel * lighting , 1);
                //return half4(baseColor * lighting, 1);
            }
            ENDHLSL
            //==========================================================


//
//half4 frag(Varyings input) : SV_Target
//{
//    float3 N = normalize(input.normalWS);
//    float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);
//
//    InputData inputData = (InputData)0;
//    inputData.positionWS = input.positionWS;
//    inputData.normalWS = normalize(input.normalWS);
//    inputData.viewDirectionWS =  GetWorldSpaceNormalizeViewDir(input.positionWS);
//    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
//
//    float3 lighting = 0;
//
//    // 메인 라이트
//    Light mainLight = GetMainLight();
//    lighting += CalcLighting(N, V, mainLight, 64.0, 1.0);
//
//    // 추가 라이트
//    #if defined(_ADDITIONAL_LIGHTS)
//
//        // Forward+에서 비메인 Directional Light 처리
//        #if USE_FORWARD_PLUS
//        UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
//        {
//            Light additionalLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
//            lighting += CalcLighting(N, V, additionalLight, 64.0, 1.0);
//        }
//        #endif
//
//        // 일반 Additional Light 루프
//        uint pixelLightCount = GetAdditionalLightsCount();
//        LIGHT_LOOP_BEGIN(pixelLightCount)
//            Light additionalLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
//            lighting += CalcLighting(N, V, additionalLight, 64.0, 1.0);
//        LIGHT_LOOP_END
//
//    #endif
//
//    // 프레넬
//    float fresnel = CalcFresnel(N, V, 5.0, 0.8);
//
//    // 물 색감
//    float3 waterBase = float3(0.02, 0.25, 0.35);
//    float3 finalColor = waterBase * lighting + fresnel;
//
//    return half4(finalColor, 1);
//}

        }
        Pass // DepthNormals 패스, URP에서 카메라의 깊이-노멀 텍스처를 채우는 데 사용 -> 안개 셰이더에서 깊이텍스쳐를 가져올 때 사용
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
        
            ZWrite On
            ColorMask 0
            Cull Back
        
            HLSLPROGRAM
            #pragma vertex VS_Main
            #pragma fragment PS_Main
        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };
        
            Varyings VS_Main(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
        
            float4 PS_Main(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }
        //Pass
        //{
        //    Name "DepthOnly"
        //    Tags { "LightMode" = "DepthOnly" }
        //    //lit
        //    //Name "ForwardLit" "LightMode" = "UniversalForward" : 포워드 렌더링 경로에서 오브젝트들을 렌더링하는데 사용됩니다.
        //    //Name "ShadowCaster" "LightMode" = "ShadowCaster" : 그림자를 그리는데 사용됩니다.
        //    //Name "GBuffer" "LightMode" = "UniversalGBuffer" : 디퍼드 랜더링의 G 버퍼 생성에 사용됩니다.
        //    //Name "DepthOnly" "LightMode" = "DepthOnly" : 뎁스 버퍼 생성에 사용됩니다.
        //    //Name "Meta" "LightMode" = "Meta" : 라이트맵 베이킹에 사용됩니다.
        //    
        //    
        //
        //    ZWrite On
        //    ColorMask 0      // 색상은 안 씀, 깊이만 기록
        //    Cull Back
        //
        //    HLSLPROGRAM
        //    #pragma vertex DepthOnlyVertex
        //    #pragma fragment DepthOnlyFragment
        //
        //    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //    #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
        //    ENDHLSL
        //}
    }
}
