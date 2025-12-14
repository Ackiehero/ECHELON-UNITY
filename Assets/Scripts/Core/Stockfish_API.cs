using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

public class Stockfish_API : MonoBehaviour
{
    public static Stockfish_API Instance;

    [Header("API Settings")]
    public string apiUrl = "https://chess-api.com/v1";
    public int depth = 14;
    public bool showThinking = true;

    public bool isThinking { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 🔹 PUBLIC ENTRY POINT
    public async void MakeMove(string fen, Action<string> onMoveFound)
    {
        if (isThinking)
        {
            Debug.LogWarning("Stockfish is already thinking.");
            return;
        }

        isThinking = true;

        if (showThinking)
            FindFirstObjectByType<GameLog>()?.LogMessage("AI thinking...");

        string move = await RequestBestMoveAsync(fen);

        isThinking = false;

        if (string.IsNullOrEmpty(move))
        {
            Debug.LogError("Stockfish API returned no move.");
            move = "e7e5"; // fallback move
        }

        // Apply move on main thread
        StartCoroutine(ApplyAIMove(move));

        // Callback for AIGameController
        onMoveFound?.Invoke(move);
    }

    // 🔹 ASYNC API REQUEST
    private async Task<string> RequestBestMoveAsync(string fen)
    {
        string json = $"{{\"fen\":\"{fen}\",\"depth\":{depth}}}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest req = new UnityWebRequest(apiUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        var op = req.SendWebRequest();
        while (!op.isDone)
            await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Stockfish API Error: {req.error}");
            return null;
        }

        string response = req.downloadHandler.text;
        Debug.Log("Stockfish API response: " + response);

        try
        {
            MoveResponse data = JsonUtility.FromJson<MoveResponse>(response);
            return data.move;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse best move: " + e);
            return null;
        }
    }

    [Serializable]
    private class MoveResponse
    {
        public string move;
    }

    // 🔹 APPLY MOVE ON MAIN THREAD
    private IEnumerator ApplyAIMove(string uci)
    {
        if (string.IsNullOrEmpty(uci) || uci.Length < 4)
            yield break;

        int fromX = uci[0] - 'a';
        int fromY = uci[1] - '1';
        int toX = uci[2] - 'a';
        int toY = uci[3] - '1';

        Game game = FindFirstObjectByType<Game>();
        if (game == null) yield break;

        GameObject pieceObj = game.GetPosition(fromX, fromY);
        if (pieceObj == null) yield break;

        Chessman piece = pieceObj.GetComponent<Chessman>();
        if (piece == null) yield break;

        // 1️⃣ Select piece for AI
        TierManager.Instance.SelectPieceForMovementByAI(piece);

        // 2️⃣ Trigger move prep
        piece.InitiateMove();

        // 🔹 NEW: Force castling plates for kings (AI-only)
        if (piece.name.Contains("king"))
        {
            CastlingManager castle = UnityEngine.Object.FindFirstObjectByType<CastlingManager>();
            castle?.ShowCastlePlates(piece);
        }

        // 3️⃣ Execute move via MovePlate
        MovePlate targetPlate = null;
        foreach (MovePlate plate in FindObjectsByType<MovePlate>(FindObjectsSortMode.None))
        {
            if (plate.GetX() == toX && plate.GetY() == toY)
            {
                targetPlate = plate;
                break;
            }
        }

        if (targetPlate != null)
            targetPlate.PerformMove();

        // 4️⃣ Deselect piece after move
        TierManager.Instance.DeselectPiece();
    }
}
