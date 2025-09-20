using UnityEngine;
using TMPro;

public class SpeechBubbleEvent : MonoBehaviour
{
    [Header("Takip")]
    public Transform target;                   // Hangi küreyi takip edecek
    public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    public Camera cam;

    [Header("İçerik")]
    public TextMeshPro textTMP;            // Child TMP (UI)
    [TextArea] public string message = "Merhaba!"; // Inspector’dan yazacağın metin

    [Header("Göster/Gizle")]
    public bool autoHide = true;
    public float lifetime = 2f;
    public bool destroyOnHide = true;          // İstersen kapatmak yerine yok etsin

    float timer;
    bool isShowing;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        // Başta gizli kalmak istersen:
        gameObject.SetActive(false);
    }

    // Bu metodu event’te çağır: (UnityEvent, Animation Event, başka script vs.)
    public void TriggerShow()
    {
        if (textTMP != null) textTMP.text = message;
        isShowing = true;
        timer = lifetime;
        gameObject.SetActive(true);

        // ilk frame’de doğru yerde olsun
        UpdatePositionAndBillboard();
    }

    void LateUpdate()
    {
        if (!isShowing) return;
        UpdatePositionAndBillboard();

        if (autoHide && lifetime > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isShowing = false;
                if (destroyOnHide) Destroy(gameObject);
                else gameObject.SetActive(false);
            }
        }
    }

    void UpdatePositionAndBillboard()
    {
        if (target == null) return;

        // hedefin üst noktası:
        Vector3 basePos = target.position;
        if (target.TryGetComponent<Renderer>(out var r))
            basePos = r.bounds.center + Vector3.up * r.bounds.extents.y;

        transform.position = basePos + worldOffset;

        // kameraya dönük kalsın (yatay billboard)
        if (cam != null)
        {
            Vector3 dir = transform.position - cam.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
