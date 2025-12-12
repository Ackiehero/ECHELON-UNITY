using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class StockfishAPIResponse : MonoBehaviour
{
    public string move;        // Best move in LAN (e2e4)
    public float eval;         // Evaluation score from Stockfish
    public string[] continuation; // Principal variation / best line
}

public class StockfishAPI : MonoBehaviour
{
    public static StockfishAPI Instance;

    [Header("Stockfish API Settings")]
    public string apiUrl = "https://chess-api.com/v1"; // Replace with your actual API
    public int searchDepth = 14;
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

    /// <summary>
    /// Request best move from Stockfish API.
    /// </summary>
    /// <param name="fen">FEN string representing the current board</param>
    /// <param name="onMoveFound">Callback with the best move (e.g., "e2e4")</param>
    public void MakeMove(string fen, System.Action<string> onMoveFound)
    {
        if (isThinking)
        {
            onMoveFound?.Invoke(null);
            return;
        }

        StartCoroutine(RequestBestMoveCoroutine(fen, onMoveFound));
    }

    private IEnumerator RequestBestMoveCoroutine(string fen, System.Action<string> onMoveFound)
    {
        isThinking = true;

        if (showThinking)
            Debug.Log("AI thinking...");

        // Prepare JSON body
        string jsonBody = JsonUtility.ToJson(new StockfishAPIRequest
        {
            fen = fen,
            depth = searchDepth
        });

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogWarning("Stockfish API request failed: " + request.error);
                onMoveFound?.Invoke(null);
            }
            else
            {
                try
                {
                    StockfishAPIResponse response = JsonUtility.FromJson<StockfishAPIResponse>(request.downloadHandler.text);
                    onMoveFound?.Invoke(response?.move ?? null);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Failed to parse Stockfish API response: " + e.Message);
                    onMoveFound?.Invoke(null);
                }
            }
        }

        isThinking = false;
    }

    // Request body format for Stockfish API
    [System.Serializable]
    private class StockfishAPIRequest
    {
        public string fen;
        public int depth;
    }
}
