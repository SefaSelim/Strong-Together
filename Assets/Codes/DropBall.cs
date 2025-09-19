using System.Collections.Generic;
using UnityEngine;

public class CubeGroupTrigger : MonoBehaviour
{
    [SerializeField] float MaxDropBallNumber;
    [Header("Temas kontrolü yapılacak kök (altındaki TÜM collider'lar geçerli sayılır)")]
    [SerializeField] private Transform groupRoot;
    bool isfinished = false;
      public GameObject prefab;

    float droppedball = 0;
    public CircleSpringSpawner3D_XY_Runtime circleSpringSpawner3D_XY_Runtime;
    private readonly HashSet<Collider> touching = new HashSet<Collider>();

    private void Awake()
    {
        if (!groupRoot)
        {
            Debug.LogError("CubeGroupTrigger: groupRoot atanmadı!");
            enabled = false;
        }
    }

    private bool IsAllowed(Collider other)
    {
        if (!other) return false;


        if (other.transform.IsChildOf(transform)) return false;


        return other.transform.IsChildOf(groupRoot);
    }

 
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Enter: {other.name}");
        if (IsAllowed(other))
            touching.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (touching.Remove(other))
            Debug.Log($"Exit: {other.name}");
    }

  
    private void OnDisable()
    {
        touching.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (touching.Count > 0 && droppedball < MaxDropBallNumber)
            {
                Debug.Log("Q basıldı: listedeki collider'lardan en az biriyle temas var.");
                circleSpringSpawner3D_XY_Runtime.numberOfSpheres--;
                Vector3 spawnposition = groupRoot.GetChild(1).position;
                Instantiate(prefab, spawnposition, groupRoot.GetChild(1).rotation);
                droppedball++;

            }

        }
        if (droppedball == MaxDropBallNumber && !isfinished)
        {

            Debug.Log("Aç baba kapıyı");
            isfinished = true;


                       //KAPI AÇILMA MEKANİĞİ VS.
        }
    }
}
