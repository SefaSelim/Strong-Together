using UnityEngine;

public class LimitDistanceToCenter : MonoBehaviour
{
    public Transform centerObject; // Mesafe kontrolü için center
    public float maxDistance = 1f; // maksimum uzaklık (metre cinsinden)

    void LateUpdate()
    {
        if (centerObject == null) return;

        Vector3 dir = transform.position - centerObject.position;
        float currentDist = dir.magnitude;

        if (currentDist > maxDistance)
        {
            // Mesafeyi maxDistance ile sınırla
            transform.position = centerObject.position + dir.normalized * maxDistance;
        }
    }
}