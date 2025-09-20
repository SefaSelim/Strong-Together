using UnityEngine;
using System.Collections.Generic;

public class CircleSpringSpawner3D_XY_Runtime : MonoBehaviour
{
    [Header("Prefab ve ayarlar")]
    public GameObject spherePrefab;
    [Range(1, 50)]
    public int numberOfSpheres = 6;
    public float radius = 2f;

    [Header("Spring Joint Ayarları")]
    public float spring = 50f;
    public float damper = 5f;
    public float distanceMultiplier = 1f;

    [Header("Limit Distance")]
    public Transform centerObject;
    public float maxDistanceToCenter = 1f;
    
    [Header("Oteleme Kuvveti")]
    public float shiftForce = 30f;
    public float lineerForce = 1f;

    private List<GameObject> spawnedSpheres = new List<GameObject>();
    private int lastSpawnCount = 0;

    // --- YENİ: Sadece spawn noktası için canlı merkez ---
    private Vector3 currentSpawnCenter;

    void Awake()
    {
        // başlangıçta verdiğin merkez ne ise onu al
        currentSpawnCenter = centerObject != null ? centerObject.position : transform.position;
    }

    void Update()
    {
        // Top sayısı değiştiyse yeniden kur (MERKEZİ O ANKİ KONUMA GÖRE!)
        if (numberOfSpheres != lastSpawnCount)
        {
            RebuildSpheres();
            lastSpawnCount = numberOfSpheres;
        }

        // --- YENİ: Sadece spawn merkezi için topların anlık ortalamasını kaydet ---
        if (spawnedSpheres.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            int c = 0;
            foreach (var s in spawnedSpheres)
            {
                if (s == null) continue;
                sum += s.transform.position;
                c++;
            }
            if (c > 0)
                currentSpawnCenter = sum / c; // SADECE sonraki respawn için kullanıyoruz
        }
    }
    
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            float dir = Input.GetKey(KeyCode.A) ? 1f : -1f;

            foreach (var sphere in spawnedSpheres)
            {
                if (sphere == null) continue;
                Rigidbody rb = sphere.GetComponent<Rigidbody>();

                Vector3 normal = (sphere.transform.position - (centerObject != null ? centerObject.position : transform.position)).normalized;
                Vector3 tangent = Vector3.Cross(Vector3.forward, normal).normalized;

                if (rb.angularVelocity.magnitude < 10f)
                { 
                    rb.AddForce(tangent * shiftForce * dir, ForceMode.Force);
                }
                rb.AddForce(- lineerForce * dir * Vector3.right, ForceMode.Force);
            }
        }
    }

    void RebuildSpheres()
    {
        // --- YENİ: Silmeden önce o anki topların ortasını spawn merkezi olarak kaydet ---
        if (spawnedSpheres.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            int c = 0;
            foreach (var s in spawnedSpheres)
            {
                if (s == null) continue;
                sum += s.transform.position;
                c++;
            }
            if (c > 0)
                currentSpawnCenter = sum / c;
        }

        foreach (var s in spawnedSpheres)
        {
            if (s != null)
                Destroy(s);
        }
        spawnedSpheres.Clear();

        SpawnSpheres();              // <-- artık currentSpawnCenter etrafında spawn eder
        ConnectSpheresWithSprings(); // aynı
    }

    void SpawnSpheres()
    {
        // --- ESKİ: transform.position yerine sadece spawn merkezi değişti ---
        float centerZ = currentSpawnCenter.z;

        for (int i = 0; i < numberOfSpheres; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfSpheres;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius + currentSpawnCenter.x,
                Mathf.Sin(angle) * radius + currentSpawnCenter.y,
                centerZ
            );

            GameObject sphere = Instantiate(spherePrefab, pos, Quaternion.identity, transform);
            sphere.name = "Sphere_" + i;

            if (sphere.GetComponent<Rigidbody>() == null)
                sphere.AddComponent<Rigidbody>();
            
            sphere.GetComponent<Rigidbody>().mass = 5f;

            spawnedSpheres.Add(sphere);
        }

        // --- YENİ: Spawn sonrası bir kez daha ortalamayı alıp kayıt (opsiyonel ama tutarlı) ---
        if (spawnedSpheres.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            int c = 0;
            foreach (var s in spawnedSpheres)
            {
                if (s == null) continue;
                sum += s.transform.position;
                c++;
            }
            if (c > 0)
                currentSpawnCenter = sum / c;
        }
    }

    // Değiştirilmedi
    void ConnectSpheresWithSprings()
    {
        for (int i = 0; i < spawnedSpheres.Count; i++)
        {
            Rigidbody rbA = spawnedSpheres[i].GetComponent<Rigidbody>();
            
            for (int j = 0; j < spawnedSpheres.Count; j++)
            {
                if (i == j) continue;

                Rigidbody rbB = spawnedSpheres[j].GetComponent<Rigidbody>();

                bool connectionExists = false;
                SpringJoint[] joints = spawnedSpheres[i].GetComponents<SpringJoint>();
                foreach (var joint in joints)
                {
                    if (joint.connectedBody == rbB)
                    {
                        connectionExists = true;
                        break;
                    }
                }

                if (!connectionExists)
                {
                    SpringJoint sj = spawnedSpheres[i].AddComponent<SpringJoint>();
                    sj.connectedBody = rbB;
                    sj.anchor = Vector3.zero;
                    sj.connectedAnchor = Vector3.zero;
                    sj.spring = spring;
                    sj.damper = damper;
                    sj.minDistance = 0;
                    sj.maxDistance = Vector3.Distance(rbA.position, rbB.position) * distanceMultiplier;
                }
            }
        }
    }
    
    public List<GameObject> GetListOfSpheres()
    {
        return spawnedSpheres;
    }
}
