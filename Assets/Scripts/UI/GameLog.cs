using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;  // Added: For coroutines

public class GameLog : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect gameLogScroll;  // Assign GameLogScroll in Inspector
    public Game game;  // Auto-find Game component
    public GameObject textLogPrefab;  // Added: Public prefab reference for TextMeshPro log entry (drag your Text - TextMeshPro prefab here)

    private Transform content;
    private GameObject[,] positionSnapshot = new GameObject[8, 8];
    private string lastPlayer = "";
    private int logCounter = 0;
    private VerticalLayoutGroup layoutGroup;
    private bool isInitialized = false;  // Added: Flag to prevent early logs

    void Start()
    {
        // Auto-find GameLogScroll and Game if not assigned
        if (gameLogScroll == null)
            gameLogScroll = GameObject.Find("GameLogScroll").GetComponent<ScrollRect>();
        if (game == null)
            game = Object.FindFirstObjectByType<Game>();  // Fixed: Use FindFirstObjectByType (deprecation fix)

        if (gameLogScroll == null || game == null)
        {
            Debug.LogError("GameLog: Could not find GameLogScroll or Game component!");
            enabled = false;
            return;
        }

        if (textLogPrefab == null)
        {
            Debug.LogError("GameLog: textLogPrefab not assigned! Create and drag a TextMeshPro prefab.");
            enabled = false;
            return;
        }

        content = gameLogScroll.content;
        layoutGroup = content.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
        }

        // Added: Delay initialization to allow board setup in Game.Start()
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.5f);  // Delay 0.5s for pieces to settle

        // Initial snapshot after board setup
        TakePositionSnapshot();

        // Initial log: [White]'s Turn!
        LogMessage("[White]'s Turn!");
        lastPlayer = "white";
        isInitialized = true;  // Added: Now allow Update() logs
    }

    void Update()
    {
        if (!isInitialized || game.IsGameOver) return;  // Added: Skip until init and if over

        // Fixed: Check for position changes FIRST (move detection before turn change)
        if (HasPositionChanged())
        {
            DetectAndLogMove();
            TakePositionSnapshot();  // Update snapshot after logging
        }

        // Then check for turn change
        if (game.GetCurrentPlayer() != lastPlayer)
        {
            LogMessage($"[{Capitalize(game.GetCurrentPlayer())}]'s Turn!");
            lastPlayer = game.GetCurrentPlayer();
        }
    }

    private void TakePositionSnapshot()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                positionSnapshot[x, y] = game.GetPosition(x, y);
            }
        }
    }

    private bool HasPositionChanged()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (positionSnapshot[x, y] != game.GetPosition(x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void DetectAndLogMove()
    {
        // Find moved piece and target position
        int toX = -1, toY = -1;
        GameObject movedPiece = null;
        GameObject capturedPiece = null;

        // Find 'to' position (new piece where snapshot was empty or different)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject snapshotPiece = positionSnapshot[x, y];
                GameObject currentPiece = game.GetPosition(x, y);

                if (snapshotPiece != currentPiece)
                {
                    if (currentPiece != null && snapshotPiece == null)
                    {
                        // Regular move to empty
                        toX = x;
                        toY = y;
                        movedPiece = currentPiece;
                        break;
                    }
                    else if (currentPiece != null && snapshotPiece != null && snapshotPiece.GetComponent<Chessman>().player != currentPiece.GetComponent<Chessman>().player)
                    {
                        // Capture: snapshot had enemy, now has mover
                        toX = x;
                        toY = y;
                        movedPiece = currentPiece;
                        capturedPiece = snapshotPiece;
                        break;
                    }
                }
            }
            if (toX != -1) break;
        }

        if (movedPiece == null || toX == -1) return;  // No valid move detected

        // Fixed: Attribute to PREVIOUS player (mover), since turn has switched after move
        string mover = (game.GetCurrentPlayer() == "white") ? "black" : "white";
        string playerColor = Capitalize(mover);

        // Piece type for mover: e.g., "w_pawn" → "Pawn"
        string pieceName = movedPiece.name.Replace("w_", "").Replace("b_", "");
        if (string.IsNullOrEmpty(pieceName)) return;
        string pieceType = Capitalize(pieceName);

        // Algebraic notation for 'to': e.g., E4 (file A=0 + x, rank = y+1)
        char file = (char)('A' + toX);
        string rank = (toY + 1).ToString();
        string toNotation = $"{file}{rank}";

        string logMessage;
        if (capturedPiece != null)
        {
            // Capture log: [White] captured Pawn on E4.
            string capturedName = capturedPiece.name.Replace("w_", "").Replace("b_", "");
            string capturedType = Capitalize(capturedName);
            logMessage = $"[{playerColor}] captured {capturedType} on {toNotation}.";
        }
        else
        {
            // Regular move log: [White] moved Pawn to E4.
            logMessage = $"[{playerColor}] moved {pieceType} to {toNotation}.";
        }

        LogMessage(logMessage);
    }

    private void LogMessage(string message)
    {
        logCounter++;

        // Instantiate prefab for log entry
        GameObject logEntry = Instantiate(textLogPrefab, content);
        logEntry.name = $"LogEntry_{logCounter}";  // Set name

        // Set size: Width 600, Height 50
        RectTransform logRect = logEntry.GetComponent<RectTransform>();
        logRect.sizeDelta = new Vector2(600f, 50f);  // Fixed: Set explicit size

        // Get TextMeshProUGUI and set text/font
        TextMeshProUGUI tmpText = logEntry.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"{logCounter}. {message}";
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.fontSize = 32;  // Fixed: Set font size to 32
            tmpText.color = Color.white;
            tmpText.margin = new Vector4(5, 5, 5, 5);  // Padding

            // Auto-size disabled for fixed size; adjust if needed
            tmpText.enableAutoSizing = false;
        }

        // Rebuild layout and scroll to bottom
        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        Canvas.ForceUpdateCanvases();
        gameLogScroll.verticalNormalizedPosition = 0f;  // Scroll to bottom
    }

    private string Capitalize(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str.Substring(1).ToLower();
    }
}