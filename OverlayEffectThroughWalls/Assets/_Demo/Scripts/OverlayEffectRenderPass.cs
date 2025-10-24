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
        UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
        UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
        UniversalLightData universalLightData = frameData.Get<UniversalLightData>();
        UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();

        var desc = universalCameraData.cameraTargetDescriptor;
        desc.depthStencilFormat = GraphicsFormat.None;
        desc.msaaSamples = 1;
        desc.graphicsFormat = GraphicsFormat.R8_UInt;

        TextureHandle tempColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_StencilTexture", true);

        using (var builder = renderGraph.AddRasterRenderPass("Write Stencil", out PassData passData))
        {
            SortingCriteria sortingCriteria = universalCameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.all;
            FilteringSettings filteringSettings = new FilteringSettings(renderQueueRange, ~0);
            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(new ShaderTagId("WriteStencil"), universalRenderingData, universalCameraData, universalLightData, sortingCriteria);
            RendererListParams rendererListParams = new RendererListParams(universalRenderingData.cullResults, drawingSettings, filteringSettings);

            passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

            builder.SetRenderAttachment(tempColor, 0);
            builder.UseRendererList(passData.rendererListHandle);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                context.cmd.DrawRendererList(data.rendererListHandle);
            });

            builder.SetGlobalTextureAfterPass(tempColor, Shader.PropertyToID("_StencilTexture"));
        }

        TextureHandle cameraColor = universalResourceData.activeColorTexture;
        BlitMaterialParameters blitParams = new BlitMaterialParameters(tempColor, cameraColor, overlayOutlineMaterial, 0);
        renderGraph.AddBlitPass(blitParams, "Overlay Outline");
        universalResourceData.cameraColor = cameraColor;
    }
}
