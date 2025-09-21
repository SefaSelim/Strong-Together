using UnityEngine;

public class Final : MonoBehaviour
{
    bool isfinished = false;
    void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Balls" && !isfinished)
        {
            isfinished = true;
            Debug.Log("Level 1 Geçildi");
        }
    }
}
