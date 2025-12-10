using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class LANChessman : NetworkBehaviour
{
    // === Visuals (drag your sprites in Inspector exactly like original Chessman.cs) ===
    public Sprite b_king, w_king, b_queen, w_queen, b_rook, w_rook,
                  b_bishop, w_bishop, b_knight, w_knight, b_pawn, w_pawn;

    // === References ===
    public GameObject movePlatePrefab;   // Drag your MovePlate prefab here

    // === Piece Data ===
    [SyncVar] public string pieceType;   // "king", "queen", etc.
    [SyncVar] public string player;      // "white" or "black"
    [SyncVar] public int tier = 1;

    [SyncVar(hook = nameof(OnPositionChanged))] public int xBoard = -1;
    [SyncVar(hook = nameof(OnPositionChanged))] public int yBoard = -1;

    private SpriteRenderer sr;

    private void OnPositionChanged(int oldVal, int newVal) => SetCoordinates();

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetCoordinates();
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(string type, string color, int x, int y)
    {
        pieceType = type;
        player = color;
        xBoard = x;
        yBoard = y;

        name = $"{(color == "white" ? "w" : "b")}_{type}";

        // Set correct sprite
        string spriteName = $"{color[0]}_{type}";
        sr.sprite = spriteName switch
        {
            "w_king" => w_king, "b_king" => b_king,
            "w_queen" => w_queen, "b_queen" => b_queen,
            "w_rook" => w_rook, "b_rook" => b_rook,
            "w_bishop" => w_bishop, "b_bishop" => b_bishop,
            "w_knight" => w_knight, "b_knight" => b_knight,
            "w_pawn" => w_pawn, "b_pawn" => b_pawn,
            _ => w_pawn
        };

        sr.sortingOrder = 10;
        transform.localScale = Vector3.one * 2.5f;

        SetCoordinates();
    }

    // Auto-update position when SyncVars change
    private void OnXChanged(int oldX, int newX) => SetCoordinates();
    private void OnYChanged(int oldY, int newY) => SetCoordinates();

    public void SetCoordinates()
    {
        float tileSizeX = 1.3755f * 0.9f;
        float tileSizeY = 1.3685f * 0.9f;

        float offsetX = -4.826f;
        float offsetY = -4.789f;

        float posX = offsetX + (xBoard - 3.5f) * tileSizeX;
        float posY = offsetY + (yBoard - 3.5f) * tileSizeY;

        // Flip board for black player
        if (PlayerController.LocalPlayer != null && !PlayerController.LocalPlayer.isWhiteSide)
        {
            posX = -posX;
            posY = -posY;
            transform.localScale = new Vector3(-2.5f, -2.5f, 1f); // Flip visually
        }

        transform.position = new Vector3(posX, posY, -1f);
    }

    // === INPUT ===
    private void OnMouseDown()
    {
        if (!isClient || !PlayerController.LocalPlayer.IsMyTurn()) return;
        if (player != PlayerController.LocalPlayer.playerColorName) return;

        DestroyMovePlates();
        InitiateMovePlates();
    }

    private void InitiateMovePlates()
    {
        if (pieceType == "pawn") SpawnPawnMoves();
        // Add other pieces later (knight, king, etc.) — or just pawn for now
    }

    private void SpawnPawnMoves()
    {
        int forward = player == "white" ? 1 : -1;
        int y = yBoard + forward;

        if (LANGameManager.Instance.PositionOnBoard(xBoard, y) && LANGameManager.Instance.GetPosition(xBoard, y) == null)
            SpawnMovePlate(xBoard, y, false);

        // Double move
        if ((player == "white" && yBoard == 1) || (player == "black" && yBoard == 6))
        {
            int y2 = yBoard + (forward * 2);
            if (LANGameManager.Instance.GetPosition(xBoard, y) == null && LANGameManager.Instance.GetPosition(xBoard, y2) == null)
                SpawnMovePlate(xBoard, y2, false);
        }

        // Captures
        int[] dx = { -1, 1 };
        foreach (int d in dx)
        {
            int captureX = xBoard + d;
            if (LANGameManager.Instance.PositionOnBoard(captureX, y))
            {
                GameObject target = LANGameManager.Instance.GetPosition(captureX, y);
                if (target != null && target.GetComponent<LANChessman>().player != player)
                    SpawnMovePlate(captureX, y, true);
            }
        }
    }

    private void SpawnMovePlate(int mx, int my, bool attack)
    {
        GameObject mp = Instantiate(movePlatePrefab, Vector3.zero, Quaternion.identity);
        LANMovePlate script = mp.GetComponent<LANMovePlate>();
        script.SetReference(gameObject);
        script.SetCoords(mx, my);
        script.attack = attack;

        if (attack) mp.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.7f);

        NetworkServer.Spawn(mp);
    }

    public void DestroyMovePlates()
    {
        var plates = FindObjectsByType<LANMovePlate>(FindObjectsSortMode.None);
        foreach (var p in plates)
            if (p.reference == gameObject)
                NetworkServer.Destroy(p.gameObject);
    }

    // Getters
    public int GetX() => xBoard;
    public int GetY() => yBoard;
}