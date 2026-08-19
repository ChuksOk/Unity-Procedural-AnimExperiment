using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGizmo : MonoBehaviour
{
    public static bool Enabled = true;

    [SerializeField] public float size = 0.1f;
    [SerializeField] public Color color = Color.red;

    // Scale factor for dynamic adjustment
    private float currentScaleFactor = 1.0f;

    private void Start()
    {
        UpdateScaleFactor();
    }

    private void Update()
    {
        UpdateScaleFactor();
    }

    // Draw sphere gizmo in the scene view (scaled to creature size)
    private void OnDrawGizmos()
    {
        if (Enabled)
        {
            Gizmos.color = color;
            float scaledSize = size * currentScaleFactor;
            Gizmos.DrawSphere(transform.position, scaledSize);
        }
    }
    
    private void UpdateScaleFactor()
    {
        if (transform != null)
        {
            currentScaleFactor = transform.lossyScale.x;
            if (currentScaleFactor <= 0.001f) currentScaleFactor = 0.001f;
        }
    }
}
