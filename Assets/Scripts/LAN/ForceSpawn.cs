using UnityEngine;
using Mirror;

public class ForceSpawn : MonoBehaviour
{
    public GameObject ChesspiecePrefab;   // Drag your LAN_Chesspiece prefab here

    void Start()
    {
        // Only Host runs this
        if (!NetworkServer.active) return;

        // If pieces already exist → do nothing
        if (FindObjectsByType<LANChessman>(FindObjectsSortMode.None).Length > 0) return;

        Debug.Log("FORCE SPAWNING 32 PIECES — THIS WILL WORK 100%");

        // Back row pieces
        string[] backRow = { "rook", "knight", "bishop", "queen", "king", "bishop", "knight", "rook" };

        // White back row + pawns
        for (int x = 0; x < 8; x++)
        {
            Spawn(backRow[x], x, 0, "white");
            Spawn("pawn",     x, 1, "white");
        }

        // Black back row + pawns
        for (int x = 0; x < 8; x++)
        {
            Spawn(backRow[x], x, 7, "black");
            Spawn("pawn",     x, 6, "black");
        }
    }

    void Spawn(string type, int x, int y, string color)
    {
        GameObject go = Instantiate(ChesspiecePrefab, new Vector3(x, y, -1f), Quaternion.identity);
        LANChessman cm = go.GetComponent<LANChessman>();
        cm.Initialize(type, color, x, y);

        NetworkServer.Spawn(go);
        Debug.Log($"Spawned {color} {type} at ({x},{y})");
    }
}