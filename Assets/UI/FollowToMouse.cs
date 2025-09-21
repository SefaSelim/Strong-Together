using UnityEngine;

public class SpotlightMouseFollow : MonoBehaviour
{
    public Camera cam;         // Ortographic veya Perspective kamera olabilir
    public LayerMask groundMask; // Ray'in çarpacağı layer (mesela "Ground")

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundMask))
        {
            // Ray'in çarptığı noktaya bak
            transform.LookAt(hit.point);
        }
    }
}