using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    [Header("Drag your music clip here")]
    public AudioClip musicClip;

    [Header("Optional: Drag your Toggle here")]
    public Toggle musicToggle;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // Always start ON when entering menu
        if (musicClip != null)
        {
            audioSource.Play();
        }

        // Reset toggle to ON (or leave it alone if no toggle)
        if (musicToggle != null)
        {
            musicToggle.isOn = true;
            musicToggle.onValueChanged.RemoveAllListeners();
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }
    }

    public void ToggleMusic(bool isOn)
    {
        if (isOn)
            audioSource.Play();
        else
            audioSource.Stop();
    }

    // Stops music automatically when leaving the menu scene
    void OnDestroy()
    {
        audioSource.Stop();
    }
}