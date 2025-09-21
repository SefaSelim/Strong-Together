using UnityEngine;
using DG.Tweening;

public class CreditsScroller : MonoBehaviour
{
    [SerializeField] private RectTransform creditsPanel;   // İçinde yazıların olduğu panel
    [SerializeField] private float startY = -600f;         // Başlangıç pozisyonu (ekranın dışında)
    [SerializeField] private float endY = 600f;            // Bitiş pozisyonu
    [SerializeField] private float duration = 10f;         // Animasyon süresi
    [SerializeField] private Ease easeType = Ease.Linear;  // Hareket eğrisi (linear → sabit hız)

    private float totalDuration  = 10f;
    public bool isInMenu = true;

    private Tween currentTween;

    private void Awake()
    {
        if (creditsPanel == null)
            creditsPanel = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Credits animasyonunu başlatır. 
    /// Butondan çağırabilirsin.
    /// </summary>
    public void PlayCredits()
    {
        
        // Önce paneli başa al
        creditsPanel.anchoredPosition = new Vector2(creditsPanel.anchoredPosition.x, startY);

        // Eğer önceki animasyon varsa durdur
        currentTween?.Kill();

        // Yukarı doğru kaydır
        currentTween = creditsPanel.DOAnchorPosY(endY, duration)
                                   .SetEase(easeType)
                                   .SetUpdate(true) // unscaled time → pause olsa da çalışır
                                   .OnComplete(() =>
                                   {
                                       Debug.Log("Credits bitti!");
                                   });
    }

    private void Update()
    {
        if (Input.anyKeyDown && isInMenu)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnDisable()
    {
        // Tween varsa durdur
        currentTween?.Kill();
        currentTween = null;

        // Paneli başa al
        if (creditsPanel != null)
        {
            creditsPanel.anchoredPosition = new Vector2(creditsPanel.anchoredPosition.x, startY);
        }
    }
    
}
