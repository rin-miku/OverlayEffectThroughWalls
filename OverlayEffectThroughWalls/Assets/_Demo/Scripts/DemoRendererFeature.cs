using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DemoRendererFeature : ScriptableRendererFeature
{
    public OverlayEffectSettings OverlayEffectSettings;
    public DetectionOutlineSettings DetectionOutlineSettings;
    private OverlayEffectRenderPass OverlayEffectRenderPass;
    private DetectionOutlineRenderPass DetectionOutlineRenderPass;

    public override void Create()
    {
        OverlayEffectRenderPass = new OverlayEffectRenderPass(OverlayEffectSettings.overlayOutlineMaterial);
        OverlayEffectRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        DetectionOutlineRenderPass = new DetectionOutlineRenderPass(DetectionOutlineSettings.detectionMaterial);
        DetectionOutlineRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(OverlayEffectRenderPass);
        renderer.EnqueuePass(DetectionOutlineRenderPass);
    }
}
