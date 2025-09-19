using System;
using UnityEngine;

public class ForceOutDirection : MonoBehaviour
{
    [SerializeField] private Transform CenterPoint;
    
    [SerializeField] private float forceMagnitude = 10f;
    Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on this GameObject.");
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 direction = (transform.position - CenterPoint.position).normalized;
            rb.AddForce(direction * forceMagnitude, ForceMode.Force);
        }  
    }
    
}
