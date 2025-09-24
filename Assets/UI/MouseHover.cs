using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class MouseHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Ayarları")]
    public float hoverScale = 1.2f;      // Mouse üzerine gelince büyüme
    public float normalScale = 1f;       // Normal boyut
    public float scaleDuration = 0.2f;   // Tween süresi

    [Header("CanvasGroup Ayarları")]
    public float hoverAlpha = 1f;        // Mouse üzerine geldiğinde alpha
    public float normalAlpha = 0.5f;     // Normal alpha
    public float alphaDuration = 0.2f;   // Tween süresi

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Büyüt
        transform.DOScale(hoverScale, scaleDuration).SetEase(Ease.OutQuad);

        // Alpha artır
        canvasGroup.DOFade(hoverAlpha, alphaDuration).SetEase(Ease.OutQuad);
        
        AudioManager.Instance.PlayClip( AudioManager.Instance.buttonHoverSound );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Eski boyuta geri dön
        transform.DOScale(normalScale, scaleDuration).SetEase(Ease.OutQuad);

        // Alpha eski değere dön
        canvasGroup.DOFade(normalAlpha, alphaDuration).SetEase(Ease.OutQuad);
    }
}