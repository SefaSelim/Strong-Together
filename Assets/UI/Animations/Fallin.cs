using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SequentialDrop : MonoBehaviour
{
    [Header("Düşecek Objeler")]
    public List<GameObject> objectsToDrop;

    [Header("Ayarlar")]
    public float dropHeight = 10f;     // Objelerin yukarıdan başlama yüksekliği
    public float dropDuration = 1f;    // Her bir objenin düşme süresi
    public float initialDelay = 0.5f;  // İlk obje ile ikinci arasındaki bekleme
    public float delayDecreaseFactor = 0.9f; // Her seferinde gecikmeyi çarpacağımız oran


    private AudioSource audioSource;

    [SerializeField] private AudioClip LettersDropSoundEffect;
    void Start()
    {
        // Başlangıçta objeleri yukarıya taşı (görünmesinler)
        foreach (GameObject obj in objectsToDrop)
        {
            if (obj == null) continue;
            obj.transform.position += Vector3.up * dropHeight;
        }

        audioSource = GetComponent<AudioSource>();

        StartCoroutine(DropObjectsSequentially());
    }

    IEnumerator DropObjectsSequentially()
    {
        float currentDelay = initialDelay;

        foreach (GameObject obj in objectsToDrop)
        {
            if (obj == null) continue;

            Vector3 targetPos = obj.transform.position - Vector3.up * dropHeight;

            // DOTween ile sekmesiz düşür
            obj.transform.DOMove(targetPos, dropDuration)
                .SetEase(Ease.Linear) // sekme yok, düz iner
                .OnComplete(() =>
                {
                    // Tam yere değdiğinde ekranı salla
                    if (ScreenShake.Instance != null)
                        ScreenShake.Instance.ShakeScreen();
                    audioSource.PlayOneShot(LettersDropSoundEffect);
                });

            // Bekle
            yield return new WaitForSeconds(currentDelay);

            // Sonraki için delay'i azalt
            currentDelay *= delayDecreaseFactor;
        }
    }
}