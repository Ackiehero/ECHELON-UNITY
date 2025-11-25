using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameMusic : MonoBehaviour
{
    [Header("In-Game Background Music (Subtle Volume)")]
    public AudioClip[] musicTracks = new AudioClip[3];  // Drag your 3 tracks here

    private AudioSource audioSource;
    private int[] shuffleOrder = new int[3];  // Random order: 0,1,2 shuffled
    private int currentTrackIndex = 0;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;  // Individual tracks don't loop
        audioSource.volume = 0.25f;  // SUBTLE - won't distract from SFX/chess

        ShuffleTracks();
        PlayNextTrack();
    }

    void ShuffleTracks()
    {
        // Create shuffled order: [0,1,2] → random like [2,0,1]
        for (int i = 0; i < 3; i++) shuffleOrder[i] = i;
        
        // Fisher-Yates shuffle (perfect randomness)
        for (int i = 2; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = shuffleOrder[i];
            shuffleOrder[i] = shuffleOrder[j];
            shuffleOrder[j] = temp;
        }
    }

    void PlayNextTrack()
    {
        if (musicTracks.Length == 0 || shuffleOrder.Length == 0) return;

        audioSource.clip = musicTracks[shuffleOrder[currentTrackIndex]];
        audioSource.Play();

        // Schedule next track when this one finishes
        Invoke(nameof(PlayNextTrack), audioSource.clip.length);
        
        currentTrackIndex++;
        if (currentTrackIndex >= 3)
        {
            currentTrackIndex = 0;
            ShuffleTracks();  // New random order for next loop
        }
    }

    // Optional: Stop music when game ends
    public void StopMusic()
    {
        CancelInvoke(nameof(PlayNextTrack));
        audioSource.Stop();
    }
}