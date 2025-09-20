using UnityEngine;

public class GroundCheck : MonoBehaviour
{

    void OnTriggerStay(Collider other)
    {
        SpheresManager.Instance.isGrounded = true;
    }
    void OnTriggerExit(Collider other)
    {
        SpheresManager.Instance.isGrounded = false;
    }
}
