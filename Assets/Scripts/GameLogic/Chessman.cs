using UnityEngine;

public class Chessman : MonoBehaviour
{
    // References
    public GameObject chessman;
    public GameObject movePlate;

    // === TIER SYSTEM ===
    public int tier = 1;  // Default Tier 1, max 3

    // Positions
    private int xBoard = -1;
    private int yBoard = -1;

    // Variable for the color of the chessman;
    public string player;

    // References for all the sprites that the chessman can be;
    public Sprite b_king, w_king, b_queen, w_queen, b_rook, w_rook, b_bishop, w_bishop, b_knight, w_knight, b_pawn, w_pawn;

    // Direct reference to Game (no GameController needed)
    private Game game;

    public void Activate()
    {
        // Fetch GameObject, then directly get Game component (no GameController)
        GameObject controllerGO = GameObject.FindGameObjectWithTag("GameController");
        if (controllerGO == null)
        {
            Debug.LogError("GameController GameObject not found! Create one and tag it 'GameController'.");
            return;
        }
        game = controllerGO.GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("Game component missing on 'GameController' GameObject! Attach Game.cs.");
            return;
        }

        // Set player color based on name (for captures)
        player = this.name.StartsWith("w") ? "white" : "black";

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

        // Add padding for piece gaps (reduce tile_size; e.g., 0.9f = 10% smaller tiles)
        float padding = 0.9f;  // Experiment: 1.0f (no padding), 0.8f (more space), 1.1f (tighter)
        tileSizeX *= padding;
        tileSizeY *= padding;

        // Center the grid on board: (board_coord - 3.5) * tile_size positions at tile centers
        float x = (xBoard - 3.5f) * tileSizeX;
        float y = (yBoard - 3.5f) * tileSizeY;        

        transform.position = new Vector3(x, y, -1.0f);

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

