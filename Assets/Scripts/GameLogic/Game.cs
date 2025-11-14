using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject Chesspiece;

    // Positions of the chessmen;
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    public string currentPlayer = "white";
    // private bool gameOver = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
}