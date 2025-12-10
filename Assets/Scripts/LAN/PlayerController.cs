using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SyncVar] public bool isWhiteSide;
    [SyncVar] public string playerColorName;

    public static PlayerController LocalPlayer { get; private set; }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            LocalPlayer = this;
            Debug.Log($"You are {playerColorName.ToUpper()}");
        }
    }

    public bool IsMyTurn()
    {
        var gm = Object.FindFirstObjectByType<LANGameManager>();
        return gm != null && gm.CurrentTurn == playerColorName;
    }

    [Command]
    public void CmdRequestMove(uint pieceNetId, int targetX, int targetY, bool isAttack)
    {
        if (!NetworkServer.active) return;

        // CORRECT way in Mirror 2023+
        if (!NetworkServer.spawned.TryGetValue(pieceNetId, out NetworkIdentity identity))
            return;

        LANChessman piece = identity.GetComponent<LANChessman>();
        if (piece == null) return;

        if (piece.player != playerColorName || LANGameManager.Instance.CurrentTurn != playerColorName)
            return;

        LANGameManager.Instance.ServerPerformMove(piece, targetX, targetY, isAttack);
    }
}