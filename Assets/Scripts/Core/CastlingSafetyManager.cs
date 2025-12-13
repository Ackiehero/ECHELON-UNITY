using UnityEngine;

public class CastlingSafetyManager : MonoBehaviour
{
    private Game game;
    private CastlingManager castlingManager;

    void Awake()
    {
        game = FindFirstObjectByType<Game>();
        castlingManager = FindFirstObjectByType<CastlingManager>();
        if (!game || !castlingManager) enabled = false;
    }

    // ONLY THIS METHOD IS CALLED FROM Chessman.cs
    public void TryShowSafeCastlePlates(Chessman king)
    {
        if (IsKingInCheck(king)) return; // Cannot castle out of check

        int x = king.GetXBoard();
        int y = king.GetYBoard();
        bool isWhite = king.player == "white";

        // === KINGSIDE ===
        if ((isWhite ? castlingManager.whiteKingside : castlingManager.blackKingside))
        {
            if (game.GetPosition(5, y) == null && game.GetPosition(6, y) == null) // f & g empty
            {
                // King passes through f1/f8 — MUST NOT be attacked
                if (!IsSquareAttacked(5, y, king.player))
                {
                    // g1/g8 is safe because king ends there and we already checked f
                    king.MovePlateAttackSpawn(x + 2, y);
                }
            }
        }

        // === QUEENSIDE ===
        if ((isWhite ? castlingManager.whiteQueenside : castlingManager.blackQueenside))
        {
            if (game.GetPosition(1, y) == null && game.GetPosition(2, y) == null && game.GetPosition(3, y) == null)
            {
                // King passes through d1/d8 — MUST NOT be attacked
                if (!IsSquareAttacked(3, y, king.player))
                {
                    king.MovePlateAttackSpawn(x - 2, y);
                }
            }
        }
    }

    public bool IsKingInCheck(Chessman king) => IsSquareAttacked(king.GetXBoard(), king.GetYBoard(), king.player);

    public bool IsSquareAttacked(int tx, int ty, string kingPlayer)
    {
        for (int x = 0; x < 8; x++)
        for (int y = 0; y < 8; y++)
        {
            GameObject p = game.GetPosition(x, y);
            if (p == null) continue;
            Chessman piece = p.GetComponent<Chessman>();
            if (piece.player == kingPlayer) continue;

            if (CanAttack(piece, x, y, tx, ty))
                return true;
        }
        return false;
    }

    private bool CanAttack(Chessman piece, int fx, int fy, int tx, int ty)
    {
        string type = piece.name.Replace("w_", "").Replace("b_", "");

        return type switch
        {
            "pawn" => Mathf.Abs(tx - fx) == 1 && (ty - fy) == (piece.player == "white" ? 1 : -1),
            "knight" => (Mathf.Abs(tx - fx), Mathf.Abs(ty - fy)) is (1, 2) or (2, 1),
            "king" => Mathf.Max(Mathf.Abs(tx - fx), Mathf.Abs(ty - fy)) <= 1,
            "bishop" => Mathf.Abs(tx - fx) == Mathf.Abs(ty - fy) && ClearPath(fx, fy, tx, ty),
            "rook" => (tx == fx || ty == fy) && ClearPath(fx, fy, tx, ty),
            "queen" => (tx == fx || ty == fy || Mathf.Abs(tx - fx) == Mathf.Abs(ty - fy)) && ClearPath(fx, fy, tx, ty),
            _ => false
        };
    }

    private bool ClearPath(int x1, int y1, int x2, int y2)
    {
        int dx = x2 > x1 ? 1 : (x2 < x1 ? -1 : 0);
        int dy = y2 > y1 ? 1 : (y2 < y1 ? -1 : 0);
        int steps = Mathf.Max(Mathf.Abs(x2 - x1), Mathf.Abs(y2 - y1));

        for (int i = 1; i < steps; i++)
            if (game.GetPosition(x1 + dx * i, y1 + dy * i) != null)
                return false;
        return true;
    }
}