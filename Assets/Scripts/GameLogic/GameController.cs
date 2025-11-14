using UnityEngine;

public class GameController : MonoBehaviour
{
    // Reference to the Game script (for accessing board state later)
    public Game game;

    void Start()
    {
        // Auto-link to the Game instance if not assigned in Inspector
        if (game == null)
        {
            game = FindFirstObjectByType<Game>();  // Updated: Fixes CS0618 warning
        }
    }

    void Update()
    {
        // Basic input handling placeholder (tutorial expands this for piece selection)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Clicked: " + hit.transform.name);  // For testing
            }
        }
    }

    // Example: Switch turns (add more as needed)
    public void SwitchTurns()
    {
        if (game != null)
        {
            game.currentPlayer = (game.currentPlayer == "white") ? "black" : "white";
            Debug.Log("Current player: " + game.currentPlayer);
        }
    }
}