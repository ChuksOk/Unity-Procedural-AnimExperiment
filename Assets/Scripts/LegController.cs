using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegController : MonoBehaviour
{
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Leg[] legs;

    [SerializeField] private float maxTipWait = 0.7f;
    
    private bool readySwitchOrder = false;
    private bool stepOrder = true;
    [SerializeField] private float bodyHeightBase = 1.3f;

    private Vector3 bodyPos;
    private Vector3 bodyUp;
    private Vector3 bodyForward;
    private Vector3 bodyRight;
    private Quaternion bodyRotation;

    [SerializeField] private float PosAdjustRatio = 0.1f;
    [SerializeField] private float RotAdjustRatio = 0.2f;
    
    // Scale factor for dynamic adjustment
    private float currentScaleFactor = 1.0f;

    private void Start()
    {
        UpdateScaleFactor();
        // Start coroutine to adjust body transform
        StartCoroutine(AdjustBodyTransform());
    }

    private void Update()
    {
        UpdateScaleFactor();
        
        // Scale the max tip wait distance based on creature size
        float scaledMaxTipWait = maxTipWait * currentScaleFactor;
        
        if (legs.Length < 2) return;

        // If tip is not in current order but it's too far from target position, Switch the order
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i].TipDistance > scaledMaxTipWait)
            {
                stepOrder = i % 2 == 0;
                break;
            }
        }

        // Ordering steps
        foreach (Leg leg in legs)
        {
            leg.Movable = stepOrder;
            stepOrder = !stepOrder;
        }

        int index = stepOrder ? 0 : 1;

        // If the opposite foot step completes, switch the order to make a new step
        if (readySwitchOrder && !legs[index].Animating)
        {
            stepOrder = !stepOrder;
            readySwitchOrder = false;
        }

        if (!readySwitchOrder && legs[index].Animating)
        {
            readySwitchOrder = true;
        }
    }
    
    private void UpdateScaleFactor()
    {
        // Get the current scale from lossyScale (accounts for parent scaling)
        if (bodyTransform != null)
        {
            currentScaleFactor = bodyTransform.lossyScale.x;
            if (currentScaleFactor <= 0.001f) currentScaleFactor = 0.001f;
        }
    }

    public Leg[] GetLegs()
    {
        return legs;
    }

    private IEnumerator AdjustBodyTransform()
    {
        while (true)
        {
            Vector3 tipCenter = Vector3.zero;
            bodyUp = Vector3.zero;

            // Collect leg information to calculate body transform
            foreach (Leg leg in legs)
            {
                tipCenter += leg.TipPos;
                bodyUp += leg.TipUpDir + leg.RaycastTipNormal;
            }

            // Scale raycast distance based on creature size for wall/ceiling crawling
            float scaledRayDist = 10.0f * currentScaleFactor;
            
            RaycastHit hit;
            // Raycast in the direction of body's local down (-up) to detect surface below
            if (Physics.Raycast(bodyTransform.position, -bodyTransform.up.normalized, out hit, scaledRayDist))
            {
                bodyUp += hit.normal;
            }

            tipCenter /= legs.Length;
            bodyUp.Normalize();

            // Scale body height based on creature size
            float scaledBodyHeight = bodyHeightBase * currentScaleFactor;

            // Interpolate postition from old to new
            bodyPos = tipCenter + bodyUp * scaledBodyHeight;
            bodyTransform.position = Vector3.Lerp(bodyTransform.position, bodyPos, PosAdjustRatio);

            // Calculate new body axis
            bodyRight = Vector3.Cross(bodyUp, bodyTransform.forward).normalized;
            if (bodyRight.magnitude < 0.001f)
            {
                // Fallback if forward is parallel to up
                bodyRight = Vector3.Cross(bodyUp, Vector3.right).normalized;
            }
            bodyForward = Vector3.Cross(bodyRight, bodyUp).normalized;

            // Interpolate rotation from old to new
            bodyRotation = Quaternion.LookRotation(bodyForward, bodyUp);
            bodyTransform.rotation = Quaternion.Slerp(bodyTransform.rotation, bodyRotation, RotAdjustRatio);

            yield return new WaitForFixedUpdate();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(bodyPos, bodyPos + bodyRight);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(bodyPos, bodyPos + bodyUp);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(bodyPos, bodyPos + bodyForward);
    }
}
