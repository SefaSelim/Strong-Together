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

    void Update()
    {
        if (numberOfSpheres != lastSpawnCount)
        {
            RebuildSpheres();
            lastSpawnCount = numberOfSpheres;
        }
    }
    
    void FixedUpdate()
    {
        // A veya D basılıysa tüm toplara kuvvet uygula
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            float dir = Input.GetKey(KeyCode.A) ? 1f : -1f; // A = sola, D = sağa

            foreach (var sphere in spawnedSpheres)
            {
                if (sphere == null) continue;
                Rigidbody rb = sphere.GetComponent<Rigidbody>();

                // Merkezden bu topa olan yön
                Vector3 normal = (sphere.transform.position - (centerObject != null ? centerObject.position : transform.position)).normalized;

                // Normale dik (tangent) vektör → XY düzleminde
                Vector3 tangent = Vector3.Cross(Vector3.forward, normal).normalized;

                // Kuvvet uygula


                if (rb.angularVelocity.magnitude < 10f)
                { 
                    rb.AddForce(tangent * shiftForce * dir, ForceMode.Force);
                }
                rb.AddForce(- lineerForce * dir * Vector3.right, ForceMode.Force); // Z yönünde hafif kuvvet
            }
        }
    }


    void RebuildSpheres()
    {
        foreach (var s in spawnedSpheres)
        {
            if (s != null)
                Destroy(s);
        }
        spawnedSpheres.Clear();

        SpawnSpheres();
        ConnectSpheresWithSprings();
    }

    void SpawnSpheres()
    {
        float centerZ = transform.position.z;

        for (int i = 0; i < numberOfSpheres; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfSpheres;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius + transform.position.x,
                Mathf.Sin(angle) * radius + transform.position.y,
                centerZ
            );

            GameObject sphere = Instantiate(spherePrefab, pos, Quaternion.identity, transform);

            if (sphere.GetComponent<Rigidbody>() == null)
                sphere.AddComponent<Rigidbody>();
            
            sphere.GetComponent<Rigidbody>().mass = 5f;

            LimitDistanceToCenter ldc = sphere.AddComponent<LimitDistanceToCenter>();
            ldc.centerObject = centerObject != null ? centerObject : this.transform;
            ldc.maxDistance = maxDistanceToCenter;

            spawnedSpheres.Add(sphere);
        }
    }

    // Değişiklik burada
    void ConnectSpheresWithSprings()
    {
        for (int i = 0; i < spawnedSpheres.Count; i++)
        {
            // i. küreyi al
            Rigidbody rbA = spawnedSpheres[i].GetComponent<Rigidbody>();
            
            // i. küreyi, kendisi dışındaki diğer tüm kürelere bağla
            for (int j = 0; j < spawnedSpheres.Count; j++)
            {
                if (i == j) // Eğer aynı küreyse, atla
                    continue;

                // j. küreyi al
                Rigidbody rbB = spawnedSpheres[j].GetComponent<Rigidbody>();

                // Bağlantının zaten var olup olmadığını kontrol et
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
                    //sj.autoConfigureConnectedAnchor = false; // El ile ayarlanacağı için false yapıldı
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
}