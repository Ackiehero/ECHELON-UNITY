using UnityEngine;
using System.Collections.Generic;

public class AI_TierBossSystem : MonoBehaviour
{
    private Game game;
    private TierManager tierManager;
    private GameLog gameLog;

    [Header("Boss Phase Settings - Adjustable Live")] 
    [Range(0f, 1f)] public float phase0KnightBishopChance = 0.4f;
    [Range(0f, 1f)] public float pawnUpgradeChance = 0.7f;
    [Range(0f, 1f)] public float massPawnMaxChance = 0.6f;
    [Range(0f, 1f)] public float knightBishopUpgradeChance = 1.0f;
    [Range(0f, 1f)] public float queenUpgradePriority = 0.9f;

    private Chessman lastMovedPawn = null;
    private int blackPawnsLost = 0;
    private int blackOfficersLost = 0;
    private int blackTier2OfficersLost = 0;

    private enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase = BossPhase.Phase1;

    void Awake()
    {
        game = FindFirstObjectByType<Game>();
        tierManager = FindFirstObjectByType<TierManager>();
        gameLog = FindFirstObjectByType<GameLog>();
    }

    void Update()
    {
        if (game == null) return;
        if (StockfishAI.Instance == null) return;  // make sure the singleton is ready

        if (game.IsGameOver) return;

        string currentPlayer = game.GetCurrentPlayer();
        if (string.IsNullOrEmpty(currentPlayer)) return;

        if (currentPlayer != "black" || StockfishAI.Instance.isThinking)
            return;
        
        RunBossPhase();
        enabled = false;
    }

    private void RunBossPhase()
    {
        UpdateLossCounters();

        Phase0_Warmup();

        if (blackOfficersLost >= 1 && blackTier2OfficersLost >= 1)
            currentPhase = BossPhase.Phase3;
        else if (blackOfficersLost >= 2)
            currentPhase = BossPhase.Phase2;
        else
            currentPhase = BossPhase.Phase1;

        Debug.Log($"[AI BOSS] → Phase {currentPhase} | Pawns Lost: {blackPawnsLost} | Officers Lost: {blackOfficersLost} | T2+ Officers Lost: {blackTier2OfficersLost}");

        switch (currentPhase)
        {
            case BossPhase.Phase1: Phase1(); break;
            case BossPhase.Phase2: Phase2(); break;
            case BossPhase.Phase3: Phase3(); break;
        }
    }

    private void Phase0_Warmup()
    {
        if (Random.value < phase0KnightBishopChance)
        {
            Chessman target = FindUpgradable("knight") ?? FindUpgradable("bishop");
            if (target != null)
                FakeUpgrade(target);
        }
    }

    private void Phase1()
    {
        if (lastMovedPawn != null && lastMovedPawn.tier < 2 && Random.value < pawnUpgradeChance)
            FakeUpgrade(lastMovedPawn);

        if (blackPawnsLost >= 1 && Random.value < massPawnMaxChance)
        {
            foreach (Chessman pawn in GetBlackPawns())
            {
                if (pawn.tier == 2)
                    FakeUpgrade(pawn);
            }
        }
    }

    private void Phase2()
    {
        if (Random.value < knightBishopUpgradeChance)
        {
            Chessman target = FindUpgradable("knight") ?? FindUpgradable("bishop");
            if (target != null)
                FakeUpgrade(target);
            else
                FakeUpgradeRandomPawn();
        }
    }

    private void Phase3()
    {
        Chessman queen = FindPiece("queen");
        if (queen != null && queen.tier < 3 && Random.value < queenUpgradePriority)
        {
            FakeUpgrade(queen);
            return;
        }

        Chessman officer = FindUpgradable("rook") ?? FindUpgradable("knight") ?? FindUpgradable("bishop");
        if (officer != null)
        {
            FakeUpgrade(officer);
            return;
        }

        FakeUpgradeRandomPawn();
    }

    private void FakeUpgrade(Chessman piece)
    {
        if (piece == null || piece.tier >= 3 || piece.name.Contains("king")) return;
        tierManager.UpgradePieceByAI(piece);
    }

    private void FakeUpgradeRandomPawn()
    {
        var pawns = GetUpgradable("pawn");
        if (pawns.Count > 0)
            FakeUpgrade(pawns[Random.Range(0, pawns.Count)]);
    }

    private void UpdateLossCounters()
    {
        blackPawnsLost = 8 - GetBlackPawns().Count;
        blackOfficersLost = 6 - (GetBlackPieces("knight").Count + GetBlackPieces("bishop").Count + GetBlackPieces("rook").Count);
        blackTier2OfficersLost = CountTier2PlusOfficers();
    }

    private int CountTier2PlusOfficers()
    {
        int expected = 6;
        int currentCount = GetBlackPieces("knight").Count + GetBlackPieces("bishop").Count + GetBlackPieces("rook").Count;

        int missingOfficers = expected - currentCount;
        int lostTier2Plus = 0;

        if (missingOfficers > 0)
        {
            int currentTier2Plus = 0;
            foreach (Chessman p in GetBlackPieces("knight")) if (p.tier >= 2) currentTier2Plus++;
            foreach (Chessman p in GetBlackPieces("bishop")) if (p.tier >= 2) currentTier2Plus++;
            foreach (Chessman p in GetBlackPieces("rook")) if (p.tier >= 2) currentTier2Plus++;

            if (currentTier2Plus > 0)
                lostTier2Plus = 1;
        }

        return lostTier2Plus;
    }

    private List<Chessman> GetBlackPawns() => GetBlackPieces("pawn");
    private List<Chessman> GetBlackPieces(string type) => GetPieces("b_", type);

    private List<Chessman> GetPieces(string prefix, string type)
    {
        List<Chessman> list = new List<Chessman>();
        for (int x = 0; x < 8; x++)
        for (int y = 0; y < 8; y++)
        {
            GameObject obj = game.GetPosition(x, y);
            if (obj != null && obj.name.Contains(prefix) && obj.name.Contains(type))
                list.Add(obj.GetComponent<Chessman>());
        }
        return list;
    }

    private List<Chessman> GetUpgradable(string type)
    {
        List<Chessman> list = new List<Chessman>();
        foreach (Chessman p in GetBlackPieces(type))
            if (p.tier < 3 && !p.name.Contains("king"))
                list.Add(p);
        return list;
    }

    private Chessman FindUpgradable(string type)
    {
        var list = GetUpgradable(type);
        return list.Count > 0 ? list[0] : null;
    }

    private Chessman FindPiece(string type)
    {
        var list = GetBlackPieces(type);
        return list.Count > 0 ? list[0] : null;
    }

    public void OnBlackPawnMoved(Chessman pawn)
    {
        lastMovedPawn = pawn;
    }

    public void ResetForNextTurn()
    {
        enabled = true;
    }
}
