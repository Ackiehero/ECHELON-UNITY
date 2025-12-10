using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameMusic : MonoBehaviour
{
    [Header("3 Random Background Tracks - Subtle Volume")]
    public AudioClip[] tracks = new AudioClip[3]; // Drag your 3 tracks here

    private AudioSource source;
    private int[] order = { 0, 1, 2 };
    private int index = 0;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;
        source.volume = 1f; // Calm & non-distracting

        // Start immediately when Game scene loads
        ShuffleAndPlay();
    }

    private void ShuffleAndPlay()
    {
        // Shuffle order
        for (int i = 2; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        index = 0;
        PlayCurrentTrack();
    }

    private void PlayCurrentTrack()
    {
        if (tracks.Length == 0 || tracks[order[index]] == null) return;

        source.clip = tracks[order[index]];
        source.Play();

        // Schedule next track exactly when this one ends
        Invoke(nameof(NextTrack), source.clip.length);
    }

    private void NextTrack()
    {
        index++;
        if (index >= 3)
        {
            ShuffleAndPlay(); // New random order every 3 tracks
        }
        else
        {
            PlayCurrentTrack();
        }
    }

    // Optional: Call this from your Game.cs when game ends
    public void StopMusic()
    {
        CancelInvoke();
        source.Stop();
    }

    // Safety cleanup
    private void OnDestroy()
    {
        CancelInvoke();
    }
}