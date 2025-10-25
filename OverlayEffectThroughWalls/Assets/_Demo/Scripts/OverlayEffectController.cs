using System.Collections.Generic;
using UnityEngine;

public class OverlayEffectController : MonoBehaviour
{
    public int overlayID;
    private List<Material> materials;
    private MaterialPropertyBlock materialPropertyBlock;

    private void Start()
    {
        materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetInt("_OverlayID", overlayID);
        materials = new List<Material>();
        foreach(var skinnedMeshRenderer in transform.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            materials.Add(skinnedMeshRenderer.sharedMaterial);
            skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        SetShaderPassEnabled(false);
    }

    public void SetShaderPassEnabled(bool value)
    {
        foreach (var material in materials)
        {
            material.SetShaderPassEnabled("WriteStencil", value);
        }
    }
}
