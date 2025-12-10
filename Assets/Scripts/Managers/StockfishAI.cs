using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class StockfishAI : MonoBehaviour
{
    public static StockfishAI Instance;
    public int searchDepth = 14;
    public int skillLevel = 18;
    public bool showThinking = true;

    private Process stockfishProcess;
    private StreamWriter inputWriter;
    private StreamReader outputReader;
    public bool isThinking { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartStockfish();
    }

    private void StartStockfish()
    {
        string exePath = GetStockfishPath();

        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
        {
            UnityEngine.Debug.LogError("STOCKFISH NOT FOUND!\n" +
                "You must have these TWO files with EXACT names:\n" +
                "Assets/StreamingAssets/Stockfish/stockfish-pc.exe     ← for Windows/PC\n" +
                "Assets/StreamingAssets/Stockfish/stockfish-mobile     ← for Android/iOS (no .exe)");
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

            UnityEngine.Debug.Log($"Stockfish STARTED successfully: {Path.GetFileName(exePath)}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Failed to launch Stockfish: " + e.Message);
        }
    }

    private string GetStockfishPath()
    {
        string folder = Path.Combine(Application.streamingAssetsPath, "Stockfish");

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return Path.Combine(folder, "stockfish-pc.exe");

#else
        // Android / iOS
        string mobilePath = Path.Combine(Application.persistentDataPath, "stockfish-mobile");
        if (!File.Exists(mobilePath))
        {
            string source = Path.Combine(folder, "stockfish-mobile");
            if (File.Exists(source))
                File.Copy(source, mobilePath);
        }
        return File.Exists(mobilePath) ? mobilePath : null;
#endif
    }

    public async void MakeMove(string fen, System.Action<string> onMoveFound)
    {
        if (isThinking || stockfishProcess == null || stockfishProcess.HasExited) return;
        isThinking = true;
        if (showThinking) FindFirstObjectByType<GameLog>()?.LogMessage("AI thinking...");

        SendCommand("position fen " + fen);
        SendCommand($"go depth {searchDepth}");

        string line = await ReadLineContaining("bestmove");
        isThinking = false;

        if (line != null) onMoveFound?.Invoke(line);
    }

    private async Task<string> ReadLineContaining(string keyword)
    {
        string line;
        while ((line = await outputReader.ReadLineAsync()) != null)
            if (line.Contains(keyword))
                return line.Split(' ')[1];
        return null;
    }

    private void SendCommand(string cmd) => inputWriter?.WriteLine(cmd);

    private void OnDestroy()
    {
        if (stockfishProcess != null && !stockfishProcess.HasExited)
        {
            SendCommand("quit");
            stockfishProcess.Kill();
        }
    }
}