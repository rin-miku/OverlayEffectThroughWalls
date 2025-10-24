using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float radius = 5f;
    public LayerMask detectableLayers;

    private HashSet<Transform> insideObjects = new HashSet<Transform>();

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, detectableLayers);
        HashSet<Transform> currentFrame = new HashSet<Transform>();

        foreach (var hit in hits)
        {
            currentFrame.Add(hit.transform);

            if (!insideObjects.Contains(hit.transform))
            {
                insideObjects.Add(hit.transform);
                OnEnter(hit.transform);
            }
        }

        var exited = new HashSet<Transform>(insideObjects);
        exited.ExceptWith(currentFrame);

        foreach (var t in exited)
        {
            insideObjects.Remove(t);
            OnExit(t);
        }

        //Debug.Log(insideObjects.Count);
    }

    void OnEnter(Transform target)
    {
        Debug.Log($"½øÈëÌ½²â·¶Î§: {target.name}");
        if (target == null) return;
        if (target.TryGetComponent(out OverlayEffectController overlayEffectController))
        {
            overlayEffectController.SetShaderPassEnabled(true);
        }
    }

    void OnExit(Transform target)
    {
        Debug.Log($"Àë¿ªÌ½²â·¶Î§: {target.name}");
        if (target == null) return;
        if(target.TryGetComponent(out OverlayEffectController overlayEffectController))
        {
            overlayEffectController.SetShaderPassEnabled(false);
        }        
    }

    private void OnDisable()
    {
        foreach (var t in insideObjects)
        {
            OnExit(t);
        }
        insideObjects.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
