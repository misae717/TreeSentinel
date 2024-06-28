using UnityEngine;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        // Singleton pattern to ensure only one BGMManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlayMusic()
    {
        if (instance.audioSource && !instance.audioSource.isPlaying)
        {
            instance.audioSource.Play();
        }
    }

    public static void StopMusic()
    {
        if (instance.audioSource && instance.audioSource.isPlaying)
        {
            instance.audioSource.Stop();
        }
    }

    public static void SetVolume(float volume)
    {
        if (instance.audioSource)
        {
            instance.audioSource.volume = volume;
        }
    }
}