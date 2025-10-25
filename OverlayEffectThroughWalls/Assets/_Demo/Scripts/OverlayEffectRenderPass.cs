using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

public class OverlayEffectRenderPass : ScriptableRenderPass
{
    private Material overlayOutlineMaterial;

    private class PassData
    {
        public RendererListHandle rendererListHandle;
    }

    public OverlayEffectRenderPass(Material material)
    {
        overlayOutlineMaterial = material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        var desc = cameraData.cameraTargetDescriptor;
        desc.depthStencilFormat = GraphicsFormat.None;
        desc.msaaSamples = 1;
        desc.graphicsFormat = GraphicsFormat.R8_UInt;

        TextureHandle tempColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_StencilTexture", true);

        using (var builder = renderGraph.AddRasterRenderPass("Write Stencil", out PassData passData))
        {
            SortingCriteria sortingCriteria = cameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.all;
            FilteringSettings filteringSettings = new FilteringSettings(renderQueueRange, ~0);
            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(new ShaderTagId("WriteStencil"), renderingData, cameraData, lightData, sortingCriteria);
            RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);

            passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

            builder.SetRenderAttachment(tempColor, 0);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
            builder.UseRendererList(passData.rendererListHandle);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                context.cmd.ClearRenderTarget(RTClearFlags.DepthStencil, Color.clear, 1f, 0u);
                context.cmd.DrawRendererList(data.rendererListHandle);
            });

            builder.SetGlobalTextureAfterPass(tempColor, Shader.PropertyToID("_StencilTexture"));
        }

        TextureHandle cameraColor = resourceData.activeColorTexture;
        BlitMaterialParameters blitParams = new BlitMaterialParameters(tempColor, cameraColor, overlayOutlineMaterial, 0);
        renderGraph.AddBlitPass(blitParams, "Overlay Outline");
        resourceData.cameraColor = cameraColor;
    }
}
