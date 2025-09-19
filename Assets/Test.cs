using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(SpheresManager.Instance.Spheres.Count);
        }
    }
}
