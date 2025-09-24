using UnityEngine;
using UnityEngine.SceneManagement;

public class asd : MonoBehaviour
{
    void Start()
    {
        Invoke("Aasd",29);
    }
    public void Aasd()
    {
        SceneManager.LoadScene(0);
    }
}
