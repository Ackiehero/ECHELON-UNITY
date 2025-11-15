using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;

    GameObject reference = null;

    // Board position not World Position
    int matrixX;
    int matrixY;

    // false: movement. true: attacking
    public bool attack = false;

    void Start()
    {
        if (attack)
        {
            // Change to Red
            GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    void OnMouseUp()
    {
        // Fixed: Proper assignment and GetComponent<Game>()
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game game = controller.GetComponent<Game>();  // Cache for reuse

        string destroyedPieceName = "";  // Added: Track if king was destroyed

        if (attack)
        {
            // Fixed: Access GetPosition on Game component
            GameObject cp = game.GetPosition(matrixX, matrixY);
            if (cp != null)
            {
                destroyedPieceName = cp.name;  // Added: Store name before destroy
                Destroy(cp);
            }
        }

        // Fixed: Access SetPositionEmpty on Game
        game.SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(), reference.GetComponent<Chessman>().GetYBoard());

        // Fixed: SetYBoard (was duplicate SetXBoard); call SetCoordinates()
        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoordinates();

        // Fixed: Access SetPosition on Game
        game.SetPosition(reference);

        reference.GetComponent<Chessman>().DestroyMovePlates();

        // Added: Check for king capture after move
        if (!string.IsNullOrEmpty(destroyedPieceName) && destroyedPieceName.Contains("_king"))
        {
            game.EndGame(game.GetCurrentPlayer());  // End game with current player as winner
            return;  // Don't switch turns
        }

        // Fixed: Switch turns after successful move (skip if game over)
        if (!game.IsGameOver)
        {
            Debug.Log($"Move complete. Switching turn from {game.GetCurrentPlayer()} to {(game.GetCurrentPlayer() == "white" ? "black" : "white")}");
            game.NextTurn();
        }
    }

    public void SetCoordinates(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
}