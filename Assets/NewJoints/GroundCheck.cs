using UnityEngine;

public class GroundCheck : MonoBehaviour
{

    void OnTriggerStay(Collider other)
    {
        SpheresManager.Instance.isGrounded = true;
        Debug.Log("true");
    }
    void OnTriggerExit(Collider other)
    {
        SpheresManager.Instance.isGrounded = false;
          Debug.Log("false");
    }
}
