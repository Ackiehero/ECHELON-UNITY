using UnityEngine;
using System.Reflection;

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
            return;
        }
    }

    void Update()
    {
        // Auto-disable if king or rook moved from start position
        CheckKingMoved("w_king", 4, 0, ref whiteKingside, ref whiteQueenside);
        CheckKingMoved("b_king", 4, 7, ref blackKingside, ref blackQueenside);
        CheckRookMoved(0, 0, ref whiteQueenside);
        CheckRookMoved(7, 0, ref whiteKingside);
        CheckRookMoved(0, 7, ref blackQueenside);
        CheckRookMoved(7, 7, ref blackKingside);
    }

    private void CheckKingMoved(string kingName, int startX, int startY, ref bool ks, ref bool qs)
    {
        GameObject king = null;
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                if (game.GetPosition(x, y)?.name == kingName)
                    king = game.GetPosition(x, y);

        if (king != null)
        {
            Chessman cm = king.GetComponent<Chessman>();
            if (cm.GetXBoard() != startX || cm.GetYBoard() != startY)
            {
                ks = qs = false;
            }
        }
    }

    private void CheckRookMoved(int startX, int startY, ref bool side)
    {
        if (game.GetPosition(startX, startY) == null)
            side = false;
    }

    // CALL THIS FROM Chessman.cs King cases in InitiateMovePlates()
    public void ShowCastlePlates(Chessman king)
    {
        bool isWhite = king.player == "white";
        int y = king.GetYBoard();

        // Kingside (g1/g8)
        if ((isWhite ? whiteKingside : blackKingside) &&
            game.GetPosition(5, y) == null && game.GetPosition(6, y) == null)
        {
            king.MovePlateAttackSpawn(king.GetXBoard() + 2, y); // g-file
        }

        // Queenside (c1/c8)  
        if ((isWhite ? whiteQueenside : blackQueenside) &&
            game.GetPosition(1, y) == null && game.GetPosition(2, y) == null && game.GetPosition(3, y) == null)
        {
            king.MovePlateAttackSpawn(king.GetXBoard() - 2, y); // c-file
        }
    }

    // CALL THIS FROM MovePlate.cs OnMouseUp() when king moves 2 squares
    public void DoCastling(Chessman king, int toX)
    {
        int y = king.GetYBoard();
        int rookFromX = toX > 4 ? 7 : 0; // h-file or a-file
        GameObject rook = game.GetPosition(rookFromX, y);
        
        if (rook != null)
        {
            Chessman rookCm = rook.GetComponent<Chessman>();
            int rookToX = toX > 4 ? toX - 1 : toX + 1;
            
            // Move rook
            game.SetPositionEmpty(rookFromX, y);
            rookCm.SetXBoard(rookToX);
            rookCm.SetYBoard(y);
            rookCm.SetCoordinates();
            game.SetPosition(rook);
            
            // Log
            FindFirstObjectByType<GameLog>()?.LogMessage(
                $"[{Capitalize(king.player)}] castled {(toX > 4 ? "kingside" : "queenside")}");
            
            ChessSFX.Move();
        }
        
        // Disable all castling for this player
        if (king.player == "white")
        {
            whiteKingside = whiteQueenside = false;
        }
        else
        {
            blackKingside = blackQueenside = false;
        }
    }

    private string Capitalize(string s)
    {
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}