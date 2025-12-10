using UnityEngine;
using UnityEngine.UI;

public class UIGamePlay : MonoBehaviour
{
    [Header("Resign Button")]
    public Button resignButton;  // ← DRAG YOUR BUTTON HERE!

    private Game game;

    void Awake()
    {
        game = FindFirstObjectByType<Game>();
        if (game == null)
        {
            Debug.LogError("UIGamePlay: Game not found!");
            enabled = false;
            return;
        }

        // Setup resign button
        if (resignButton != null)
        {
            resignButton.onClick.RemoveAllListeners();
            resignButton.onClick.AddListener(ResignGame);
        }
    }

    private void ResignGame()
    {
        // White (player) surrenders → Black wins
        GameLog log = FindFirstObjectByType<GameLog>();
        if (log != null)
            log.LogMessage("White decided to surrender.");

        game.EndGame("black");  // Uses your existing EndGame system
    }
}