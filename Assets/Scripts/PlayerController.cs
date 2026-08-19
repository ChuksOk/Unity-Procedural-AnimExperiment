using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player movement script with support for vertical/ceiling crawling
    
    [SerializeField] private float MoveSpeed = 3.8f;
    [SerializeField] private float RotSpeed = 80.0f;
    
    private LegController legController;
    private float currentScaleFactor = 1.0f;
    private CharacterController characterController;

    void Start()
    {
        legController = GetComponent<LegController>();
        characterController = GetComponent<CharacterController>();
        
        // Add CharacterController if it doesn't exist for proper collision handling
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 0.5f;
            characterController.radius = 0.3f;
        }
    }

    void Update()
    {
        // Handle keyboard control with surface-relative movement for wall/ceiling crawling
        // This loop competes with AdjustBodyTransform() in LegController script to properly postion the body transform
        
        // Update scale factor to adjust movement speed for tiny creatures
        UpdateScaleFactor();
        float scaledMoveSpeed = MoveSpeed * currentScaleFactor;

        // Get input values
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // Determine the current surface normal from legs or raycast
        Vector3 surfaceNormal = GetSurfaceNormal();
        
        // Calculate movement direction relative to the creature's orientation and surface
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, surfaceNormal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, surfaceNormal).normalized;
        
        // Ensure vectors are valid
        if (forward.magnitude < 0.001f) forward = Vector3.ProjectOnPlane(Vector3.forward, surfaceNormal).normalized;
        if (right.magnitude < 0.001f) right = Vector3.Cross(surfaceNormal, forward).normalized;
        
        // Calculate movement vector on the surface plane
        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput) * scaledMoveSpeed * Time.deltaTime;
        
        // Apply movement
        if (characterController != null && characterController.enabled)
        {
            characterController.Move(moveDirection);
        }
        else
        {
            transform.position += moveDirection;
        }

        // Rotation - rotate around the surface normal instead of world up
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(surfaceNormal, -RotSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(surfaceNormal, RotSpeed * Time.deltaTime);
        }
    }
    
    private Vector3 GetSurfaceNormal()
    {
        // Try to get surface normal from legs first
        if (legController != null)
        {
            Leg[] legs = legController.GetLegs();
            if (legs != null && legs.Length > 0)
            {
                Vector3 avgNormal = Vector3.zero;
                int validCount = 0;
                
                foreach (Leg leg in legs)
                {
                    if (leg.RaycastTipNormal != Vector3.zero)
                    {
                        avgNormal += leg.RaycastTipNormal;
                        validCount++;
                    }
                }
                
                if (validCount > 0)
                {
                    return avgNormal.normalized;
                }
            }
        }
        
        // Fallback: raycast downward from body
        RaycastHit hit;
        float rayDist = 2.0f * currentScaleFactor;
        if (Physics.Raycast(transform.position, -transform.up, out hit, rayDist))
        {
            return hit.normal;
        }
        
        // Default to world up if no surface found
        return Vector3.up;
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