    private void OnMouseUp()
    {
        if (game == null || game.IsGameOver || game.GetCurrentPlayer() != player) return;

        // Play Select SFX
        ChessSFX.Select();

        // SELECT PIECE FOR UPGRADE
        TierManager.Instance.SelectPiece(this);

        // CLEAN OLD PLATES (DO NOT DESELECT!)
        DestroyMovePlates();

        // SHOW NEW MOVES
        InitiateMoveplates();
    }

    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject plate in movePlates)
            Destroy(plate.gameObject);
    }

    public void InitiateMoveplates()
    {
        switch (this.name)
        {
            case "b_queen":
            case "w_queen":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(1, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                LineMovePlate(-1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(1, -1);
                break;
            case "b_knight":
            case "w_knight":
                LMovePlate();
                break;
            case "b_bishop":
            case "w_bishop":
                LineMovePlate(1,1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, -1);
                break;
            case "b_king":
            case "w_king":
                if (this.name.Contains("king"))
                {
                    KingSafetyManager ksm = FindFirstObjectByType<KingSafetyManager>();
                    if (ksm != null)
                    {
                        ksm.GenerateSafeKingMoves(this);
                    }
                }
                else
                {
                    SurroundMovePlate();
                }
                
                FindFirstObjectByType<CastlingManager>()?.ShowCastlePlates(this);
                break;
            case "b_rook":
            case "w_rook":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1); 
                LineMovePlate(-1, 0); 
                LineMovePlate(0, -1); 
                break;    
            case "b_pawn":
                PawnMovePlate();  // No params; handles single/double/attacks inside
                break;
            case "w_pawn":
                PawnMovePlate();  // No params; handles single/double/attacks inside
                break;
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        // Direct use of 'game' reference (no need for gameObject.GetComponent)
        if (game == null)
        {
            Debug.LogError("Game reference lost!");
            return;
        }

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (game.PositionOnBoard(x, y) && game.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement; 
        }

        if (game.PositionOnBoard(x, y) && game.GetPosition(x, y) != null)
        {
            Chessman target = game.GetPosition(x, y).GetComponent<Chessman>();
            if (target != null && target.player != player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }
    }

    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }

    public void SurroundMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard);
        PointMovePlate(xBoard + 1, yBoard + 1);
    }

    public void PointMovePlate(int x, int y)
    {
        // Direct use of 'game' reference
        if (game == null)
        {
            Debug.LogError("Game reference lost!");
            return;
        }

        if (game.PositionOnBoard(x, y))
        {
            GameObject cp = game.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            } 
            else 
            {
                Chessman target = cp.GetComponent<Chessman>();
                if (target != null && target.player != player)
                {
                    MovePlateAttackSpawn(x, y);
                }
            }
        }
    }

    // Parameterless; handles single forward, optional double (first move), and diagonal attacks
    public void PawnMovePlate()
    {
        // Direct use of 'game' reference
        if (game == null)
        {
            Debug.LogError("Game reference lost!");
            return;
        }

        int forward = (player == "white") ? 1 : -1;
        int startRow = (player == "white") ? 1 : 6;
        int singleY = yBoard + forward;
        int doubleY = yBoard + (forward * 2);
        bool isFirstMove = (yBoard == startRow);

        // Single forward move (always available if empty)
        if (game.PositionOnBoard(xBoard, singleY) && game.GetPosition(xBoard, singleY) == null)
        {
            MovePlateSpawn(xBoard, singleY);
        }

        // Double forward move (only on first move, if both squares empty)
        if (isFirstMove && game.PositionOnBoard(xBoard, doubleY) && 
            game.GetPosition(xBoard, singleY) == null && 
            game.GetPosition(xBoard, doubleY) == null)
        {
            MovePlateSpawn(xBoard, doubleY);
            Debug.Log($"{this.name} can double-move to ({xBoard}, {doubleY})");  // Optional debug
        }

        // Diagonal attacks (always on forward row, if enemy present)
        int attackY = singleY;  // Attacks are always single-step forward
        if (game.PositionOnBoard(xBoard + 1, attackY) && game.GetPosition(xBoard + 1, attackY) != null)
        {
            Chessman target = game.GetPosition(xBoard + 1, attackY).GetComponent<Chessman>();
            if (target != null && target.player != player)
            {
                MovePlateAttackSpawn(xBoard + 1, attackY);
            }
        }

        if (game.PositionOnBoard(xBoard - 1, attackY) && game.GetPosition(xBoard - 1, attackY) != null)
        {
            Chessman target = game.GetPosition(xBoard - 1, attackY).GetComponent<Chessman>();
            if (target != null && target.player != player)
            {
                MovePlateAttackSpawn(xBoard - 1, attackY);
            }
        }
    }

    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        float tileSizeX = ((1550f / 100f) * 0.7f) / 8f;
        float tileSizeY = ((1550f / 100f) * 0.7f) / 8f;

        float padding = 0.9f;
        tileSizeX *= padding;
        tileSizeY *= padding;
        
        float posX = (matrixX - 3.5f) * tileSizeX;
        float posY = (matrixY - 3.5f) * tileSizeY;

        // For Display on Unity Game Screen
        GameObject mp = Instantiate(movePlate, new Vector3(posX, posY, -3.0f), Quaternion.identity);

        // To keep track inside script
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoordinates(matrixX, matrixY);  
    }

    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        float tileSizeX = ((1550f / 100f) * 0.7f) / 8f;
        float tileSizeY = ((1550f / 100f) * 0.7f) / 8f;

        float padding = 0.9f;
        tileSizeX *= padding;
        tileSizeY *= padding;
        
        float posX = (matrixX - 3.5f) * tileSizeX;
        float posY = (matrixY - 3.5f) * tileSizeY;

        // For Display on Unity Game Screen
        GameObject mp = Instantiate(movePlate, new Vector3(posX, posY, -3.0f), Quaternion.identity);

        // To keep track inside script
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoordinates(matrixX, matrixY);  
    }

    public void InitiateMove()
    {
        DestroyMovePlates();
        InitiateMoveplates();
    }

    // KingSafetymanager signals
    public bool WouldAttack(int targetX, int targetY, int fromX, int fromY)
    {
        // Simulate attack logic without spawning plates
        int dx = targetX - fromX;
        int dy = targetY - fromY;

        switch (name.Replace("w_", "").Replace("b_", ""))
        {
            case "pawn":
                int direction = player == "white" ? 1 : -1;
                return Mathf.Abs(dx) == 1 && dy == direction;

            case "knight":
                return (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 2) ||
                    (Mathf.Abs(dx) == 2 && Mathf.Abs(dy) == 1);

            case "bishop":
                if (Mathf.Abs(dx) != Mathf.Abs(dy)) return false;
                return LineOfSightClear(fromX, fromY, targetX, targetY);

            case "rook":
                if (dx != 0 && dy != 0) return false;
                return LineOfSightClear(fromX, fromY, targetX, targetY);

            case "queen":
                if (dx != 0 && dy != 0 && Mathf.Abs(dx) != Mathf.Abs(dy)) return false;
                return LineOfSightClear(fromX, fromY, targetX, targetY);

            case "king":
                return Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1;

            default:
                return false;
        }
    }

    private bool LineOfSightClear(int fromX, int fromY, int toX, int toY)
    {
        int dx = (int)Mathf.Sign(toX - fromX);
        int dy = (int)Mathf.Sign(toY - fromY);
        int steps = Mathf.Max(Mathf.Abs(toX - fromX), Mathf.Abs(toY - fromY));

        if (dx == 0 && dy == 0) return true; // same square (shouldn't happen)

        for (int i = 1; i < steps; i++)
        {
            int checkX = fromX + i * dx;
            int checkY = fromY + i * dy;
            if (FindFirstObjectByType<Game>().GetPosition(checkX, checkY) != null)
                return false;
        }
        return true;
    }
}