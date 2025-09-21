using System.Collections.Generic;
using UnityEngine;

public class OnEnemyTouchSwapMaterial : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] string enemyTag = "Enemy";
    [SerializeField] bool useTrigger = true; // Trigger ise true, normal çarpýþma ise false

    [Header("Target")]
    [SerializeField] PerSphereTextureSwapper_FromList swapper;

    [Tooltip("Hepsi mi etkilensin, yoksa tek bir sphere mi?")]
    [SerializeField] bool affectAllSpheres = true;

    [Tooltip("Tek sphere modunda otomatik index bul (bu script hangi sphere'in altýnda ise onu bulur)")]
    [SerializeField] bool autoDetectSphereIndex = true;
    [SerializeField] int targetSphereIndex = 0;     // auto kapalýysa manuel index

    [Header("Hit Material")]
    [SerializeField] Material materialOnHit;
    [SerializeField] bool reapplyProps = true;

    // ---- runtime state ----
    HashSet<Collider> _activeEnemyColliders = new(); // ayný düþmanla birden çok event'te çift sayýmý engelle
    bool _applied = false;

    // snapshot (all)
    List<Material> _prevMatsAll;
    List<int> _prevIdxAll;

    // snapshot (one)
    int _resolvedIndex = -1;
    Material _prevMatOne;
    int _prevIdxOne = -1;

    void Awake()
    {
        if (swapper == null)
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            swapper = FindFirstObjectByType<PerSphereTextureSwapper_FromList>();
#else
            swapper = FindObjectOfType<PerSphereTextureSwapper_FromList>();
#endif
        }
    }

    // ---------- physics callbacks ----------
    void OnTriggerEnter(Collider other) { if (useTrigger) HandleEnter(other); }
    void OnTriggerExit(Collider other) { if (useTrigger) HandleExit(other); }
    void OnCollisionEnter(Collision c) { if (!useTrigger) HandleEnter(c.collider); }
    void OnCollisionExit(Collision c) { if (!useTrigger) HandleExit(c.collider); }

    void HandleEnter(Collider other)
    {
        if (swapper == null || materialOnHit == null) return;
        if (!other || !other.CompareTag(enemyTag)) return;

        // ayný collider ikinci kez sayýlmasýn
        if (!_activeEnemyColliders.Add(other)) return;

        // ilk düþman temasý ise uygula
        if (!_applied)
        {
            ApplyHitMaterial();
            _applied = true;
        }
    }

    void HandleExit(Collider other)
    {
        if (swapper == null) return;
        if (!other || !other.CompareTag(enemyTag)) return;

        // collider setinden çýkar
        _activeEnemyColliders.Remove(other);

        // set boþaldýysa geri dön
        if (_applied && _activeEnemyColliders.Count == 0)
        {
            RevertToPreviousMaterial();
            _applied = false;
        }
    }

    // ---------- apply / revert ----------
    void ApplyHitMaterial()
    {
        if (affectAllSpheres)
        {
            int n = swapper.SphereCount;
            _prevMatsAll = new List<Material>(n);
            _prevIdxAll = new List<int>(n);

            for (int i = 0; i < n; i++)
            {
                _prevMatsAll.Add(swapper.GetCurrentMaterialAsset(i)); // gerçek mevcut materyal
                _prevIdxAll.Add(swapper.GetCurrentMaterialIndex(i)); // liste indexi (varsa)
            }
            swapper.SetAllMaterial(materialOnHit, reapplyProps);
        }
        else
        {
            // index tespiti
            _resolvedIndex = autoDetectSphereIndex
                ? swapper.FindSphereIndexByTransform(transform, 10)
                : targetSphereIndex;

            if (_resolvedIndex < 0)
            {
                Debug.LogWarning($"[OnEnemyTouchSwapMaterial] Sphere index bulunamadý.");
                return;
            }

            _prevMatOne = swapper.GetCurrentMaterialAsset(_resolvedIndex);
            _prevIdxOne = swapper.GetCurrentMaterialIndex(_resolvedIndex);

            swapper.SetOneMaterialAsset(_resolvedIndex, materialOnHit, reapplyProps);
        }
    }

    void RevertToPreviousMaterial()
    {
        if (affectAllSpheres)
        {
            int n = swapper.SphereCount;
            if (_prevMatsAll == null || _prevIdxAll == null || _prevMatsAll.Count != n || _prevIdxAll.Count != n)
            {
                Debug.LogWarning("[OnEnemyTouchSwapMaterial] Önceki durum bulunamadý; geri dönüþ atlandý.");
                return;
            }

            for (int i = 0; i < n; i++)
            {
                var prevMat = _prevMatsAll[i];
                var prevIdx = _prevIdxAll[i];

                if (prevMat != null)
                    swapper.SetOneMaterialAsset(i, prevMat, true);
                else if (prevIdx >= 0)
                    swapper.ApplyMaterial(i, prevIdx, true);
            }
        }
        else
        {
            if (_resolvedIndex < 0) return;
            if (_prevMatOne != null)
                swapper.SetOneMaterialAsset(_resolvedIndex, _prevMatOne, true);
            else if (_prevIdxOne >= 0)
                swapper.ApplyMaterial(_resolvedIndex, _prevIdxOne, true);
        }
    }
}
