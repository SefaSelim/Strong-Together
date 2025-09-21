using UnityEngine;

public class ShakeTest : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ScreenShake.Instance.ShakeScreen();
        }
    }
}
