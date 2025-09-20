using UnityEngine;

public class GravityControler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Physics.gravity = new Vector3(Physics.gravity.x,-Physics.gravity.y,Physics.gravity.z);
        }
    }

}
