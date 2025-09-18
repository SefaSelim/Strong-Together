using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Rotate(Vector3.forward, -100 * speed * Time.deltaTime);
        }  
    }
}
