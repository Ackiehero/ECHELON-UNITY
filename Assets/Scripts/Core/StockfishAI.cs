using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class StockfishAI : MonoBehaviour
{
    public static StockfishAI Instance;

    [Header("Stockfish Settings")]
    public int searchDepth = 14;
    public int skillLevel = 18;
    public bool showThinking = true;

    private Process stockfishProcess;
    private StreamWriter inputWriter;
    private StreamReader outputReader;
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
        StartStockfish();
    }

    private void StartStockfish()
    {
        string exePath = GetStockfishExecutablePath();

        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
        {
            UnityEngine.Debug.LogError($"STOCKFISH NOT FOUND at: {exePath}");
            enabled = false;
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        try
        {
            stockfishProcess = Process.Start(psi);
            inputWriter = stockfishProcess.StandardInput;
            outputReader = stockfishProcess.StandardOutput;

            SendCommand("uci");
            SendCommand($"setoption name Skill Level value {skillLevel}");
            SendCommand("isready");

            UnityEngine.Debug.Log("Stockfish STARTED! Platform: " + Application.platform);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Failed to start Stockfish: " + e.Message);
            enabled = false;
        }
    }

    private string GetStockfishExecutablePath()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // PC/Editor
        return Path.Combine(Application.streamingAssetsPath, "Stockfish", "stockfish.exe");
#else
        // ANDROID: Extract from StreamingAssets → persistentDataPath + chmod
        string finalPath = Path.Combine(Application.persistentDataPath, "stockfish");

        if (!File.Exists(finalPath))
        {
            string assetPath = Path.Combine(Application.streamingAssetsPath, "Stockfish/stockfish-android-armv8");

            WWW www = new WWW(assetPath);
            while (!www.isDone) { } // Wait

            if (!string.IsNullOrEmpty(www.error))
            {
                UnityEngine.Debug.LogError("Failed to read Stockfish from APK: " + www.error);
                return null;
            }

            File.WriteAllBytes(finalPath, www.bytes);
            UnityEngine.Debug.Log("Stockfish extracted to: " + finalPath);

            // Make executable
            try
            {
                var chmod = new Process();
                chmod.StartInfo.FileName = "/system/bin/chmod";
                chmod.StartInfo.Arguments = "755 " + finalPath;
                chmod.StartInfo.UseShellExecute = false;
                chmod.StartInfo.CreateNoWindow = true;
                chmod.Start();
                chmod.WaitForExit();
                UnityEngine.Debug.Log("chmod 755 applied");
            }
            catch { }
        }

        return finalPath;
#endif
    }

    public async void MakeMove(string fen, System.Action<string> onMoveFound)
    {
        if (isThinking || stockfishProcess == null || stockfishProcess.HasExited) 
        {
            onMoveFound?.Invoke("e7e5");
            return;
        }

        isThinking = true;
        if (showThinking) FindFirstObjectByType<GameLog>()?.LogMessage("AI thinking...");

        SendCommand("position fen " + fen);
        SendCommand($"go depth {searchDepth}");

        string bestMove = await ReadBestMove();
        isThinking = false;
        onMoveFound?.Invoke(bestMove ?? "e7e5");
    }

    private async Task<string> ReadBestMove()
    {
        string bestMove = null;
        string line;

        while ((line = await outputReader.ReadLineAsync()) != null)
        {
            if (line.Contains("bestmove"))
            {
                bestMove = line.Split(' ')[1];
                break;
            }
        }

        // Consume all remaining lines to prevent blocking
        while (outputReader.Peek() >= 0)
            await outputReader.ReadLineAsync();

        return bestMove;
    }

    private void SendCommand(string cmd)
    {
        inputWriter?.WriteLine(cmd);
        inputWriter?.Flush();
    }

    private void OnDestroy()
    {
        if (stockfishProcess != null && !stockfishProcess.HasExited)
        {
            SendCommand("quit");
            stockfishProcess.Kill();
        }
    }
}