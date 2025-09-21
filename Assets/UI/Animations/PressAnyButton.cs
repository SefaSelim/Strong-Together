using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class PressAnyButtons : MonoBehaviour
{
    [Header("Kamera Ayarları")]
    public Transform cameraTransform;   // Kameranın transformu
    public float moveX = 5f;            // X ekseninde kayma miktarı
    public float moveZ = 5f;            // Z ekseninde uzaklaşma miktarı
    public float duration = 1f;         // Kamera animasyon süresi

    [Header("Canvas Fade")]
    public float fadeDuration = 0.2f;   // CanvasGroup alpha fade süresi

    private bool isMoving = false;
    private CanvasGroup canvasGroup;
    
    private bool isAnyButtonPressed = false;
    
    [SerializeField] private GameObject selectPanel;
    CanvasGroup selectCanvasGroup;

    private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        selectCanvasGroup = selectPanel.GetComponent<CanvasGroup>();
        if (selectCanvasGroup == null)
            selectCanvasGroup = selectPanel.AddComponent<CanvasGroup>();

        audioSource = GetComponent<AudioSource>();
        selectCanvasGroup.alpha = 0f; // Başlangıçta görünmez yap
        
    }

    void Update()
    {
        if (Input.anyKeyDown && !isMoving && !isAnyButtonPressed)
        {
            // 0.3 saniye sonra kamera hareketi
            Invoke("MoveCamera", 0.3f);

            // Canvas fade hemen başlasın
            StartCoroutine(FadeInCanvas());
            StartCoroutine(FadeOutCanvas(1f));
            
            isAnyButtonPressed = true; // Sadece bir kez tetiklenmesini sağla
        }
    }

    void MoveCamera()
    {
        if (cameraTransform == null) return;
        
        audioSource.PlayOneShot(audioClip);

        isMoving = true;

        Vector3 targetPos = cameraTransform.position + new Vector3(-moveX, 0, -moveZ);

        cameraTransform.DOMove(targetPos, duration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                isMoving = false; // animasyon bittiğinde tekrar tetiklenebilir
            });
    }

    IEnumerator FadeInCanvas()
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f; // tam olarak 0 yap
    }
    
    IEnumerator FadeOutCanvas(float waitTime)
    {
        // Bekleme süresi
        yield return new WaitForSeconds(waitTime);

        float startAlpha = selectCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            selectCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
            yield return null;
        }

        selectCanvasGroup.alpha = 1f; // tam olarak 1 yap
    }

}