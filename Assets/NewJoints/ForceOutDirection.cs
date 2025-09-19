using System;
using UnityEngine;

public class ForceOutDirection : MonoBehaviour
{
    [SerializeField] private Transform CenterPoint;
    
    [SerializeField] private float forceMagnitude = 10f;
    [SerializeField] private float impulseMagnitude = 5f;
    [SerializeField] private float jumpMagnitude = 1f;

    private Rigidbody rb;

    // input flagleri
    private bool jumpPressed;
    private bool holdSpace;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on this GameObject.");
        }

        CenterPoint = GameObject.FindWithTag("Center").transform;
    }

    private void Update()
    {
        // Input sadece Update’te okunmalı (FPS bağımlı değil)
        if (Input.GetKeyDown(KeyCode.Space) && SpheresManager.Instance.isGrounded)
        {
            jumpPressed = true;
        }

        holdSpace = Input.GetKey(KeyCode.Space);

        // gravity yönüne göre jumpMagnitude ayarı
        if (Physics.gravity.y > 0)
            jumpMagnitude = -Mathf.Abs(jumpMagnitude);
        else
            jumpMagnitude = Mathf.Abs(jumpMagnitude);
    }

    private void FixedUpdate()
    {
        Vector3 direction = (transform.position - CenterPoint.position).normalized;

        if (jumpPressed)
        {
            rb.AddForce(direction * impulseMagnitude, ForceMode.Impulse);
            rb.AddForce(Vector3.up * jumpMagnitude, ForceMode.Impulse);
            jumpPressed = false; // resetle
        }

        if (holdSpace)
        {
            // ForceMode.Force zaten deltaTime ile çarpıyor,
            // ama daha kontrollü olsun dersen sen de Time.fixedDeltaTime ile çarpabilirsin
            rb.AddForce(direction * forceMagnitude, ForceMode.Force);
        }
    }
}