using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Game))]
public class AIGameController : MonoBehaviour
{
    private Game game;
    private float lastMoveTime = 0f;
    public bool aiShouldMove = false; // ← NOW PUBLIC!

    [Header("AI Behavior")]
    [Tooltip("Max seconds AI can think per move. If exceeded → Black surrenders")]
    public float aiTimeout = 30f;

    private void Awake()
    {
        game = GetComponent<Game>();
    }

    private void Update()
    {
        if (game.IsGameOver) return;

        string current = game.GetCurrentPlayer();

        if (current == "black")
        {
            if (!aiShouldMove)
            {
                aiShouldMove = true;
                lastMoveTime = Time.time;
                StartCoroutine(AITurn());
            }

            if (aiShouldMove && Time.time - lastMoveTime > aiTimeout)
            {
                StartCoroutine(BlackSurrender());
            }
        }
        else
        {
            aiShouldMove = false;
        }
    }

    private IEnumerator AITurn()
    {
        yield return new WaitForSeconds(0.8f);
        DestroyAllMovePlates();

        string fen = FENGenerator.GenerateFEN(game);
        StockfishAI.Instance.MakeMove(fen, OnAIMoveReceived);
    }

    private void OnAIMoveReceived(string uci)
    {
        if (string.IsNullOrEmpty(uci) || uci.Length < 4)
        {
            aiShouldMove = false;
            return;
        }

        int fromX = uci[0] - 'a';
        int fromY = uci[1] - '1';
        int toX   = uci[2] - 'a';
        int toY   = uci[3] - '1';

        GameObject pieceObj = game.GetPosition(fromX, fromY);
        if (pieceObj == null)
        {
            aiShouldMove = false;
            return;
        }

        Chessman cm = pieceObj.GetComponent<Chessman>();
        if (cm == null || cm.player != "black")
        {
            aiShouldMove = false;
            return;
        }

        cm.InitiateMove();

        foreach (MovePlate plate in FindObjectsByType<MovePlate>(FindObjectsSortMode.None))
        {
            if (plate.GetX() == toX && plate.GetY() == toY)
            {
                plate.PerformMove();
                lastMoveTime = Time.time;
                break;
            }
        }

        FindFirstObjectByType<AI_TierBossSystem>()?.ResetForNextTurn(); // PHASE 1: Reset boss system

        aiShouldMove = false;
    }

    private IEnumerator BlackSurrender()
    {
        aiShouldMove = false;
        GameLog log = FindFirstObjectByType<GameLog>();
        if (log != null) log.LogMessage("Black decided to surrender.");
        game.EndGame("white");
        yield return null;
    }

    private void DestroyAllMovePlates()
    {
        foreach (MovePlate p in FindObjectsByType<MovePlate>(FindObjectsSortMode.None))
            Destroy(p.gameObject);
    }
}