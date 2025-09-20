using UnityEngine;

public class DoorRotator : MonoBehaviour
{
    [Header("Angles (deg)")]
    [Tooltip("Kapının kapalıyken yerel Y açısı")]
    public float closedAngle = 0f;
    [Tooltip("Kapının açık hedef açısı (örn. 90)")]
    public float openAngle = 90f;

    [Header("Motion")]
    [Tooltip("Derece/saniye")]
    public float speed = 90f;
    [Tooltip("Pivot burada değilse kapıyı bu objeye child yapın ve burayı pivot/menteşe olarak verin.")]
    public Transform hinge;

    float _targetAngle;

    void Awake()
    {
        if (hinge == null) hinge = transform;
        _targetAngle = closedAngle;
        // İstersen başlangıçta kapıyı kapalı açıya sabitle:
        Vector3 e = hinge.localEulerAngles;
        e.y = closedAngle;
        hinge.localEulerAngles = e;
    }

    public void Open()  { _targetAngle = openAngle;  }
    public void Close() { _targetAngle = closedAngle; }

    void Update()
    {
        // Mevcut yerel Y açısını hedefe doğru taşı
        float currentY = hinge.localEulerAngles.y;
        float nextY = Mathf.MoveTowardsAngle(currentY, _targetAngle, speed * Time.deltaTime);

        Vector3 e = hinge.localEulerAngles;
        e.y = nextY;
        hinge.localEulerAngles = e;
    }
}
