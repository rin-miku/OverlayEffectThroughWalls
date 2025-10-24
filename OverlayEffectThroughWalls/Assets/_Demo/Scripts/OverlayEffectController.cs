using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class OverlayEffectController : MonoBehaviour
{
    public int overlayID;
    private List<Material> materials;

    private void Start()
    {
        materials = new List<Material>();
        foreach(var skinnedMeshRenderer in transform.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            materials.AddRange(skinnedMeshRenderer.materials);
        }

        SetShaderPassEnabled(false);
        SetOverlayID(overlayID);
    }

    public void SetShaderPassEnabled(bool value)
    {
        foreach (var material in materials)
        {
            material.SetShaderPassEnabled("WriteStencil", value);
        }
    }

    public void SetOverlayID(int overlayID)
    {
        foreach (var material in materials)
        {
            material.SetInteger("_OverlayID", overlayID);
        }
    }
}
