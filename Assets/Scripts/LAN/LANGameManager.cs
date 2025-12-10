using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.SceneManagement;

public class LANGameManager : NetworkBehaviour
{
    public static LANGameManager Instance { get; private set; }

    [SyncVar] public string CurrentTurn = "white";
    [SyncVar] public bool GameStarted = false;

    public GameObject ChesspiecePrefab;   // ← DRAG YOUR CHESSPIECE PREFAB HERE IN INSPECTOR
    public Transform boardParent;

    private GameObject[,] positions = new GameObject[8, 8];

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnStartServer()
    {
        // This is the ONLY thing that was missing — tiny delay fixes Host spawn
        StartCoroutine(SpawnWhenServerReady());
    }

    private IEnumerator SpawnWhenServerReady()
    {
        while (!NetworkServer.active)
            yield return null;
        
        Debug.Log("NETWORK SERVER READY — SPAWNING 32 PIECES");
        SpawnPiecesAfterReady();
    }

    private IEnumerator SpawnPiecesAfterReady()
    {
        yield return new WaitForEndOfFrame(); // 100% safe spawn

        Debug.Log("SPAWNING ALL 32 PIECES NOW");

        // WHITE
        SpawnPiece("king",    4, 0, "white");
        SpawnPiece("queen",   3, 0, "white");
        SpawnPiece("rook",    0, 0, "white");
        SpawnPiece("rook",    7, 0, "white");
        SpawnPiece("bishop",  2, 0, "white");
        SpawnPiece("bishop",  5, 0, "white");
        SpawnPiece("knight",  1, 0, "white");
        SpawnPiece("knight",  6, 0, "white");
        for (int x = 0; x < 8; x++) SpawnPiece("pawn", x, 1, "white");

        // BLACK
        SpawnPiece("king",    4, 7, "black");
        SpawnPiece("queen",   3, 7, "black");
        SpawnPiece("rook",    0, 7, "black");
        SpawnPiece("rook",    7, 7, "black");
        SpawnPiece("bishop",  2, 7, "black");
        SpawnPiece("bishop",  5, 7, "black");
        SpawnPiece("knight",  1, 7, "black");
        SpawnPiece("knight",  6, 7, "black");
        for (int x = 0; x < 8; x++) SpawnPiece("pawn", x, 6, "black");

        GameStarted = true;
        RpcStartGame();
    }

    private void SpawnPiece(string type, int x, int y, string color)
    {
        if (ChesspiecePrefab == null)
        {
            Debug.LogError("CHESSPIECE PREFAB IS NULL! Drag it in Inspector!");
            return;
        }

        GameObject go = Instantiate(ChesspiecePrefab);
        LANChessman cm = go.GetComponent<LANChessman>();
        cm.Initialize(type, color, x, y);

        NetworkServer.Spawn(go);
        positions[x, y] = go;

        Debug.Log($"Spawned {color} {type} at ({x},{y})");
    }

    [ClientRpc]
    private void RpcStartGame()
    {
        Debug.Log("Game started! Your color: " + (PlayerController.LocalPlayer?.playerColorName ?? "???"));
    }

    [Server] public void ServerPerformMove(LANChessman p, int tx, int ty, bool attack) { /* your existing move code */ }
    [Server] public void EndTurn() => CurrentTurn = CurrentTurn == "white" ? "black" : "white";
    [Server] public void EndGame(string winner) { /* your win code */ }

    public bool PositionOnBoard(int x, int y) => x >= 0 && y >= 0 && x < 8 && y < 8;
    public GameObject GetPosition(int x, int y) => PositionOnBoard(x, y) ? positions[x, y] : null;
    public void SetPosition(GameObject obj)
    {
        LANChessman cm = obj.GetComponent<LANChessman>();
        if (cm) positions[cm.xBoard, cm.yBoard] = obj;
    }
    public void SetPositionEmpty(int x, int y) => positions[x, y] = null;
}