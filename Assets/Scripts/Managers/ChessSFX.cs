using UnityEngine;

public class ChessSFX : MonoBehaviour
{
    public static ChessSFX Instance;

    [Header("Echelon Tier Chess SFX")]
    public AudioClip chessSelectSFX;
    public AudioClip chessMove1SFX;
    public AudioClip chessMove2SFX;
    public AudioClip chessAttackSFX;

    [Header("Tier Upgrade SFX")]
    public AudioClip tierUp2SFX;   // Tier 1 to 2
    public AudioClip tierUp3SFX;   // Tier 2 to 3 (FINAL UPGRADE - EPIC!)

    private AudioSource audioSource;
    private System.Random rng = new System.Random();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public static void Select()  => Instance?.Play(Instance.chessSelectSFX);
    public static void Move()    => Instance?.PlayRandomMove();
    public static void Attack()  => Instance?.Play(Instance.chessAttackSFX);

    // FINAL UPGRADE SYSTEM - Tier 3 ALWAYS CUTS OFF Tier 2
    public static void TierUp(int newTier)
    {
        if (Instance == null || Instance.audioSource == null) return;

        AudioClip clip = newTier switch
        {
            2 => Instance.tierUp2SFX,
            3 => Instance.tierUp3SFX,
            _ => null
        };

        if (clip == null) return;

        // Tier 3 is FINAL - CUT OFF ANYTHING playing (especially Tier 2)
        if (newTier == 3)
        {
            Instance.audioSource.Stop();  // Instantly kills TierUp2_SFX
            Instance.audioSource.PlayOneShot(Instance.tierUp3SFX);
        }
        else
        {
            // Tier 2 - normal play (can be cut off later by Tier 3)
            Instance.audioSource.PlayOneShot(clip);
        }
    }

    private void Play(AudioClip clip)
    {
        if (clip && audioSource) audioSource.PlayOneShot(clip);
    }

    private void PlayRandomMove()
    {
        if (chessMove1SFX == null && chessMove2SFX == null) return;

        AudioClip chosen = (chessMove1SFX && chessMove2SFX)
            ? (rng.NextDouble() < 0.5 ? chessMove1SFX : chessMove2SFX)
            : (chessMove1SFX ?? chessMove2SFX);

        Play(chosen);
    }
}