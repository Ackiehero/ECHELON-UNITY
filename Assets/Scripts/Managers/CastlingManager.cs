using UnityEngine;

public class CastlingManager : MonoBehaviour
{
    private Game game;

    // Castling rights (true = available)
    public bool whiteKingside = true, whiteQueenside = true;
    public bool blackKingside = true, blackQueenside = true;

    void Awake()
    {
        game = FindFirstObjectByType<Game>();
        if (game == null)
        {
            Debug.LogError("CastlingManager: Game not found!");
            enabled = false;
        }
    }

    void Update()
    {
        SafeCheckKingMoved("w_king", 4, 0, ref whiteKingside, ref whiteQueenside);
        SafeCheckKingMoved("b_king", 4, 7, ref blackKingside, ref blackQueenside);

        SafeCheckRookMoved(0, 0, ref whiteQueenside);
        SafeCheckRookMoved(7, 0, ref whiteKingside);
        SafeCheckRookMoved(0, 7, ref blackQueenside);
        SafeCheckRookMoved(7, 7, ref blackKingside);
    }

    private void SafeCheckKingMoved(string kingName, int startX, int startY, ref bool ks, ref bool qs)
    {
        GameObject king = null;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && !piece.Equals(null) && piece.name == kingName)
                {
                    king = piece;
                    break;
                }
            }
            if (king != null) break;
        }

        if (king != null && !king.Equals(null))
        {
            Chessman cm = king.GetComponent<Chessman>();
            if (cm != null && (cm.GetXBoard() != startX || cm.GetYBoard() != startY))
            {
                ks = qs = false;
            }
        }
        else
        {
            // King destroyed → disable castling
            ks = qs = false;
        }
    }

    private void SafeCheckRookMoved(int startX, int startY, ref bool side)
    {
        GameObject rook = game.GetPosition(startX, startY);
        if (rook == null || rook.Equals(null))
        {
            side = false;
        }
    }

    public void ShowCastlePlates(Chessman king)
    {
        if (king == null) return;

        bool isWhite = king.player == "white";
        int y = king.GetYBoard();
        int currentX = king.GetXBoard();  // Usually 4

        KingSafetyManager safety = FindFirstObjectByType<KingSafetyManager>();
        if (safety == null) return;  // Safety fallback

        string opponent = isWhite ? "black" : "white";

        // Also check that the king is NOT currently in check
        bool kingInCheck = safety.IsSquareAttacked(currentX, y, opponent);

        // Kingside
        if ((isWhite ? whiteKingside : blackKingside) &&
            !kingInCheck &&
            IsEmptySafe(5, y) && IsEmptySafe(6, y) &&
            !safety.IsSquareAttacked(5, y, opponent) &&  // Square king passes through
            !safety.IsSquareAttacked(6, y, opponent))    // Square king lands on
        {
            king.MovePlateAttackSpawn(king.GetXBoard() + 2, y);
        }

        // Queenside
        if ((isWhite ? whiteQueenside : blackQueenside) &&
            !kingInCheck &&
            IsEmptySafe(1, y) && IsEmptySafe(2, y) && IsEmptySafe(3, y) &&
            !safety.IsSquareAttacked(3, y, opponent) &&  // Square king passes through
            !safety.IsSquareAttacked(2, y, opponent))    // Square king lands on
        {
            king.MovePlateAttackSpawn(king.GetXBoard() - 2, y);
        }
    }

    private bool IsEmptySafe(int x, int y)
    {
        GameObject piece = game.GetPosition(x, y);
        return piece == null || piece.Equals(null);
    }

    public void DoCastling(Chessman king, int toX)
    {
        if (king == null || king.Equals(null)) return; // King destroyed?

        int y = king.GetYBoard();
        int rookFromX = toX > 4 ? 7 : 0; // h-file or a-file

        // Get rook safely
        GameObject rookGO = game.GetPosition(rookFromX, y);
        if (rookGO != null && !rookGO.Equals(null))
        {
            Chessman rookCm = rookGO.GetComponent<Chessman>();
            if (rookCm != null)
            {
                int rookToX = toX > 4 ? toX - 1 : toX + 1;

                // Move rook
                game.SetPositionEmpty(rookFromX, y);
                rookCm.SetXBoard(rookToX);
                rookCm.SetYBoard(y);
                rookCm.SetCoordinates();
                game.SetPosition(rookCm.gameObject);
            }
        }

        // Move king
        game.SetPositionEmpty(king.GetXBoard(), king.GetYBoard());
        king.SetXBoard(toX);
        king.SetCoordinates();
        game.SetPosition(king.gameObject);

        // Log castling
        FindFirstObjectByType<GameLog>()?.LogMessage(
            $"[{Capitalize(king.player)}] castled {(toX > 4 ? "kingside" : "queenside")}"
        );

        ChessSFX.Move();

        // Disable all castling for this player
        if (king.player == "white")
            whiteKingside = whiteQueenside = false;
        else
            blackKingside = blackQueenside = false;
    }

    private string Capitalize(string s)
    {
        return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
    }
}
