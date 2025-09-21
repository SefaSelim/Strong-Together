using UnityEngine;

/// <summary>
/// Varil yerde ve hareket halindeyken loop'lu yuvarlanma sesini �alar.
/// H�z d��t���nde sesi kapat�r. Zemin temas�n� �arp��ma noktalar�ndan takip eder.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BarrelRollingSFX : MonoBehaviour
{
    [Header("Ba�lant�lar")]
    [Tooltip("Varilin Rigidbody bile�eni (bo� b�rak�l�rsa otomatik bulunur).")]
    [SerializeField] private Rigidbody rb;
    [Tooltip("Varilin Collider'� (yar��ap tahmini i�in kullan�l�r).")]
    [SerializeField] private Collider col;
    [Tooltip("Ses ��k���. Bo�sa otomatik eklenir ve ayarlan�r.")]
    [SerializeField] private AudioSource audioSource;

    [Header("Ses Klipleri")]
    [Tooltip("Loop'lu yuvarlanma sesi (zorunlu).")]
    [SerializeField] private AudioClip rollingLoop;
    [Tooltip("�ste�e ba�l�: �lk �arp��mada bir 'thud' efekti.")]
    [SerializeField] private AudioClip impactOneShot;

    [Header("Zemin Ayar�")]
    [Tooltip("Hangi katmanlar� 'zemin' sayal�m?")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [Tooltip("Bir temas�n zemine say�lmas� i�in normalin yukar�ya yak�nl�k e�i�i (0�1).")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float groundUpDotMin = 0.35f;
    [Tooltip("Zeminden ayr�ld�ktan sonra sesi hemen kesmemek i�in bekleme (sn).")]
    [SerializeField] private float groundGraceTime = 0.15f;

    [Header("Hareket E�i�i (m/s tahmini)")]
    [Tooltip("Sesi ba�latmak i�in gereken h�z e�i�i.")]
    [SerializeField] private float startSpeed = 0.25f;
    [Tooltip("Sesi durdurmak i�in gereken h�z e�i�i (histerezis i�in daha d���k tut).")]
    [SerializeField] private float stopSpeed = 0.18f;
    [Tooltip("H�z�n normalize edildi�i yakla��k maksimum (volume/pitch skalas�).")]
    [SerializeField] private float maxSpeedForScaling = 6f;

    [Header("Ses Karakteri")]
    [Tooltip("Maksimum ses seviyesi.")]
    [Range(0f, 1f)][SerializeField] private float maxVolume = 0.6f;
    [Tooltip("H�za g�re min pitch.")]
    [SerializeField] private float minPitch = 0.9f;
    [Tooltip("H�za g�re max pitch.")]
    [SerializeField] private float maxPitch = 1.2f;
    [Tooltip("Volume/pitch de�i�iminde yumu�atma.")]
    [SerializeField] private float lerpSpeed = 10f;

    // Dahili
    private float approxRadius = 0.25f;
    private float lastGroundedTime = -999f;
    private bool grounded;
    private bool wantLoop;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!col) col = GetComponent<Collider>();

        // Yar��ap� collider bounding box'tan kaba tahmin et (silindirik varil varsay�m�).
        if (col)
        {
            var e = col.bounds.extents;
            approxRadius = Mathf.Max(e.x, e.z);
            approxRadius = Mathf.Clamp(approxRadius, 0.1f, 1.0f);
        }

        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;         // 3D ses
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 1.5f;
            audioSource.maxDistance = 20f;
        }
    }

    private void Start()
    {
        if (rollingLoop != null)
        {
            audioSource.clip = rollingLoop;
        }
        else
        {
            Debug.LogWarning($"[{nameof(BarrelRollingSFX)}] RollingLoop atanmad�!");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // �ste�e ba�l� tek seferlik darbe sesi (�r. oyuncu �arp�nca).
        if (impactOneShot && other.relativeVelocity.magnitude > 1.0f)
        {
            // Zeminle de�il, farkl� bir cisimle g��l� �arp��ma ise daha do�al durur.
            if (((1 << other.gameObject.layer) & groundLayers) == 0)
                AudioSource.PlayClipAtPoint(impactOneShot, other.GetContact(0).point, Mathf.Clamp01(maxVolume));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayers) == 0) return;

        foreach (var c in collision.contacts)
        {
            // Normal yukar�ya bak�yorsa ve zemin katman�nda ise zemindeyiz
            if (Vector3.Dot(c.normal, Vector3.up) >= groundUpDotMin)
            {
                grounded = true;
                lastGroundedTime = Time.time;
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayers) != 0)
        {
            grounded = false;
        }
    }

    private void Update()
    {
        // Yakla��k teker temas h�z�n� hesapla: lineer veya a��sal (tangential) h�zdan b�y�k olan�n� al.
        float linear = rb ? rb.linearVelocity.magnitude : 0f;
        float tangential = rb ? rb.angularVelocity.magnitude * approxRadius : 0f;
        float speed = Mathf.Max(linear, tangential);

        bool isGroundedNow = grounded || (Time.time - lastGroundedTime) <= groundGraceTime;

        // Histerezisli oynatma mant���
        if (isGroundedNow && speed >= startSpeed && rollingLoop)
            wantLoop = true;
        else if (speed <= stopSpeed || !isGroundedNow)
            wantLoop = false;

        if (wantLoop)
        {
            if (!audioSource.isPlaying && rollingLoop) audioSource.Play();

            // H�za g�re volume ve pitch
            float t = Mathf.Clamp01(speed / Mathf.Max(0.01f, maxSpeedForScaling));
            float targetVol = t * maxVolume;
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, t);

            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVol, Time.deltaTime * lerpSpeed);
            audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * lerpSpeed);
        }
        else
        {
            // Yumu�ak�a k�s, �ok d��t�yse tamamen kapat
            if (audioSource.isPlaying)
            {
                audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * lerpSpeed);
                if (audioSource.volume <= 0.02f) audioSource.Stop();
            }
        }

        // �er�eve sonunda grounded flag'ini bir sonraki OnCollisionStay'e kadar temizle
        grounded = false;
    }
}
