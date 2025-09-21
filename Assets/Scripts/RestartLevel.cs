using NUnit.Framework;
using UnityEngine;

public class RestartLevel : MonoBehaviour
{
    public DeathManager deathManager;
    bool isrestarted = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Balls" && !isrestarted)
        {
            deathManager.RestartLevel();
            isrestarted = true;
        }
    }
}
