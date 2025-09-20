using UnityEngine;

public class ButtonCheckpoint : MonoBehaviour
{
    private ButtonAction buttonAction;
    private void Start()
    {
        buttonAction = GetComponentInParent<ButtonAction>();
    }

    private void OnTriggerEnter(Collider other)
    {
        buttonAction?.ButtonPressed();
    }
    private void OnTriggerStay(Collider other)
    {
        buttonAction?.ButtonStay();
    }
    private void OnTriggerExit(Collider other)
    {
        buttonAction?.ButtonReleased();
    }
}
