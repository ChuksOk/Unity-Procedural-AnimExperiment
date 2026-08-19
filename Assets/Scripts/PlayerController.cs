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

    void Start()
    {
        legController = GetComponent<LegController>();
    }

    void Update()
    {
        // Handle keyboard control
        // This loop competes with AdjustBodyTransform() in LegController script to properly postion the body transform
        
        // Update scale factor to adjust movement speed for tiny creatures
        UpdateScaleFactor();
        float scaledMoveSpeed = MoveSpeed * currentScaleFactor;

        float ws = Input.GetAxis("Vertical") * scaledMoveSpeed * Time.deltaTime;
        transform.Translate(0, 0, ws);

        float ad = Input.GetAxis("Horizontal") * scaledMoveSpeed * Time.deltaTime;
        transform.Translate(ad, 0, 0);

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -RotSpeed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, RotSpeed * Time.deltaTime, 0);
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
