using System;
using Unity.Cinemachine;
using UnityEngine;


public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float shakeIntensity = 3f;
    [SerializeField] private float shakeDuration = 0.2f;
    
    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer = 0f;
    private float shakeTimerTotal = 0f;
    private float startingIntensity = 0f;

    public static ScreenShake Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        noise =  GetComponent<CinemachineBasicMultiChannelPerlin>();
        
    }

    private void Update()
    {

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            noise.AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1f - (shakeTimer / shakeTimerTotal));
        }
    }

    public void ShakeScreen()
    {

        startingIntensity = shakeIntensity;
        
        shakeTimer = shakeDuration;
        shakeTimerTotal = shakeDuration;
        startingIntensity = shakeIntensity;
        
    }
}