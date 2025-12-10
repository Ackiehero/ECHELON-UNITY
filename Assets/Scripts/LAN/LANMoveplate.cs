using UnityEngine;
using Mirror;

public class LANMovePlate : NetworkBehaviour
{
    public GameObject reference = null;
    public bool attack = false;

    private int matrixX;
    private int matrixY;

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
        UpdatePosition();
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    private void UpdatePosition()
    {
        float tileSizeX = 1.3755f * 0.9f;
        float tileSizeY = 1.3685f * 0.9f;
        float offsetX = -4.826f;
        float offsetY = -4.789f;

        float posX = offsetX + (matrixX - 3.5f) * tileSizeX;
        float posY = offsetY + (matrixY - 3.5f) * tileSizeY;

        if (PlayerController.LocalPlayer != null && !PlayerController.LocalPlayer.isWhiteSide)
        {
            posX = -posX;
            posY = -posY;
        }

        transform.position = new Vector3(posX, posY, -3f);
    }

    private void OnMouseDown()
    {
        if (!PlayerController.LocalPlayer?.IsMyTurn() ?? false) return;

        LANChessman piece = reference.GetComponent<LANChessman>();
        if (piece == null) return;

        // Send move request to server via PlayerController
        PlayerController.LocalPlayer.CmdRequestMove(piece.netId, matrixX, matrixY, attack);
    }
}