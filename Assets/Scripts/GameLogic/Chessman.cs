using UnityEngine;

public class Chessman : MonoBehaviour
{
    // References
    public GameObject chessman;
    public GameObject movePlate;

    // Positions
    private int xBoard = -1;
    private int yBoard = -1;

    // Variable for the color of the chessman;
    public string player;

    // References for all the sprites that the chessman can be;
    public Sprite b_king, w_king, b_queen, w_queen, b_rook, w_rook, b_bishop, w_bishop, b_knight, w_knight, b_pawn, w_pawn;

    // Declaration for the controller reference (this fixes CS0103)
    private GameController controller;

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        // Take the instantiated gameobject and set the position of the chessman;
        SetCoordinates();

        switch (this.name)
        {
            case "b_king": this.GetComponent<SpriteRenderer>().sprite = b_king; break;
            case "w_king": this.GetComponent<SpriteRenderer>().sprite = w_king; break;
            case "b_queen": this.GetComponent<SpriteRenderer>().sprite = b_queen; break;
            case "w_queen": this.GetComponent<SpriteRenderer>().sprite = w_queen; break;
            case "b_rook": this.GetComponent<SpriteRenderer>().sprite = b_rook; break;
            case "w_rook": this.GetComponent<SpriteRenderer>().sprite = w_rook; break;
            case "b_bishop": this.GetComponent<SpriteRenderer>().sprite = b_bishop; break;
            case "w_bishop": this.GetComponent<SpriteRenderer>().sprite = w_bishop; break;
            case "b_knight": this.GetComponent<SpriteRenderer>().sprite = b_knight; break;
            case "w_knight": this.GetComponent<SpriteRenderer>().sprite = w_knight; break;
            case "b_pawn": this.GetComponent<SpriteRenderer>().sprite = b_pawn; break;
            case "w_pawn": this.GetComponent<SpriteRenderer>().sprite = w_pawn; break;
            default:
                Debug.LogWarning("Unknown chessman type: " + this.name);
                break;
        }

        transform.localScale = new Vector3(2.5f, 2.5f, 1.0f);
    }

    public void SetCoordinates()
    {
        // Board-derived tile sizes (from 1572x1564 px @ 0.7 scale, PPU=100)
        float tileSizeX = ((1550f / 100f) * 0.7f) / 8f;  // ~1.3755 units wide per tile
        float tileSizeY = ((1550f / 100f) * 0.7f) / 8f;  // ~1.3685 units tall per tile

        // Optional: Add padding for piece gaps (reduce tile_size; e.g., 0.9f = 10% smaller tiles)
        float padding = 0.9f;  // Experiment: 1.0f (no padding), 0.8f (more space), 1.1f (tighter)
        tileSizeX *= padding;
        tileSizeY *= padding;

        // Center the grid on board: (board_coord - 3.5) * tile_size positions at tile centers
        float x = (xBoard - 3.5f) * tileSizeX;
        float y = (yBoard - 3.5f) * tileSizeY;

        // Bottom-center pivot: No extra offset needed (base sits at position).
        // Uncomment below if pieces appear too high/low (e.g., for center-pivot fallback):
        // y -= tileSizeY / 2f;  // Drops base to tile bottom

        transform.position = new Vector3(x, y, -1.0f);

        // Optional debug: Log position and bounds for tweaking
        // Debug.Log(name + " at board(" + xBoard + "," + yBoard + ") -> world(" + x + "," + y + "); Bounds: " + GetComponent<Renderer>().bounds.size);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            int sortingOrder = 80 - (yBoard * 10);  // Row 0=80 (front/top), Row 7=0 (back/bottom)
            // Optional x-tiebreaker: sortingOrder += (7 - xBoard);  // Left pieces slightly forward
            sr.sortingOrder = sortingOrder;
        }
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }
}
