using System;
using UnityEngine;

public class ForceOutDirection : MonoBehaviour
{
    [SerializeField] private Transform CenterPoint;
    
    [SerializeField] private float forceMagnitude = 10f;
    [SerializeField] private float impulseMagnitude = 5f;
    [SerializeField] private float jumpMagnitude = 1f;
    
    
    Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on this GameObject.");
        }
        
        CenterPoint = GameObject.FindWithTag("Center").transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))// isgroundedken calisacak
        {
            Vector3 direction = (transform.position - CenterPoint.position).normalized;
            rb.AddForce(direction * impulseMagnitude, ForceMode.Impulse);
            rb.AddForce(Vector3.up * jumpMagnitude ,ForceMode.Impulse);
        }
        
        if (Input.GetKey(KeyCode.Space)) 
        {
            Vector3 direction = (transform.position - CenterPoint.position).normalized;
            rb.AddForce(direction * forceMagnitude, ForceMode.Force);

        }
    }
    
}
