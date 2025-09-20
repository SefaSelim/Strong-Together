using UnityEngine;

public class FollowParentNoRotation : MonoBehaviour
{
    private Transform parent;
    private Quaternion initialRotation;

    void Start()
    {
        parent = transform.parent;
        initialRotation = transform.rotation; // Başlangıç açısını kaydet
    }

    void LateUpdate()
    {
        if (parent == null) return;

        // Parent'ın pozisyonunu takip et
        transform.position = parent.position;

        // Rotation sabit kalsın
        transform.rotation = initialRotation;
    }
}