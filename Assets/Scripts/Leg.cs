using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leg : MonoBehaviour
{
    // Self-explanatory variable names
    private LegController legController;

    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform rayOrigin;
    public GameObject ikTarget;

    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private AnimationCurve heightCurve;

    private float tipMaxHeight = 0.2f;
    private float tipAnimationTime = 0.15f;
    private float tipAnimationFrameTime = 1 / 60.0f;

    private float ikOffset = 1.0f;
    [SerializeField] private float tipMoveDist = 0.55f;
    [SerializeField] private float maxRayDist = 7.0f;
    private float tipPassOver = 0.55f / 2.0f;

    public Vector3 TipPos { get; private set; }
    public Vector3 TipUpDir { get; private set; }
    public Vector3 RaycastTipPos { get; private set; }
    public Vector3 RaycastTipNormal { get; private set; }

    public bool Animating { get; private set; } = false;
    public bool Movable { get; set; } = false;
    public float TipDistance { get; private set; }
    
    // Scale factor for dynamic adjustment
    private float currentScaleFactor = 1.0f;

    private void Awake()
    {
        legController = GetComponentInParent<LegController>();

        transform.parent = bodyTransform;
        rayOrigin.parent = bodyTransform;
        TipPos = ikTarget.transform.position;
        
        UpdateScaleFactor();
    }

    private void Start()
    {
        UpdateIKTargetTransform();
    }

    private void Update()
    {
        UpdateScaleFactor();
        
        // Scale distances based on current object scale to support tiny creatures (0.1 scale)
        float scaledMaxRayDist = maxRayDist * currentScaleFactor;
        float scaledTipMoveDist = tipMoveDist * currentScaleFactor;
        float scaledIkOffset = ikOffset * currentScaleFactor;
        float scaledTipMaxHeight = tipMaxHeight * currentScaleFactor;
        float scaledTipPassOver = tipPassOver * currentScaleFactor;

        RaycastHit hit;

        // Calculate the tip target position
        // Use bodyTransform.up for ray direction to support vertical/ceiling crawling
        if (Physics.Raycast(rayOrigin.position, -bodyTransform.up.normalized, out hit, scaledMaxRayDist))
        {
            RaycastTipPos = hit.point;
            RaycastTipNormal = hit.normal;
        }

        TipDistance = (RaycastTipPos - TipPos).magnitude;

        // If the distance gets too far, animate and move the tip to new position
        if (!Animating && (TipDistance > scaledTipMoveDist && Movable))
        {
            StartCoroutine(AnimateLeg(scaledTipMaxHeight, scaledTipPassOver, scaledIkOffset));
        }
    }
    
    private void UpdateScaleFactor()
    {
        // Get the current scale from lossyScale (accounts for parent scaling)
        currentScaleFactor = transform.lossyScale.x;
        if (currentScaleFactor <= 0.001f) currentScaleFactor = 0.001f; // Prevent issues at very small scales
    }

    private IEnumerator AnimateLeg(float scaledTipMaxHeight, float scaledTipPassOver, float scaledIkOffset)
    {
        Animating = true;

        float timer = 0.0f;
        float animTime;

        Vector3 startingTipPos = TipPos;
        Vector3 tipDirVec = RaycastTipPos - TipPos;
        tipDirVec += tipDirVec.normalized * scaledTipPassOver;

        Vector3 right = Vector3.Cross(bodyTransform.up, tipDirVec.normalized).normalized;
        TipUpDir = Vector3.Cross(tipDirVec.normalized, right);

        while (timer < tipAnimationTime + tipAnimationFrameTime)
        {
            animTime = speedCurve.Evaluate(timer / tipAnimationTime);

            // If the target is keep moving, apply acceleration to correct the end point
            float tipAcceleration = Mathf.Max((RaycastTipPos - startingTipPos).magnitude / tipDirVec.magnitude, 1.0f);

            TipPos = startingTipPos + tipDirVec * tipAcceleration * animTime; // Forward direction of tip vector
            TipPos += TipUpDir * heightCurve.Evaluate(animTime) * scaledTipMaxHeight; // Upward direction of tip vector

            UpdateIKTargetTransform(scaledIkOffset);

            timer += tipAnimationFrameTime;

            yield return new WaitForSeconds(tipAnimationFrameTime);
        }

        Animating = false;
    }

    private void UpdateIKTargetTransform(float scaledIkOffset = 1.0f)
    {
        // Update leg ik target transform depend on tip information
        ikTarget.transform.position = TipPos + bodyTransform.up.normalized * scaledIkOffset;
        ikTarget.transform.rotation = Quaternion.LookRotation(TipPos - ikTarget.transform.position) * Quaternion.Euler(90, 0, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(RaycastTipPos, 0.1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(TipPos, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(TipPos, RaycastTipPos);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(ikTarget.transform.position, 0.1f);
    }
}
