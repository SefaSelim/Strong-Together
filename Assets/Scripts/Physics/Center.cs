using UnityEngine;

public class CenterOfMassFollowerAuto : MonoBehaviour
{
    [Header("Parent objesi, altındaki tüm objeler COM için kullanılacak")]
    public Transform parentObjects;

    void FixedUpdate()
    {
        if (parentObjects == null || parentObjects.childCount == 0) return;

        Vector3 com = Vector3.zero;
        int count = 0;

        foreach (Transform child in parentObjects)
        {
            if (child.CompareTag("Balls"))
            {
                com += child.position;
                count++;
            }
        }

        if (count > 0)
        {
            com /= count;
            com.z = transform.position.z; // Z sabit
            transform.position = com;
        }
    }
}