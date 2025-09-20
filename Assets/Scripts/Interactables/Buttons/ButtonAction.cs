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
    }
    public void ButtonStay()
    {
        onButtonStay?.Invoke();
    }
    public void ButtonReleased()
    {
        onButtonReleased?.Invoke();
    }
    
}
