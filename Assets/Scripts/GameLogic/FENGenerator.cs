// FENGenerator.cs
using UnityEngine;

public static class FENGenerator
{
    public static string GenerateFEN(Game game)
    {
        string fen = "";
        GameObject[,] positions = GetPositions(game);

        // Board
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
                    char fenChar = PieceToFEN(piece);
                    fen += fenChar;
                }
            }
            if (empty > 0) fen += empty;
            if (y > 0) fen += "/";
        }

        fen += " " + (game.GetCurrentPlayer() == "white" ? "w" : "b");
        fen += " KQkq - 0 1"; // Castling & en passant simplified

        return fen;
    }

    private static GameObject[,] GetPositions(Game game)
    {
        System.Reflection.FieldInfo field = game.GetType().GetField("positions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (GameObject[,])field.GetValue(game);
    }

    private static char PieceToFEN(string name)
    {
        return name switch
        {
            "w_king" => 'K',
            "w_queen" => 'Q',
            "w_rook" => 'R',
            "w_bishop" => 'B',
            "w_knight" => 'N',
            "w_pawn" => 'P',
            "b_king" => 'k',
            "b_queen" => 'q',
            "b_rook" => 'r',
            "b_bishop" => 'b',
            "b_knight" => 'n',
            "b_pawn" => 'p',
            _ => '?'
        };
    }
}