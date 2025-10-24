using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

public class DetectionOutlineRenderPass : ScriptableRenderPass
{
    private Material detectionOutlineMaterial;

    public DetectionOutlineRenderPass(Material material)
    {
        detectionOutlineMaterial = material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        var desc = cameraData.cameraTargetDescriptor;
        desc.depthStencilFormat = GraphicsFormat.None;
        desc.msaaSamples = 1;
        desc.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;

        TextureHandle cameraColor = resourceData.activeColorTexture;
        TextureHandle tempColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_DetectionOutline", true);

        BlitMaterialParameters blitParams = new BlitMaterialParameters(cameraColor, tempColor, detectionOutlineMaterial, 0);
        renderGraph.AddBlitPass(blitParams, "Detection Outline");
        resourceData.cameraColor = tempColor;
    }
}