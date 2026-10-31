---
name: fm-tactical-analysis
description: Analyse the user's Football Manager 27 tactics against their squad, using the football-manager MCP tools (fm_get_tactics, fm_get_role_fit, fm_get_squad). Use when the user asks to review, assess or improve their tactics, whether a tactic suits their players, which of their tactic slots to use, who should play where or in which role, or why a tactic isn't working.
---

# FM tactical analysis

Turn the game's own data into a verdict on the user's tactics and a short list of concrete changes. The tools read the
live game and change nothing; every change is the user's to make in the game.

## 1. Gather

1. Call `fm_get_tactics`, always first. It covers the three slots with the game's effectiveness assessment, and the
   active tactic in full: formations, mentality, style, team instructions, and each lineup slot's player, position,
   role and role rating in both phases.
2. Call `fm_get_role_fit` only for the positions the first pass flags (step 2). Use one call per position and phase,
   with the base code (`DC` for a DCL slot). Don't call it for every position by reflex.
3. Call `fm_get_squad` with `includeAttributes: true` when a point hinges on attributes, such as pace behind a high
   line, stamina for heavy pressing, or a role's key attributes. Never set `includeHidden` unless the user has
   explicitly agreed in this conversation.

## 2. Read the data

**Scales**
- Effectiveness scores run 0-100.
- Role ratings run 0-20. These are the game's stars, so 20 = five stars and each star is 4 points.
- A value like "15-17" is a range: the game's knowledge is incomplete.

**Rough reading of role ratings**

| Rating | Meaning |
|---|---|
| 17+ | Excellent fit |
| 15-16 | Good |
| 13-14 | Workable |
| 12 or less | A real weakness |

Compare against the squad, not in absolute terms: a 15 is the best available if nobody rates higher.

**Effectiveness parts**
- **Player ability** is role suitability (familiarity) plus capability (quality).
- **Player buy-in** is mostly about the manager, not the tactic. Report it, but don't fix the tactic for it.
- **Tactical understanding** breaks down by area, plus complexity and comprehension. Understanding grows through
  match exposure and training.

**Inactive slots**
- They are rated with the current eleven moved in by slot index, which is what switching would do. A player can look
  misplaced there, e.g. a winger landing in a DM slot.
- Judge a slot's potential by its shape and roles, and by who in the squad could fill them.
- Don't judge it by the effectiveness of today's eleven alone.

**Languages and position labels**
- Positions in the active lineup use English codes: DCL/DCR are the left/right of a central pair.
- Role names, advice and slot labels are in the game's language. In French: GB = GK, DD/DG = DR/DL, DC(D/G) = DC(R/L),
  MDC(D/G) = DM(R/L), MD/MG = MR/ML, MC(D/G) = MC(R/L), MOD/MOG/MOC = AMR/AML/AMC, BTC(D/G) = ST(R/L). D = droite
  (right), G = gauche (left).
- Answer in the user's language, quoting the game's role names as the game writes them.

**Team instructions** use the game's identifiers (`MuchHigherTempo`, `DeeperDefensiveLine`, `DropOff`, ...). Translate
them into plain football terms.

## 3. Analyse

Work through these in order and keep notes. Only the conclusions go into the report.

1. **Which slot is strongest, and why.** Compare overall effectiveness and its parts across the slots. Say what drives
   the gaps (role suitability, capability, understanding) and the game's recommended styles.
2. **Squad fit in the active tactic.** For each lineup slot, in each phase:
   - Is the player's rating in the chosen role low compared with the squad? Flag 13 or below.
   - Does the same player rate 2+ higher in another role at that position (`roleOptions`)? That's an easy fix.
   - Does someone else in the squad rate 2+ higher there? Confirm with `fm_get_role_fit`, and check that player isn't
     needed elsewhere.
   - Is a player out of their natural area, such as a DM playing on the wing?
3. **Coherence.**
   - Do the in- and out-of-possession shapes and roles balance? Look at who gives width, who creates, who runs in
     behind, and who covers when full-backs push on.
   - Do the team instructions fit the mentality, the style and the players? Examples: a high line or high line of
     engagement needs pace at the back; heavy pressing needs stamina and work rate; direct passing needs a target or
     runners.
4. **Understanding and complexity.**
   - Low-understanding areas are training priorities.
   - High complexity with low comprehension argues for simplifying instructions.
5. **Alternatives.** Use the other slots, and the game's recommended styles, to judge whether the user should switch,
   adapt, or keep the current tactic for now.

Separate what the game says (scores, ratings, advice) from your own reasoning, and never invent attributes or ratings.

## 4. Report

Keep it tight. Use this structure:

1. **Verdict.** Two or three sentences: which tactic suits this squad best and the single biggest issue.
2. **Slots at a glance.** A small table: slot, overall, player ability, role suitability, tactical understanding, one
   line on its character.
3. **Main issues**, ranked, three to five. Each one cites the evidence: players, ratings, scores.
4. **Suggested changes**, each concrete and doable in the game: "Move X to Y", "Change Z's in-possession role from A to
   B (16 vs 14)", "Swap instruction ...", "Focus training on ...". Lead with the high-impact, low-risk ones.
5. **Watch out.** Uncertain data (ranges), hidden information you didn't use, and anything only the user can judge
   (e.g. a player's mood or a planned signing).

Offer to dig into a specific slot, position or player next. Don't run more tools unprompted once the report is
delivered.
