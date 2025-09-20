using UnityEngine;
using UnityEngine.Events;

public class ButtonAction : MonoBehaviour
{
    [SerializeField] UnityEvent onButtonPressed;
    [SerializeField] UnityEvent onButtonStay;
    [SerializeField] UnityEvent onButtonReleased;
    
    public void ButtonPressed()
    {
        onButtonPressed?.Invoke();
        print("pressed");
    }
    public void ButtonStay()
    {
        onButtonStay?.Invoke();
        print("stay");
    }
    public void ButtonReleased()
    {
        onButtonReleased?.Invoke();
        print("released");
    }
    
}
