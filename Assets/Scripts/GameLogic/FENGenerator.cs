using UnityEngine;
using System.Reflection;

public static class FENGenerator
{
    public static string GenerateFEN(Game game)
    {
        string fen = "";
        GameObject[,] positions = GetPositions(game);

        // Board layout
        for (int y = 7; y >= 0; y--)
        {
            int empty = 0;
            for (int x = 0; x < 8; x++)
            {
                GameObject p = positions[x, y];
                if (p == null)
                {
                    empty++;
                }
                else
                {
                    if (empty > 0) { fen += empty; empty = 0; }
                    string piece = p.name;
                    fen += PieceToFEN(piece);
                }
            }
            if (empty > 0) fen += empty;
            if (y > 0) fen += "/";
        }

        // Current player
        fen += " " + (game.GetCurrentPlayer() == "white" ? "w" : "b");

        // Castling rights
        string castling = GetCastlingRights();
        fen += " " + castling;

        // En passant placeholder (still simplified)
        fen += " -";

        // Halfmove clock and fullmove number (simplified, can expand later)
        fen += " 0 1";

        return fen;
    }

    private static GameObject[,] GetPositions(Game game)
    {
        FieldInfo field = game.GetType().GetField("positions",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (GameObject[,])field.GetValue(game);
    }

    private static string PieceToFEN(string name)
    {
        return name switch
        {
            "w_king" => "K",
            "w_queen" => "Q",
            "w_rook" => "R",
            "w_bishop" => "B",
            "w_knight" => "N",
            "w_pawn" => "P",
            "b_king" => "k",
            "b_queen" => "q",
            "b_rook" => "r",
            "b_bishop" => "b",
            "b_knight" => "n",
            "b_pawn" => "p",
            _ => "?"
        };
    }

    private static string GetCastlingRights()
    {
        CastlingManager cm = Object.FindFirstObjectByType<CastlingManager>();
        if (cm == null) return "-";

        string rights = "";
        if (cm.whiteKingside) rights += "K";
        if (cm.whiteQueenside) rights += "Q";
        if (cm.blackKingside) rights += "k";
        if (cm.blackQueenside) rights += "q";

        return rights.Length > 0 ? rights : "-";
    }
}
