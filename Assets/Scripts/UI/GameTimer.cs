using UnityEngine;
using TMPro;  // Added: For TextMeshPro support
using System.Collections;

public class GameTimer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI textTimerTop;   // Black player's timer (top of screen)
    public TextMeshProUGUI textTimerBot;   // White player's timer (bottom of screen)

    [Header("Timer Settings")]
    public int totalMinutes = 10;  // 10 minutes per player
    public bool startOnAwake = true;  // Auto-start on Awake

    private float timeRemainingWhite = 600f;  // White's time (displayed on bottom)
    private float timeRemainingBlack = 600f;  // Black's time (displayed on top)
    private bool isRunning = false;
    private Coroutine timerCoroutine;

    void Awake()
    {
        // Auto-find UI Texts if not assigned
        if (textTimerTop == null)
            textTimerTop = GameObject.Find("text_timer_top").GetComponent<TextMeshProUGUI>();
        if (textTimerBot == null)
            textTimerBot = GameObject.Find("text_timer_bot").GetComponent<TextMeshProUGUI>();

        if (textTimerTop == null || textTimerBot == null)
        {
            Debug.LogError("GameTimer: Could not find text_timer_top or text_timer_bot in scene!");
            enabled = false;
            return;
        }

        // Initialize display
        UpdateTimerDisplay();
    }

    void Start()
    {
        if (startOnAwake)
            StartTimer();
    }

    public void StartTimer()
    {
        if (isRunning) return;

        isRunning = true;
        timerCoroutine = StartCoroutine(TimerRoutine());
        Debug.Log("GameTimer: Started 10-minute per-player timers (Top: Black, Bottom: White).");
    }

    public void PauseTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        isRunning = false;
        Debug.Log("GameTimer: Paused.");
    }

    public void ResumeTimer()
    {
        if (!isRunning)
        {
            timerCoroutine = StartCoroutine(TimerRoutine());
            isRunning = true;
            Debug.Log("GameTimer: Resumed.");
        }
    }

    public void ResetTimers()
    {
        PauseTimer();
        timeRemainingWhite = totalMinutes * 60f;
        timeRemainingBlack = totalMinutes * 60f;
        UpdateTimerDisplay();
        Debug.Log("GameTimer: Reset to 10 minutes each.");
    }

    private IEnumerator TimerRoutine()
    {
        while (isRunning)
        {
            // Fixed: No auto-decrement here—handled in Game.Update() for current player only
            // Just check for timeout (e.g., if a player runs out during their turn)

            // Check for time up (e.g., if <0, end game)
            if (timeRemainingWhite <= 0 || timeRemainingBlack <= 0)
            {
                EndGameOnTimeout();
                yield break;
            }

            yield return new WaitForSeconds(0.1f);  // Light check every 0.1s to avoid frame spam
        }
    }

    private void UpdateTimerDisplay()
    {
        // Format: MM:SS
        // Top: Black's time
        int minutesBlack = Mathf.FloorToInt(timeRemainingBlack / 60);
        int secondsBlack = Mathf.FloorToInt(timeRemainingBlack % 60);
        textTimerTop.text = string.Format("{0:00}:{1:00}", minutesBlack, secondsBlack);
        textTimerTop.color = (timeRemainingBlack < 30f) ? Color.red : Color.white;

        // Bottom: White's time
        int minutesWhite = Mathf.FloorToInt(timeRemainingWhite / 60);
        int secondsWhite = Mathf.FloorToInt(timeRemainingWhite % 60);
        textTimerBot.text = string.Format("{0:00}:{1:00}", minutesWhite, secondsWhite);
        textTimerBot.color = (timeRemainingWhite < 30f) ? Color.red : Color.white;
    }

    private void EndGameOnTimeout()
    {
        // Customize: e.g., determine winner by opponent's remaining time or draw
        string winner = timeRemainingWhite > timeRemainingBlack ? "White" : (timeRemainingBlack > timeRemainingWhite ? "Black" : "Draw");
        Debug.Log($"GameTimer: Time's up! {winner} wins by timeout.");

        // Integrate with Game.cs EndGame() if available
        Game game = Object.FindFirstObjectByType<Game>();  // Fixed: Use FindFirstObjectByType (deprecation fix)
        if (game != null)
        {
            game.EndGame(winner.ToLower());
        }

        PauseTimer();
    }

    // Fixed: Call from Game.cs Update() to deduct only current player's time
    public void DeductTimeForCurrentPlayer(float deltaTime, string currentPlayer)
    {
        if (!isRunning) return;

        if (currentPlayer == "white")
        {
            timeRemainingWhite = Mathf.Max(0f, timeRemainingWhite - deltaTime);  // White's time (bottom)
        }
        else  // black
        {
            timeRemainingBlack = Mathf.Max(0f, timeRemainingBlack - deltaTime);  // Black's time (top)
        }

        UpdateTimerDisplay();
    }
}