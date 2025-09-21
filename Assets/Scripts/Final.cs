using UnityEngine;
using UnityEngine.SceneManagement;

public class Final : MonoBehaviour
{
    [SerializeField] string sceneToLoad;  // Inspector’dan sahne adı
    [SerializeField] float loadDelay = 0f; // İstersen gecikme (sn)
    bool isFinished = false;

    void OnTriggerEnter(Collider other)
    {
        if (!isFinished && other.CompareTag("Balls"))
        {
            isFinished = true;
            if (loadDelay > 0f)
                Invoke(nameof(DoLoad), loadDelay);
            else
                DoLoad();
        }
    }

    void DoLoad()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            Debug.LogError("Final: 'sceneToLoad' boş. Inspector’dan sahne adını gir.");
    }
}
