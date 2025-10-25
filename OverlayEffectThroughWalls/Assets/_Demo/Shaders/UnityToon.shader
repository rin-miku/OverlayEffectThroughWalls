Shader "Custom/ToonWriteStencil"
{
    SubShader
    {
        Pass
        {
            Name "WriteStencil"
            Tags { "LightMode" = "WriteStencil" }

            ZTest LEqual
            ZWrite Off
            Cull Back
            Blend Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            UNITY_INSTANCING_BUFFER_START(OverlayProps)
                UNITY_DEFINE_INSTANCED_PROP(int, _OverlayID)
            UNITY_INSTANCING_BUFFER_END(OverlayProps)

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            uint4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                int overlayID = UNITY_ACCESS_INSTANCED_PROP(OverlayProps, _OverlayID);
                return uint4(overlayID,0,0,0);
            }
            ENDHLSL
        }
    }
}
