Shader "Custom/OverlayOutline"
{
    Properties
    {
        [HDR]_FillColor ("Fill Color", Color) = (1,1,1,1)
        [HDR]_OutlineColor ("Outline Color", Color) = (1,1,1,1)
        [HDR]_OutlineColor2 ("Outline Color 2", Color) = (1,1,1,1)
    }

    SubShader
    {
        Pass
        {
            Name "OverlayOutline"
            Tags { "LightMode" = "OverlayOutline" }

            ZTest Always
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            half4 _FillColor;
            half4 _OutlineColor;
            half4 _OutlineColor2;
            Texture2D<uint> _StencilTexture;

            bool IsEdge(uint2 p)
            {
                uint c = _StencilTexture.Load(int3(p, 0));
                uint l = _StencilTexture.Load(int3(p + uint2(-1,0), 0));
                uint r = _StencilTexture.Load(int3(p + uint2(1,0), 0));
                uint u = _StencilTexture.Load(int3(p + uint2(0,-1), 0));
                uint d = _StencilTexture.Load(int3(p + uint2(0,1), 0));
                return (l != c) | (r != c) | (u != c) | (d != c);
            }

            half4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                uint width, height;

                _StencilTexture.GetDimensions(width, height);

                uint2 pixelCoord = uint2(uv * float2(width, height));

                uint mask = _StencilTexture.Load(int3(pixelCoord, 0)).r;

                if(mask == 0) discard;

                half4 outlineColor = _OutlineColor;
                if(mask == 2) outlineColor = _OutlineColor2;

                if(IsEdge(pixelCoord)) return outlineColor;
                else return _FillColor;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
