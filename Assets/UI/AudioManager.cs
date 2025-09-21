using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    private AudioSource musicSource;
    
    public AudioClip buttonHoverSound;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        musicSource = GetComponent<AudioSource>();
    }
    
    public void PlayClip(AudioClip clip)
    {
        musicSource.PlayOneShot(clip);
    }
    
}
