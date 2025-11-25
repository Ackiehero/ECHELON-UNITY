using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;
    GameObject reference = null;
    int matrixX;
    int matrixY;
    public bool attack = false;

    void Start()
    {
        if (attack)
            GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    }

    void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game game = controller.GetComponent<Game>();
        Chessman attacker = reference.GetComponent<Chessman>();
        GameObject defenderGO = game.GetPosition(matrixX, matrixY);

        // === ECHELON: TIER CLASH SYSTEM ===
        if (attack && defenderGO != null)
        {
            // Play Attack SFX
             ChessSFX.Attack();

            Chessman defender = defenderGO.GetComponent<Chessman>();
            bool sameClass = IsSameClass(attacker, defender);

            if (sameClass)
            {
                if (defender.tier > attacker.tier)
                {                   
                    // DEFENDER WINS → ATTACKER DIES → TURN ENDS IMMEDIATELY
                    LogMessage($"Tier {attacker.tier} {Capitalize(attacker.player)} {GetPieceName(attacker)} was defeated!");
                    Destroy(reference);
                    TierManager.Instance?.DeselectPiece();

                    // FIXED: CLEANUP AND END TURN
                    attacker.DestroyMovePlates();
                    if (!game.IsGameOver)
                        game.NextTurn();
                    
                    if (reference.name.Contains("_king"))
                        game.EndGame(defender.player);
                    
                    return;
                }
                else
                {
                    // ATTACKER WINS
                    LogMessage($"Tier {defender.tier} {Capitalize(defender.player)} {GetPieceName(defender)} was captured.");
                    Destroy(defenderGO);
                }
            }
            else
            {
                // DIFFERENT CLASS → NORMAL CAPTURE
                Destroy(defenderGO);
            }
        }

        else
        {
            ChessSFX.Move();  // ← Move sound plays ONLY for normal moves, perfect timing
        }

        // === NORMAL MOVE (only if attacker survived) ===
        game.SetPositionEmpty(attacker.GetXBoard(), attacker.GetYBoard());
        attacker.SetXBoard(matrixX);
        attacker.SetYBoard(matrixY);
        attacker.SetCoordinates();
        game.SetPosition(reference);

        attacker.DestroyMovePlates();

        // === KING CAPTURED? ===
        if (defenderGO != null && defenderGO.name.Contains("_king"))
        {
            game.EndGame(game.GetCurrentPlayer());
            return;
        }

        // === NEXT TURN (normal move) ===
        if (!game.IsGameOver)
            game.NextTurn();
    }

    private void LogMessage(string msg)
    {
        GameLog gl = Object.FindFirstObjectByType<GameLog>();
        gl?.LogMessage(msg);
    }

    private bool IsSameClass(Chessman a, Chessman b)
    {
        string typeA = a.name.Replace("w_", "").Replace("b_", "");
        string typeB = b.name.Replace("w_", "").Replace("b_", "");
        if (typeA == typeB) return true;
        if ((typeA == "knight" && typeB == "bishop") || (typeA == "bishop" && typeB == "knight"))
            return true;
        return false;
    }

    private string GetPieceName(Chessman c)
    {
        string n = c.name.Replace("w_", "").Replace("b_", "");
        return n switch
        {
            "king" => "King", "queen" => "Queen", "rook" => "Rook",
            "bishop" => "Bishop", "knight" => "Knight", "pawn" => "Pawn",
            _ => n
        };
    }

    private string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    public void SetCoordinates(int x, int y) { matrixX = x; matrixY = y; }
    public void SetReference(GameObject obj) { reference = obj; }
    public GameObject GetReference() { return reference; }
}