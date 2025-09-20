using UnityEngine;
using TMPro;

public class SpeechBubbleEvent : MonoBehaviour
{
    [Header("Takip")]
    public Transform target;
    public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    public Camera cam;

    [Header("İçerik")]
    public TextMeshPro textTMP;
    public string message;

    [Header("Göster/Gizle")]
    public bool autoHide = true;
    public float lifetime = 2f;
    public bool destroyOnHide = true;

    float timer;
    bool isShowing;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        gameObject.SetActive(false); // prefab sahnede görünmesin
    }

    /// <summary>
    /// Prefab içindeki balonu verilen hedef ve mesaj ile etkinleştirip gösterir.
    /// (Canvas kapalıysa açar, balonu doğru pozisyona koyar.)
    /// </summary>
    public void ShowFor(Transform followTarget, string msg, Camera camOverride = null)
    {
        // Alanları ata
        target = followTarget;
        message = msg;
        if (camOverride != null) cam = camOverride;
        if (cam == null) cam = Camera.main;

        // Canvas kapalıysa aç
        // Not: Script genelde Canvas GameObject'inde durur.
        var canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.enabled = true;

        // Balon GO kapalıysa aç
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Metni yaz
        if (textTMP != null) textTMP.text = message;

        // Zamanlayıcı ve gösterim durumu
        isShowing = true;
        timer = lifetime;

        // İlk frame doğru yerde ve kameraya dönük olsun
        UpdatePositionAndBillboard();
    }

    public void TriggerShow()  // Eski kullanımın bozulmasın diye bırakıyorum
    {
        ShowFor(target, message, null);
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

        Vector3 basePos = target.position;
        if (target.TryGetComponent<Renderer>(out var r))
            basePos = r.bounds.center + Vector3.up * r.bounds.extents.y;

        transform.position = basePos + worldOffset;

        if (cam != null)
        {
            Vector3 dir = transform.position - cam.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
