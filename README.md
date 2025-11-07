# Echelon: Tier Chess

A tactical evolution of classic chess, where players secretly upgrade pieces using limited tokens, adding layers of depth, deception, and uncertainty. You never know which piece might have the hidden edge.

**Date Created**: November 4, 2025  
**Author**: Cristo Rey Gulle Jr.  

> **Note**: All designs and gameplay mechanics described here are part of an evolving concept. The final implementation may vary based on playtesting results, technical feasibility, or creative direction.

## 🎯 Overview

This chess variant follows all standard chess rules but introduces a **hidden Tier system**. Players spend finite tokens to secretly upgrade their pieces during their turn, creating uncertainty—opponents see only that an upgrade happened, not *which* piece. Win conditions remain checkmate or surrender.

Key twists:
- **Deception**: Hide your powerhouses.
- **Prediction**: Track logs to anticipate threats.
- **Resource Management**: 32 tokens per player, spent forever—no refunds.

## 💠 Tier System

### Piece Upgrade Costs
| Piece Type | Cost per Tier | Max Tier | Total Tokens if Maxed |
|------------|---------------|----------|-----------------------|
| Pawn      | 2 tokens     | 3       | 4 total              |
| Rook      | 3 tokens     | 3       | 6 total              |
| Knight    | 3 tokens     | 3       | 6 total              |
| Bishop    | 3 tokens     | 3       | 6 total              |
| Queen     | 5 tokens     | 3       | 10 total             |
| King      | Cannot upgrade | 1     | —                    |

### ⚙️ Upgrade Rules
1. Spend tokens only during *your* turn—never on the opponent's.
2. Upgrade any piece by paying its cost. Multiple upgrades per turn allowed (e.g., Pawn Tier 1 → 3 costs 4 tokens).
3. Opponent sees a vague log: “White upgraded a piece.” or “Black upgraded a piece.”
4. No piece exceeds Tier 3. King cannot be upgraded.
5. Tokens spent are final—no refunds.
6. Pawn promotions retain the current tier.

## ⚔️ Clash Rules

1. **Standard Movement & Capture**: All pieces move and capture as in classic chess. Standard hierarchy applies: Pawns < Officials (Rook, Knight, Bishop) < Queen.
2. **Tier-Based Resolution (Equal-Class Clashes)**: When same-type or equal-class pieces clash:
   - Higher-tier piece wins.
   - Same tier? Attacker wins.
3. **Equal-Class Pairs**:
   - Pawn ↔ Pawn
   - Rook ↔ Rook
   - Knight ↔ Bishop (Officials)
   - Queen ↔ Queen
4. **Reveal Rule**: After an equal-class clash, *only* the captured piece’s tier is revealed in the log (e.g., “Tier 2 White Pawn was captured.”). Winner’s tier stays hidden.

## 🪙 Token Management & Strategic Depth

- Each player starts with **32 Tier Tokens**—finite "training resources" for the game.
- Every upgrade is a permanent decision, rewarding:
  - **Deception**: Conceal strongest units.
  - **Prediction**: Track upgrade logs and enemy shifts.
  - **Discipline**: Balance early aggression with late-game investments.
  - **Nerves of Steel**: Pawn trades are never truly safe.

## 🌀 Game Flow

Each turn: **Turn → Train (Optional) → Move → (If Clash: Apply Tier System) → End Turn**

- Upgrade units at the start of your turn, before moving.
- Clashes between same-class pieces use tiers; only the loser's tier is revealed.

### Examples

#### Example 1: Non-Tier Clash
- White: Pawn to e4.
- Black: Pawn to d5.
- White: Pawn takes d5 → Log: “Tier 1 Black Pawn was captured.” (Standard chess rules apply.)

#### Example 2: Tiered Clash
- White upgrades (Log: “White upgraded a piece.”), then Pawn to e4.
- Black upgrades (Log: “Black upgraded a piece.”), then Pawn to d5.
- White: Pawn takes d5 → Log: “Tier 1 White Pawn was captured.”  
  (Black's higher-tier pawn wins; tiers hidden until capture.)

#### Example 3: Clash Scenarios
Visual diagrams (conceptual):
- Same class, same tier → Attacker wins.
- Same class, different tier → Higher tier wins.
- Different class → Standard hierarchy (tiers irrelevant).

For detailed diagrams, see the [GDD PDF](Echelon%20Tier%20Chess%20-%20GDD.pdf).

## 🎨 Game Art-Style References

Inspired by stylized, ethereal designs (e.g., Hollow Knight-like silhouettes with tier-based evolutions). Head shapes vary by piece type:

- **Pawn**: Simple, hooded figures.
- **Rook**: Towered helmets.
- **Bishop**: Mitre hats.
- **Knight**: Horned helms.
- **Queen/King**: Crowned variants.

See Page 5 of the GDD for sketches. Art will evolve with implementation.

## 🚀 Getting Started

This repository hosts the design and development of *Echelon: Tier Chess*. Currently, it includes the core Game Design Document (GDD).

### Prerequisites
- None (design phase). For future implementation: Unity, Godot, or Python (with libraries like `chess` for prototyping).

### Installation
1. Clone the repo:
   ```
   git clone https://github.com/yourusername/echelon-tier-chess.git
   cd echelon-tier-chess
   ```
2. Review the [GDD PDF](Echelon%20Tier%20Chess%20-%20GDD.pdf) for full rules.

### Usage
- **Playtesting**: Prototype manually or use a chess engine mod (TBD).
- **Development**: See `/src` for code (coming soon). Focus: Hidden tiers, token UI, clash resolution.

## 🤝 Contributing
Contributions welcome! 
- Fork the repo.
- Create a feature branch (`git checkout -b feature/amazing-idea`).
- Commit changes (`git commit -m 'Add tier UI'`).
- Push and open a PR.

Ideas: Balance tokens, add AI, mobile app.

## 📄 License
This project is under the MIT License. See [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments
- Built on classic chess foundations.
- Art inspirations: Hollow Knight, indie tactical games.

For questions or playtesting, contact Cristo Rey Gulle Jr. at [your-email@example.com].

---

⭐ Star this repo if you're excited for tactical chess evolution!
