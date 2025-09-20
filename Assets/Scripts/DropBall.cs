using System.Collections.Generic;
using UnityEngine;

public class CubeGroupTrigger : MonoBehaviour
{
    [SerializeField] int MaxDropBallNumber = 1;            // int kullan
    [Header("Temas kontrolü yapılacak kök (altındaki TÜM collider'lar geçerli sayılır)")]
    [SerializeField] private Transform groupRoot;

    public GameObject prefab;
    public CircleSpringSpawner3D_XY_Runtime circleSpringSpawner3D_XY_Runtime;

    private readonly HashSet<Collider> touching = new HashSet<Collider>();
    private Collider triggerCol;
    private int droppedball = 0;
    private bool isfinished = false;

    private void Awake()
    {
        triggerCol = GetComponent<Collider>();
        if (!triggerCol || !triggerCol.isTrigger)
            Debug.LogWarning("CubeGroupTrigger: Üzerindeki collider yok ya da isTrigger değil.");

        if (!groupRoot)
        {
            Debug.LogError("CubeGroupTrigger: groupRoot atanmadı!");
            enabled = false;
        }
    }

    private bool IsAllowed(Collider other)
    {
        if (!other) return false;
        if (other.transform.IsChildOf(transform)) return false; // kendini sayma
        return other.transform.IsChildOf(groupRoot);            // sadece groupRoot altı
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsAllowed(other))
            touching.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        touching.Remove(other);
    }

    private void OnDisable()
    {
        touching.Clear();
    }

    // Hâlâ üst üste mi? (yanlış-pozitifleri temizler)
    private bool IsOverlapping(Collider c)
    {
        if (c == null || triggerCol == null) return false;
        Vector3 dir; float dist;
        return Physics.ComputePenetration(
            triggerCol, triggerCol.transform.position, triggerCol.transform.rotation,
            c,         c.transform.position,         c.transform.rotation,
            out dir, out dist
        );
    }

    // Bayat entry’leri temizle (disable/destroy/ayrılmış olanlar)
    private void PruneTouching()
    {
        touching.RemoveWhere(c =>
            c == null ||
            !c.enabled ||
            !c.gameObject.activeInHierarchy ||
            !IsAllowed(c) ||
            !IsOverlapping(c)
        );
    }

    private void Update()
    {
        // her frame gerçek temasları doğrula
        PruneTouching();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (touching.Count > 0 && droppedball < MaxDropBallNumber)
            {
                Debug.Log("Q basıldı: listedeki collider'lardan en az biriyle temas var.");

                if (circleSpringSpawner3D_XY_Runtime != null)
                    circleSpringSpawner3D_XY_Runtime.numberOfSpheres = 
                        Mathf.Max(0, circleSpringSpawner3D_XY_Runtime.numberOfSpheres - 1);

                // Spawn noktası
                var spawnTf = groupRoot.GetChild(1);
                Instantiate(prefab, spawnTf.position, spawnTf.rotation);

                droppedball++;
            }
        }

        if (droppedball >= MaxDropBallNumber && !isfinished)
        {
            Debug.Log("Aç baba kapıyı");
            isfinished = true;
            // KAPI AÇILMA MEKANİĞİ
        }
    }
}
