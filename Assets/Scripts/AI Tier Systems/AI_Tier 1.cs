using UnityEngine;
using System.Collections.Generic;

public class AI_TierBossSystem : MonoBehaviour
{
    private Game game;
    private TierManager tierManager;
    private GameLog gameLog;

    [Header("Boss Phase Settings - Adjustable Live")]
    [Range(0f, 1f)] public float phase0KnightBishopChance = 0.4f;   // 40% chance to upgrade a Knight/Bishop each turn
    [Range(0f, 1f)] public float pawnUpgradeChance = 0.7f;
    [Range(0f, 1f)] public float massPawnMaxChance = 0.6f;
    [Range(0f, 1f)] public float knightBishopUpgradeChance = 1.0f;
    [Range(0f, 1f)] public float queenUpgradePriority = 0.9f;

    private Chessman lastMovedPawn = null; // Remembers which pawn moved last
    private int blackPawnsLost = 0; // Tracks how many pawns black has lost
    private int blackOfficersLost = 0; // Tracks how many officers black has lost
    private int blackTier2OfficersLost = 0; // Tracks how many tier 2+ officers black has lost

    private enum BossPhase { Phase1, Phase2, Phase3 } // The phases of the boss system
    private BossPhase currentPhase = BossPhase.Phase1; // The current phase of the boss system

    void Awake()
    {
        game = FindFirstObjectByType<Game>();
        tierManager = FindFirstObjectByType<TierManager>();
        gameLog = FindFirstObjectByType<GameLog>();
    }

    void Update() // MAIN LOOP — Runs once per AI turn
    {
        if (game == null || game.IsGameOver || game.GetCurrentPlayer() != "black" || StockfishAI.Instance.isThinking)
            return;

        RunBossPhase();
        enabled = false; 
    }

    private void RunBossPhase()
    {
        UpdateLossCounters();

        Phase0_Warmup();

        if (blackOfficersLost >= 1 && blackTier2OfficersLost >= 1) // Only enter Phase 3 if we actually LOST a Tier 2+ officer
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
            {
                FakeUpgrade(target);
                Debug.Log("[AI BOSS] Phase 0 Warm-up: Knight/Bishop upgraded");
            }
        }
    }

    private void Phase1()
    {
        // Upgrade the pawn that moved last turn
        if (lastMovedPawn != null && lastMovedPawn.tier < 2 && Random.value < pawnUpgradeChance)
            FakeUpgrade(lastMovedPawn);

        // Rage: if lost any pawns → chance to max all existing Tier 2 pawns to Tier 3
        if (blackPawnsLost >= 1 && Random.value < massPawnMaxChance)
        {
            bool didUpgrade = false;
            foreach (Chessman pawn in GetBlackPawns())
            {
                if (pawn.tier == 2)
                {
                    FakeUpgrade(pawn);  // 2 → 3 only
                    didUpgrade = true;
                }
            }

            if (didUpgrade)
                Debug.Log("[AI BOSS] Lost pawns → All Tier 2 pawns promoted to Tier 3!");
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
        // Queen has highest priority
        Chessman queen = FindPiece("queen");
        if (queen != null && queen.tier < 3 && Random.value < queenUpgradePriority)
        {
            FakeUpgrade(queen);
            return;
        }

        // Then officers
        Chessman officer = FindUpgradable("rook") ?? FindUpgradable("knight") ?? FindUpgradable("bishop");
        if (officer != null)
        {
            FakeUpgrade(officer);
            return;
        }

        // Last: pawn
        FakeUpgradeRandomPawn();
    }

    // SAFE UPGRADE — uses your existing button system
    private void FakeUpgrade(Chessman piece)
    {
        if (piece == null || piece.tier >= 3 || piece.name.Contains("king")) return;

        tierManager.UpgradePieceByAI(piece);
    }

    private void FakeUpgradeRandomPawn()
    {
        var pawns = GetUpgradable("pawn");
        if (pawns.Count > 0) FakeUpgrade(pawns[Random.Range(0, pawns.Count)]);
    }

    private void LogBlackUpgrade()
    {
        gameLog?.LogMessage("Black upgraded a piece.");
    }

    private void UpdateLossCounters()
    {
        blackPawnsLost = 8 - GetBlackPawns().Count;
        blackOfficersLost = 6 - (GetBlackPieces("knight").Count + GetBlackPieces("bishop").Count + GetBlackPieces("rook").Count);
        blackTier2OfficersLost = CountTier2PlusOfficers();
    }

    private int CountTier2PlusOfficers()
    {
        // We only count Tier 2+ officers that are DEFEATED/CAPTURED
        int expected = 6;
        int currentCount = 
        GetBlackPieces("knight").Count +
        GetBlackPieces("bishop").Count +
        GetBlackPieces("rook").Count;

        int missingOfficers = expected - currentCount;

        // Now check how many of the missing ones were Tier 2+
        int lostTier2Plus = 0;

        if (missingOfficers > 0)
        {
            // Count current Tier 2+ officers
            int currentTier2Plus = 0;
            foreach (Chessman p in GetBlackPieces("knight")) if (p.tier >= 2) currentTier2Plus++;
            foreach (Chessman p in GetBlackPieces("bishop")) if (p.tier >= 2) currentTier2Plus++;
            foreach (Chessman p in GetBlackPieces("rook"))   if (p.tier >= 2) currentTier2Plus++;

            // If we started with 6 officers — if we have Tier 2+ now, but lost officers,
            // then at least one lost was Tier 2+
            if (currentTier2Plus > 0 && missingOfficers >= 1)
                lostTier2Plus = 1; // conservative: only count 1
        }

        return lostTier2Plus;
    }

    // Helper
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

    private Chessman FindUpgradable(string type) => GetUpgradable(type).Count > 0 ? GetUpgradable(type)[0] : null;
    private Chessman FindPiece(string type) => GetBlackPieces(type).Count > 0 ? GetBlackPieces(type)[0] : null;

    public void OnBlackPawnMoved(Chessman pawn) => lastMovedPawn = pawn;
    public void ResetForNextTurn() => enabled = true;
}