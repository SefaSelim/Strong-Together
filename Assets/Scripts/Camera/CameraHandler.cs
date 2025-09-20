using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public enum CamTypes
{
    Orthographic,
    Perspective,
}

public class CameraManager : MonoBehaviour
{
    public CamTypes camType;
    private void Awake()
    {
        Camera mainCamera = Camera.main;

        if (!mainCamera.GetComponent<CinemachineBrain>())
        {
            mainCamera.AddComponent<CinemachineBrain>();
        }

        switch (camType)
        {
            case CamTypes.Orthographic:
                mainCamera.orthographic = true;
                GetComponentInChildren<CinemachineCamera>().LookAt = null;
                Destroy(GetComponentInChildren<CinemachineCamera>().GetComponentInChildren<CinemachineHardLookAt>());
                break;
            case CamTypes.Perspective:
                mainCamera.orthographic = false;
                break;
        }
    }
}
