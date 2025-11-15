using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Game : MonoBehaviour
{
    public GameObject Chesspiece;

    // Positions of the chessmen;
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    public string currentPlayer = "white";
    public bool IsGameOver { get; private set; } = false;  // Added: Track game over state
    
    private GameTimer gameTimer;  // Added: Reference to timer for deduction

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameTimer = GetComponent<GameTimer>();  // Added: Get timer reference
        if (gameTimer != null)
        {
            gameTimer.StartTimer();  // Added: Auto-start timers
        }

        playerWhite = new GameObject[]{
            Create("w_rook", 0, 0),
            Create("w_knight", 1, 0),
            Create("w_bishop", 2, 0),
            Create("w_queen", 3, 0),
            Create("w_king", 4, 0),
            Create("w_bishop", 5, 0),
            Create("w_knight", 6, 0),
            Create("w_rook", 7, 0),
            Create("w_pawn", 0, 1),
            Create("w_pawn", 1, 1),
            Create("w_pawn", 2, 1),
            Create("w_pawn", 3, 1),
            Create("w_pawn", 4, 1),
            Create("w_pawn", 5, 1),
            Create("w_pawn", 6, 1),
            Create("w_pawn", 7, 1),
        };
        playerBlack = new GameObject[]{
            Create("b_rook", 0, 7),
            Create("b_knight", 1, 7),
            Create("b_bishop", 2, 7),
            Create("b_queen", 3, 7),
            Create("b_king", 4, 7),
            Create("b_bishop", 5, 7),
            Create("b_knight", 6, 7),
            Create("b_rook", 7, 7),
            Create("b_pawn", 0, 6),
            Create("b_pawn", 1, 6),
            Create("b_pawn", 2, 6),
            Create("b_pawn", 3, 6),
            Create("b_pawn", 4, 6),
            Create("b_pawn", 5, 6),
            Create("b_pawn", 6, 6),
            Create("b_pawn", 7, 6),
        };

        // Fixed: Separate loops for white and black; i < length
        for (int i = 0; i < playerWhite.Length; i++)
        {
            SetPosition(playerWhite[i]);
        }
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
        }
    }

    void Update()  // Added: Deduct time only for current player each frame
    {
        if (IsGameOver || gameTimer == null) return;

        gameTimer.DeductTimeForCurrentPlayer(Time.deltaTime, currentPlayer);
    }
    
    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(Chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman c = obj.GetComponent<Chessman>();
        c.name = name;
        c.SetXBoard(x);
        c.SetYBoard(y);
        c.Activate();
        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman c = obj.GetComponent<Chessman>();
        positions[c.GetXBoard(), c.GetYBoard()] = obj;
    }

    // Episode 5
    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public GameObject GetPosition(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < 8 && y < 8)
        {
            return positions[x, y];
        }
        return null;
    }

    public bool PositionOnBoard(int x, int y)
    {
        // Fixed: Proper syntax - return false if out of bounds
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1))
            return false;
        return true;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void NextTurn()
    {
        if (IsGameOver) return;  // Added: Skip if game over

        if (currentPlayer == "white")
        {
            currentPlayer = "black";  // Fixed: = for assignment (was ==)
        }
        else
        {
            currentPlayer = "white";  // Fixed: = for assignment (was ==)
        }
    }

    // Added: End game on king capture, log win message, reset after 10s
    public void EndGame(string winner)
    {
        if (IsGameOver) return;  // Prevent multiple calls

        IsGameOver = true;
        Debug.Log($"{winner} wins!");  // Display "(Player) win!" in Console (swap for UI later)

        // Added: Pause timers on game over
        if (gameTimer != null)
        {
            gameTimer.PauseTimer();
        }

        StartCoroutine(ResetAfterDelay(10f));
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // Reload current scene
    }
}