using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class TierManager : MonoBehaviour
{
    public static TierManager Instance { get; private set; }

    [Header("DRAG THESE 2 ONLY")]
    public TextMeshProUGUI tokenNumberText;
    public Button upgradeButton;

    private int whiteTokens = 32;
    private int blackTokens = 32;
    private Chessman selectedPiece;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => UpgradePiece(selectedPiece));
            upgradeButton.interactable = false;
            upgradeButton.image.color = new Color(1, 1, 1, 0.5f);
        }

        UpdateTokenDisplay();
    }

    // Check turn EVERY FRAME — deselect if not your turn
    private void Update()
    {
        if (selectedPiece != null)
        {
            Game game = Object.FindFirstObjectByType<Game>();
            if (game.GetCurrentPlayer() != selectedPiece.player)
            {
                // NOT YOUR TURN → FORCE DESELECT
                DeselectPiece();
            }
        }
    }

    public void SelectPiece(Chessman piece)
    {
        if (selectedPiece != null && selectedPiece != piece)
            ClearTierMark(selectedPiece);

        selectedPiece = null;
        upgradeButton.interactable = false;
        upgradeButton.image.color = new Color(1, 1, 1, 0.5f);

        Game game = Object.FindFirstObjectByType<Game>();
        if (piece.player != game.GetCurrentPlayer()) return;

        selectedPiece = piece;
        ShowTierMark(piece);

        if (!piece.name.Contains("king") && piece.tier < 3)
        {
            int cost = GetCost(piece);
            int tokens = piece.player == "white" ? whiteTokens : blackTokens;
            if (tokens >= cost)
            {
                upgradeButton.interactable = true;
                upgradeButton.image.color = Color.white;
            }
        }
    }

    public void DeselectPiece()
    {
        if (selectedPiece != null)
            ClearTierMark(selectedPiece);

        selectedPiece = null;
        upgradeButton.interactable = false;
        upgradeButton.image.color = new Color(1, 1, 1, 0.5f);
    }

    public void UpgradePiece(Chessman piece)
    {
        if (selectedPiece == null || selectedPiece.tier >= 3 || selectedPiece.name.Contains("king")) return;

        int cost = GetCost(selectedPiece);
        ref int tokens = ref (selectedPiece.player == "white" ? ref whiteTokens : ref blackTokens);
        if (tokens < cost) return;

        tokens -= cost;
        selectedPiece.tier++;
        // PLAY UPGRADE SOUND BASED ON NEW TIER
        ChessSFX.TierUp(selectedPiece.tier);

        ClearTierMark(selectedPiece);
        ShowTierMark(selectedPiece);
        UpdateTokenDisplay();

        string color = selectedPiece.player == "white" ? "White" : "Black";
        Object.FindFirstObjectByType<GameLog>()?.LogMessage($"{color} upgraded a piece.");

        // Refresh button WITHOUT calling SelectPiece
        RefreshUpgradeButton();
    }

    private void RefreshUpgradeButton()
    {
        if (selectedPiece == null || selectedPiece.name.Contains("king") || selectedPiece.tier >= 3)
        {
            upgradeButton.interactable = false;
            upgradeButton.image.color = new Color(1, 1, 1, 0.5f);
            return;
        }

        int cost = GetCost(selectedPiece);
        ref int tokens = ref (selectedPiece.player == "white" ? ref whiteTokens : ref blackTokens);
        if (tokens >= cost)
        {
            upgradeButton.interactable = true;
            upgradeButton.image.color = Color.white;
        }
        else
        {
            upgradeButton.interactable = false;
            upgradeButton.image.color = new Color(1, 1, 1, 0.5f);
        }
    }

    private void ShowTierMark(Chessman piece)
    {
        ClearTierMark(piece);

        string roman = piece.tier switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            _ => "?"
        };

        GameObject mark = new GameObject("TierMark");
        mark.transform.SetParent(piece.transform, false);
        mark.transform.localPosition = new Vector3(0, -0.1f, 0);

        TextMeshPro tmp = mark.AddComponent<TextMeshPro>();
        tmp.text = roman;
        tmp.fontSize = 3;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.84f, 0f);
        tmp.fontStyle = FontStyles.Bold;
        tmp.outlineWidth = 0.3f;
        tmp.outlineColor = Color.black;
        tmp.sortingOrder = 200;
        tmp.rectTransform.sizeDelta = new Vector2(0.8f, 0.8f);
        tmp.rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    private void ClearTierMark(Chessman piece)
    {
        Transform old = piece.transform.Find("TierMark");
        if (old != null) Destroy(old.gameObject);
    }

    public void UpdateTokenDisplay()
    {
        // Try LAN mode first
        var lanManager = Object.FindFirstObjectByType<LANGameManager>();
        if (lanManager != null)
        {
            string currentPlayer = PlayerController.LocalPlayer?.playerColorName ?? "white";
            int tokens = currentPlayer == "white" ? whiteTokens : blackTokens;
            if (tokenNumberText != null)
                tokenNumberText.text = tokens.ToString();
            return;
        }

        // Fallback to single-player (old way)
        var game = Object.FindFirstObjectByType<Game>();
        if (game != null && tokenNumberText != null)
        {
            string currentPlayer = game.GetCurrentPlayer();
            int tokens = currentPlayer == "white" ? whiteTokens : blackTokens;
            tokenNumberText.text = tokens.ToString();
        }
    }

    private int GetCost(Chessman p)
    {
        string type = p.name.Replace("w_", "").Replace("b_", "");
        return type switch
        {
            "pawn" => 2,
            "rook" or "knight" or "bishop" => 3,
            "queen" => 5,
            _ => 999
        };
    }

    // FINAL FIX: Prevents editor spam when stopping play mode
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // AI BOSS SYSTEM USES THIS — PUBLIC ON PURPOSE
    public void UpgradePieceByAI(Chessman piece)
    {
        if (piece == null || piece.tier >= 3 || piece.name.Contains("king")) return;

        piece.tier++;
        ShowTierMark(piece);
        GameLog gl = FindFirstObjectByType<GameLog>();
        gl?.LogMessage("Black upgraded a piece.");
    }
}