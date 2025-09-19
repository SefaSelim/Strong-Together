using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class CenterGrowSafe : MonoBehaviour
{
    [Header("Büyüme")]
    public float scaleMultiplier = 1.5f;   // W basılıyken kaç kat büyüsün
    public float tweenDuration   = 0.45f;  // büyüme/küçülme süresi
    public Ease ease             = Ease.InOutSine;

    // Dahili
    private Rigidbody rb;
    private SphereCollider col;
    private Vector3 baseScale;
    private float baseColRadius;

    private bool prevKinematic;
    private bool prevDetect;
    private RigidbodyConstraints prevConstraints;

    private float t;          // 0..1 (0=normal, 1=büyük)
    private Tween tTw;

    void Awake()
    {
        rb  = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();

        baseScale     = transform.localScale;
        baseColRadius = col.radius;

        // Stabilite önerisi
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) BeginTransition(open: true);
        if (Input.GetKeyUp(KeyCode.W))   BeginTransition(open: false);
    }

    void FixedUpdate()
    {
        // Tween aktifken her physics adımında ölçek uygula
        if (tTw != null && tTw.IsActive())
        {
            float ratio = Mathf.Lerp(1f, scaleMultiplier, t);

            // Transform büyürken collider da otomatik büyür -> radius’u sabit tutuyoruz
            transform.localScale = baseScale * ratio;
            col.radius = baseColRadius;

            Physics.SyncTransforms(); // anında PhysX’e bildir
        }
    }

    void BeginTransition(bool open)
    {
        // Geçiş başlamadan önce: fiziksel çakışmayı durdur
        prevKinematic   = rb.isKinematic;
        prevDetect      = rb.detectCollisions;
        prevConstraints = rb.constraints;

        rb.isKinematic      = true;   // çözümleyici yüklenmesin
        rb.detectCollisions = false;  // temas çözümü dursun
        rb.linearVelocity = Vector3.zero;   // artıkları temizle
        rb.angularVelocity = Vector3.zero;
        rb.constraints |= RigidbodyConstraints.FreezePosition; // yerinde kalsın

        float target = open ? 1f : 0f;

        tTw?.Kill();
        tTw = DOVirtual.Float(t, target, tweenDuration, v => t = v)
                .SetEase(ease)
                .OnComplete(EndTransition);
    }

    void EndTransition()
    {
        // Son ölçeği uygula ve temizle
        float ratio = Mathf.Lerp(1f, scaleMultiplier, t);
        transform.localScale = baseScale * ratio;
        col.radius = baseColRadius;
        Physics.SyncTransforms();

        // Eski fizik ayarlarına dön
        rb.detectCollisions = prevDetect;
        rb.isKinematic      = prevKinematic;
        rb.constraints      = prevConstraints;

        // Güvenlik: hızları sıfırla
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
