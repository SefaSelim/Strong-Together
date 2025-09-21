using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{

    public void RestartLevel()
    {
        print("deneme");
          SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
