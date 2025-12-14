using UnityEngine;

public class KingSafetyManager : MonoBehaviour
{
    private Game game;

    void Awake()
    {
        game = FindFirstObjectByType<Game>();
        if (game == null)
        {
            Debug.LogError("KingSafetyManager: Game component not found!");
            enabled = false;
        }
    }

    public bool IsSquareAttacked(int tx, int ty, string attackingPlayer)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject pieceObj = game.GetPosition(x, y);
                if (pieceObj == null) continue;

                Chessman cm = pieceObj.GetComponent<Chessman>();
                if (cm == null || cm.player != attackingPlayer) continue;

                // Simulate if this piece could attack (tx, ty)
                if (cm.WouldAttack(tx, ty, x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void GenerateSafeKingMoves(Chessman king)
    {
        int x = king.GetXBoard();
        int y = king.GetYBoard();
        string opponent = king.player == "white" ? "black" : "white";

        int[,] directions = new int[,] {
            {-1, -1}, {-1, 0}, {-1, 1},
            { 0, -1},          { 0, 1},
            { 1, -1}, { 1, 0}, { 1, 1}
        };

        for (int i = 0; i < 8; i++)
        {
            int tx = x + directions[i, 0];
            int ty = y + directions[i, 1];

            if (!game.PositionOnBoard(tx, ty)) continue;

            GameObject target = game.GetPosition(tx, ty);
            Chessman targetCm = target ? target.GetComponent<Chessman>() : null;

            bool isOpponentKing = targetCm != null &&
                                  targetCm.name.Contains("king") &&
                                  targetCm.player == opponent;

            // Kings cannot move adjacent to each other
            if (isOpponentKing) continue;

            game.SetPosition(king.gameObject);

            bool safe = !IsSquareAttacked(tx, ty, opponent);

            // Restore king's position
            game.SetPosition(king.gameObject);

            if (!safe) continue;

            if (target == null)
            {
                king.MovePlateSpawn(tx, ty);
            }
            else if (targetCm.player == opponent)
            {
                king.MovePlateAttackSpawn(tx, ty);
            }
            // Friendly piece → blocked, no plate
        }

        game.SetPosition(king.gameObject);
    }
}