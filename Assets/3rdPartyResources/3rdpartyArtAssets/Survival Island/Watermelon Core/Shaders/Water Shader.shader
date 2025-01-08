Shader "WMelon/Water"
{
    Properties
    {
        _Pattern1("Water Pattern", 2D) = "black" {}
        _PatternColor1("Pattern Color", Color) = (1, 1, 1, 0.2)
        _PatternSize1("Pattern Size", float) = 1
        _PatternVelocity1("Pattern Velocity", Vector) = (0.01, 0.01, 0, 0)

        [Space]
        [Toggle(SECOND_PATTERN_ON)]_SecondPatternOn("Second Pattern Enabled", float) = 0
        _Pattern2("Second Water Pattern", 2D) = "black" {}
        _PatternColor2("Second Pattern Color", Color) = (1, 1, 1, 0.2)
        _PatternSize2("Second Pattern Size", float) = 1
        _PatternVelocity2("Second Pattern Velocity", Vector) = (0.01, 0.01, 0, 0)

        [Space]
        [Toggle(DISTORTION_ON)]_DistortionOn("Distortion Enabled", float) = 0
        _PatternDistortionNoise("Pattern Distortion Noise", 2D) = "black" {}
        _DistortionPower("Distortion Noise Power", float) = 1
        _PatternDistortionNoiseScale("Distortion Noise Scale", float) = 1
        _PatternDistortionNoiseVelocity("Distortion Noise Velocity", Vector) = (0.1, 0.1, 0, 0)

        [Space]
        [Space]
        _ShallowColor("Shallow Color", Color) = (0.325,0.807,0.971,0.725)
        _DeepColor("Main Deep Color", Color) = (0.086,0.407,0.749,1)
        _DeepColor2("Secondary Deep Color", Color) = (0.106,0.457,0.8,1)
        _DepthColorRange("Depth Color Range", Vector) = (0.1, 5, 0, 0)
       
        [Toggle(SECONDARY_DEEP_COLOR_ON)]_SecondaryDeepColorOn("Secondary Deep Color Enabled", float) = 0
        _DeepNoise("Deep Noise", 2D) = "black" {}
        _DeepNoisePower("Deep Noise Power", Range(0, 1)) = 1
        _DeepNoiseVelocity("Deep Noise Velocity", Vector) = (0.01, 0.01, 0, 0)
        _DeepNoiseScale("Deep Noise Scale", float) = 1

        [Space]
        [Space]
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FoamDepthRange("Foam Depth Range", Vector) = (0.05, 0.1, 0, 0)

        [Toggle(FOAM_NOISE_ON)]_FoamNoiseOn("Foam Noise Enabled", float) = 0
        _FoamNoise("Foam Noise", 2D) = "white" {}
        _FoamNoisePower("Foam Noise Power", Range(0, 1)) = 1
        _FoamNoiseVelocity("Foam Noise Velocity", Vector) = (0.01, 0.01, 0, 0)
        _FoamNoiseScale("Foam Noise Scale", float) = 1

        [Space]
        [Toggle(SPECULAR_ON)]_SpecularOn("Specular", float) = 0

        _SpecularMap("Specular Map", 2D) = "white" {}
        _Specular("Specular Highlights", range(0, 1)) = 0.1
        _Shiness("Specular shiness", range(0.1, 1)) = 0.1
        _SpecularMin("Specular Min", Color) = (0,0,0,0)
        _SpecularMax("Specular Max", Color) = (1,1,1,1)

        [Toggle(TOON_ON)]_ToonOn("Toon", float) = 0

        _RampTex("Texture", 2D) = "white" {}
        _RampMin("Ramp Min", Color) = (0,0,0,1)
        _RampMax("Ramp Max", Color) = (1,1,1,1)

        [Toggle] _CustomLightDirectionOn("Custom Light Direction Enabled", float) = 0
        _CustomLightDirection("Custom Light Direction", Vector) = (-0.5, -0.5, -0.5, 0)

        [Space]
        [Space]
        [Toggle(NORMAL_MAP_ON)] _NormalMapOn("Normal On", float) = 0
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalMapScale("Normal Map Scale", float) = 1
        _NormalMapVelocity("Normal Map Velocity", Vector) = (0.1, 0.1, 0, 0)
        [Space]
        [Toggle(SECONDARY_NORMAL_MAP_ON)] _SecondaryNormalMapOn("Secondary Normal On", float) = 0
        _SecondaryNormalMap ("Secondary Normal Map", 2D) = "bump" {}
        _SecondaryNormalMapScale("Secondary Normal Map Scale", float) = 1
        _SecondaryNormalMapVelocity("Secondary Normal Map Velocity", Vector) = (0.1, 0.1, 0, 0)

        [Space]
        [Space]
        _Color("Color", Color) = (1,1,1,1)
        _LightColorOn("Uses Light Color", float) = 1
        _LightColorInfluence("LightColorinfluence", Range(0, 1)) = 1

        [KeywordEnum(None, Vertex, Pixel)] _Shadows("Receive Shadows", Float) = 2
        _SColor("Shadow Color", Color) = (0.5,0.5,0.5,1)

        [Toggle(EMISSION_ON)]_EmissionOn("Emission", float) = 0
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _EmissionTex("Emission Texture", 2D) = "white" {}
        _EmissionTexScale("Emission Texture Scale", float) = 1
        _EmissionTexVelocity("Emission Texture Velocity", Vector) = (0.1, 0.1, 0, 0)

        [Toggle(CURVE_ON)]_CurveOn("Curve", float) = 0
    }
    SubShader
    {
        Cull back
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Cull Back

            HLSLPROGRAM
            
            #pragma shader_feature_local CURVE_ON
            #pragma shader_feature_local EMISSION_ON
            #pragma shader_feature_local SPECULAR_ON
            #pragma shader_feature_local _SHADOWS_NONE _SHADOWS_VERTEX _SHADOWS_PIXEL
            #pragma shader_feature_local TOON_ON
            #pragma shader_feature_local SECOND_PATTERN_ON
            #pragma shader_feature_local DISTORTION_ON
            #pragma shader_feature_local SECONDARY_DEEP_COLOR_ON
            #pragma shader_feature_local FOAM_NOISE_ON
            #pragma shader_feature_local NORMAL_MAP_ON
            #pragma shader_feature_local SECONDARY_NORMAL_MAP_ON
            #pragma shader_feature_local USE_GLOBAL_MULTIPLIERS
            #pragma multi_compile_fog

#ifndef _SHADOWS_NONE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _LIGHT_COOKIES

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
#endif

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #define W_WATER          

            #include "Includes/Water/WaterParams.hlsl"
            #include "Includes/Utils.hlsl"
            #include "Includes/Emission.hlsl"
            #include "Includes/Toon.hlsl"
            #include "Includes/LightColor.hlsl"
            #include "Includes/Specular.hlsl"
            #include "Includes/Curve.hlsl"
            #include "Includes/Position.hlsl"
            #include "Includes/Normals.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                float4 color : COLOR0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;

                float4 positionWS: TEXCOORD1;

        #define WORLD_NORM
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;

        #define VIEW_DIR
                float3 viewDir : TEXCOORD4;

                half  fogFactor : TEXCOORD5;

                float4 screenPos: TEXCOORD6;
            };

            v2f vert (appdata input)
            {
                // Structure that passed down to the fragment function
                v2f output = (v2f)0;

                // Getting world position of the vertex
                output.positionWS = float4(TransformObjectToWorld(input.positionOS.xyz), 1);

                // Storing shadow attenuation value inside positionWS.w if the shadow is per vertex
#ifdef _SHADOWS_VERTEX
                output.positionWS.w = ShadowAtten(output.positionWS);
#endif

                // Applying curvature offset
                input.positionOS -= GetCurveOffset(output.positionWS.xyz);

                // Recalculating world position after evaluating curve and foliage;
                output.positionWS.xyz = TransformObjectToWorld(input.positionOS.xyz);

                // Calcualting normals. The result is stored in the last two parameters
                GetWorldNormal(input.normalOS, input.tangentOS, output.normalWS, output.tangentWS);

                // Calculating the direction from camera to the vertex
                output.viewDir = GetViewDir(output.positionWS);

                // Getting clip space position
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                output.screenPos = ComputeScreenPos(output.positionCS);

                // Just passing uv through
                output.uv = input.uv;

                // Just passing color through
                output.color = input.color;

                // calculating fog
                output.fogFactor = ComputeFogFactor(output.positionCS);

                return output;
            }

            half quadOut(half t) {
                return 1 - (1 - t) * (1 - t);
            }

            half ilerp(half a, half b, half t)
            {
                return (t - a)/(b - a);
            }

            float GetDepth(float4 screenPos)
            {
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenPos.xy / screenPos.w);
                //rawNormals = SampleSceneNormals(screenPos.xy / screenPos.w);
                float cameraDepth = Linear01Depth(rawDepth, _ZBufferParams);

                return cameraDepth * _ProjectionParams.z + _ProjectionParams.y - screenPos.w;
            }

            half4 _ShallowColor;
            half4 _DeepColor;
            half4 _DeepColor2;
            sampler2D _DeepNoise;
            half _DeepNoisePower;
            half2 _DeepNoiseVelocity;
            half _DeepNoiseScale;
            half2 _DepthColorRange;

            float4 GetDepthColor(float4 positionWS, half depth)
            {
#ifdef SECONDARY_DEEP_COLOR_ON
                float2 deepNoiseUV = positionWS.xz * _DeepNoiseScale - (_Time.y * _DeepNoiseVelocity.xy) % 1;
                half deepNoise = tex2D(_DeepNoise, deepNoiseUV) * _DeepNoisePower;
                half4 deepColor = lerp(_DeepColor, _DeepColor2, deepNoise);
#else 
                half4 deepColor = _DeepColor;
#endif
                half t = saturate(ilerp(_DepthColorRange.x, _DepthColorRange.y, depth));

                half4 depthColor = lerp(_ShallowColor, deepColor, t);

                return depthColor;
            }

            half4 _FoamColor;
            half2 _FoamDepthRange;
            sampler2D _FoamNoise;
            half _FoamNoisePower;
            half2 _FoamNoiseVelocity;
            half _FoamNoiseScale;

            float4 GetFoam(float4 positionWS, half4 color, half depth)
            {
                half foamNoise = 0;
#ifdef FOAM_NOISE_ON
                float2 foamNoiseUV = positionWS.xz * _FoamNoiseScale - (_Time.y * _FoamNoiseVelocity.xy) % 1;
                foamNoise = tex2D(_FoamNoise, foamNoiseUV) * _FoamNoisePower;
#endif
                half t = (1 - saturate(ilerp(_FoamDepthRange.x, _FoamDepthRange.y, depth))) * (1 - foamNoise);
                return lerp(color, _FoamColor, t);;
            }

            sampler2D _Pattern1;
            half4 _PatternColor1;
            half _PatternSize1;
            half2 _PatternVelocity1;

            sampler2D _Pattern2;
            half4 _PatternColor2;
            half _PatternSize2;
            half2 _PatternVelocity2;

            sampler2D _PatternDistortionNoise;
            half _DistortionPower;
            half _PatternDistortionNoiseScale;
            half2 _PatternDistortionNoiseVelocity;

            float3 GetPattern(float4 positionWS, half depth)
            {
                float distortionPower = 0;
#ifdef DISTORTION_ON
                float2 distortionUV = positionWS.xz * _PatternDistortionNoiseScale - (_Time.y * _PatternDistortionNoiseVelocity.xy) % 1;
                distortionPower = tex2D(_PatternDistortionNoise, distortionUV).x * _DistortionPower;
#endif
                float2 waterPatternUV = positionWS.xz * _PatternSize1 - ((_Time.y + distortionPower) * _PatternVelocity1.xy) % 1;
                half4 patternColor = tex2D(_Pattern1, waterPatternUV) * _PatternColor1;
                
#ifdef SECOND_PATTERN_ON
                float2 waterPatternUV2 = positionWS.xz * _PatternSize2 - ((_Time.y + distortionPower) * _PatternVelocity2.xy) % 1;
                half4 patternColor2 = tex2D(_Pattern2, waterPatternUV2) * _PatternColor2;

                patternColor = patternColor * patternColor.a + patternColor2 * patternColor2.a;
#endif

                half t = saturate(ilerp(_DepthColorRange.x, _DepthColorRange.y, depth));
                return patternColor.rgb * saturate(patternColor.a) * t;
            }

            

            float3 GetNormals(float4 positionWS, float3 normalWS, float4 tangentWS, half depth)
            {
#ifdef NORMAL_MAP_ON
                float2 normalsUV = positionWS.xz * _NormalMapScale - (_Time.y * _NormalMapVelocity.xy) % 1;
                float3 tangentNormal = UnpackNormal(_NormalMap, normalsUV);

#ifdef SECONDARY_NORMAL_MAP_ON
                float2 normalsUV2 = positionWS.xz * _SecondaryNormalMapScale - (_Time.y * _SecondaryNormalMapVelocity.xy) % 1;
                float3 secondaryTangentNormal = UnpackNormal(_SecondaryNormalMap, normalsUV2);

                tangentNormal = normalize(half3(tangentNormal.xy + secondaryTangentNormal.xy, tangentNormal.z * secondaryTangentNormal.z));
#endif
                float sgn = tangentWS.w;
                float3 bitangent = sgn * cross(normalWS, tangentWS.xyz);
                half3x3 tangentToWorld = half3x3(tangentWS.xyz, bitangent.xyz, normalWS);

                half3 normWorld = TransformTangentToWorld(tangentNormal, tangentToWorld);

                half t = saturate(ilerp(_DepthColorRange.x, _DepthColorRange.y, depth));

                return lerp(1, normWorld, t);
#endif
                return normalWS;
            }

            half _EmissionTexScale;
            half2 _EmissionTexVelocity;

            float4 GetWaterEmission(float4 positionW, float2 uv)
            {
                float2 emissionUV = positionW.xz * _EmissionTexScale - (_Time.y * _EmissionTexVelocity.xy) % 1;
                
                return GetEmmision(uv);
            }

            half4 frag(v2f input) : SV_Target
            {              
                half depth = GetDepth(input.screenPos);
                input.normalWS = GetNormals(input.positionWS, input.normalWS, input.tangentWS, depth);

                half4 color = GetDepthColor(input.positionWS, depth);
                color = GetFoam(input.positionWS, color, depth);

                color.rgb += GetPattern(input.positionWS, depth);

                // Getting light color into account (cookies included)
                color *= GetLightColor(input.positionWS);

                // Adding specular reflections
                color += GetSpecular(input.positionWS, input.normalWS, input.viewDir, input.uv);

                // Adding shading with toon
                color *= GetToon(input.positionWS, input.normalWS);

                color += GetWaterEmission(input.positionWS, input.uv);

                // Calculaing shadows
                //color *= GetShadows(input.positionWS, input.screenPos);

                // Calculaing shadows
                color.rgb *= GetMainLighting(input.positionWS, input.screenPos, 1);//GetShadows(input.positionWS, input.screenPos);
                //color.rgb += GetAdditionalLighting(input.positionWS, input.screenPos, 1);

                color.rgb = MixFog(color.rgb, InitializeInputDataFog(float4(input.positionWS.xyz, 1.0), input.fogFactor));

                return color;
            }

            ENDHLSL
        }
    }

    CustomEditor "Watermelon.Shader.WaterGUI"
}
