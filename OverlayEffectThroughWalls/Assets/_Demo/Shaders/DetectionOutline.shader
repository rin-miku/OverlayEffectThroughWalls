Shader "Custom/DetectionOutline"
{
    Properties
    {
        _DepthSensitivity("Depth Sensitivity", Float) = 100.0
        _NormalSensitivity("Normal Sensitivity", Float) = 100.0
        _EdgeThresholdMin("Edge Threshold Min", Range(0,1)) = 0.85
        _EdgeThresholdMax("Edge Threshold Max", Range(0,1)) = 0.95
        [HDR]_OutlineColor("Outline Color", Color) = (10, 4, 0.5, 1)

        //[HideInInspector]
        _DetectionRadius("Detection Radius", Float) = 0.0
        [HideInInspector]
        _CenterPosition("Center Position", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags{ "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "DetectionOutline"
            Tags { "LightMode" = "DetectionOutline" }
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex   Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _DepthSensitivity;
            float _NormalSensitivity;
            float _EdgeThresholdMin;
            float _EdgeThresholdMax;
            half4 _OutlineColor;
            float _DetectionRadius;
            float3 _CenterPosition;

            TEXTURE2D(_CameraDepthTexture);
            TEXTURE2D(_CameraNormalsTexture);

            float SampleDepth(float2 uv)
            {
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_PointClamp, uv).r;
                #if UNITY_REVERSED_Z
                    return rawDepth;
                #else
                    return rawDepth * 2.0 - 1.0;
                #endif
            }

            float3 SampleNormal(float2 uv)
            {
                float4 packedNormal = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_PointClamp, uv);
                return normalize(packedNormal.xyz * 2.0 - 1.0); 
            }

            float DepthEdge(float2 uv)
            {
                float2 texelSize = 1.0 / _ScreenParams.xy;

                float d = SampleDepth(uv);
                float dR = SampleDepth(uv + float2(texelSize.x, 0));
                float dL = SampleDepth(uv - float2(texelSize.x, 0));
                float dU = SampleDepth(uv + float2(0, texelSize.y));
                float dD = SampleDepth(uv - float2(0, texelSize.y));

                float dx = abs(dR - dL);
                float dy = abs(dU - dD);
                float edge = sqrt(dx * dx + dy * dy);
                return saturate(edge * _DepthSensitivity);
            }

            float NormalEdge(float2 uv)
            {
                float2 texelSize = 1.0 / _ScreenParams.xy;
            
                float3 n = SampleNormal(uv);
                float3 nR = SampleNormal(uv + float2(texelSize.x, 0));
                float3 nL = SampleNormal(uv - float2(texelSize.x, 0));
                float3 nU = SampleNormal(uv + float2(0, texelSize.y));
                float3 nD = SampleNormal(uv - float2(0, texelSize.y));
            
                float3 dn = (nR - nL) + (nU - nD);
                float edge = length(dn);
                return saturate(edge * _NormalSensitivity);
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord.xy;

                float edgeDepth = DepthEdge(uv);
                float edgeNormal = NormalEdge(uv);
                float edge = saturate(edgeDepth + edgeNormal);
                float edgeMask = smoothstep(_EdgeThresholdMin, _EdgeThresholdMax, edge);

                float depth = SampleDepth(uv);
                float3 worldPosition = ComputeWorldSpacePosition(uv, depth, unity_MatrixInvVP);
                float dist = distance(worldPosition.xz, _CenterPosition.xz);
                float detectionMask = 1 - smoothstep(_DetectionRadius - 50, _DetectionRadius, dist);

                float mask = edgeMask * detectionMask;

                half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);

                return lerp(color, _OutlineColor, mask);
            }
            ENDHLSL
        }
    }
}