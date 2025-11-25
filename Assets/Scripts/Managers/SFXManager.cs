using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Default UI click sound")]
    public AudioClip clickSound;          // Drag your button sound here once!

    private AudioSource source;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        source = GetComponent<AudioSource>();
    }

    // This method will show up in the Inspector!
    public void PlayClick()
    {
        if (source && clickSound)
            source.PlayOneShot(clickSound);
    }

    // Optional: different sounds
    public void Play(AudioClip clip)
    {
        if (source && clip)
            source.PlayOneShot(clip);
    }
}