---
name: fm-tactical-analysis
description: Analyse the user's Football Manager 27 tactics against their squad and their results, and build or change a tactic in the game, using the football-manager MCP tools (get_tactics, get_role_fit, get_squad, get_matches, get_match, get_tactic_options, set_tactic). Use when the user asks to review, assess, improve or create a tactic, whether a tactic suits their players, which of their tactic slots to use, who should play where or in which role, why a tactic isn't working, or to set a tactic up for a match.
---

# FM tactical analysis

Turn the game's own data into a verdict on the user's tactics and a short list of concrete changes, and, when the user
wants, write a tactic into the game with `set_tactic`. Every tool reads the live game; only `set_tactic` with
`apply: true` changes it, and only once the user has agreed to the plan.

## 1. Gather

1. Call `get_tactics`, always first. It covers the three slots with the game's effectiveness assessment, and the
   active tactic in full: formations, mentality, style, team instructions, and each lineup slot's player, position,
   role and role rating in both phases.
2. Call `get_matches`, always. It gives the season's record and every match with the shape the team lined up in, so
   results can be tied to the tactic actually used. Match each shape to a slot by its formation (the slots use
   different shapes). Open a few matches with `get_match` when a pattern needs explaining, e.g. the draws, or the
   games a tactic was used in.
3. Call `get_role_fit` only for the positions the first pass flags (step 2). Use one call per position and phase,
   with the base code (`DC` for a DCL slot). Don't call it for every position by reflex.
4. Call `get_squad` with `includeStats: true` when finishing or creativity is in question (xG overperformance, xG per
   shot, key passes), and with `includeAttributes: true` when a point hinges on attributes, such as pace behind a high
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

Role ratings are a blunt measure. They don't see a player's standout quality, such as a passer who makes a chance
every game from a role he rates 14 in. Before advising that a player be dropped or moved, check his match numbers (key
passes, chances created, xA) and ask the user what they see in him.

**Results**
- xG measures the chances, goals the finishing. Goals well below xG over several matches point to finishing; low xG
  with plenty of possession points to chance creation (shots from poor positions, low xG per shot).
- Possession says who had the ball, not who was better. A low share away from home means the opponent was allowed
  the ball, which is a choice of the tactic as much as of the opponent.
- A few matches are a small sample. Say so when a pattern rests on three or four games.

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

**Keys, labels and position codes**
- Settings and roles come as keys, the game's English identifiers whatever language it runs in (`CentreForward`,
  `Positive`, `4231DMWide`), next to the game's labels in its display language. Keys are what `set_tactic` takes.
- Positions in the active lineup use English codes: DCL/DCR are the left/right of a central pair.
- Advice and inactive slots' position labels are in the game's language. In French, for example: GB = GK, DD/DG =
  DR/DL, DC(D/G) = DC(R/L), MDC(D/G) = DM(R/L), MD/MG = MR/ML, MC(D/G) = MC(R/L), MOD/MOG/MOC = AMR/AML/AMC,
  BTC(D/G) = ST(R/L). D = droite (right), G = gauche (left).
- Answer in the user's language, quoting the game's role names as the game writes them.
- Formation keys don't always match the shape the game shows (`343DMWide` is labelled 5-2-3): go by the label.

**Team instructions** are keyed by the game's identifiers (`Tempo`: `MuchHigherTempo`, `DefensiveLineBehaviour`:
`DropOff`, ...). Translate them into plain football terms.

## 3. Analyse

Work through these in order and keep notes. Only the conclusions go into the report.

1. **Which slot is strongest, and why.** Compare overall effectiveness and its parts across the slots. Say what drives
   the gaps (role suitability, capability, understanding) and the game's recommended styles.
2. **Squad fit in the active tactic.** For each lineup slot, in each phase:
   - Is the player's rating in the chosen role low compared with the squad? Flag 13 or below.
   - Does the same player rate 2+ higher in another role at that position (`roleOptions`)? That's an easy fix.
   - Does someone else in the squad rate 2+ higher there? Confirm with `get_role_fit`, and check that player isn't
     needed elsewhere.
   - Is a player out of their natural area, such as a DM playing on the wing?
3. **Coherence.**
   - Do the in- and out-of-possession shapes and roles balance? Look at who gives width, who creates, who runs in
     behind, and who covers when full-backs push on.
   - Do the team instructions fit the mentality, the style and the players? Examples: a high line or high line of
     engagement needs pace at the back; heavy pressing needs stamina and work rate; direct passing needs a target or
     runners.
   - Watch for instructions that pull against each other. A common one is a deep defensive line (or dropping off)
     with a high line of engagement and a hard press: the front presses high while the back sits deep, and the gap
     between the lines grows.
4. **Results by tactic.** Using `get_matches`, group the matches by the shape played. For each tactic, how did it do,
   home and away, and against which kind of opponent? Look at xG for and against and possession, as well as results.
5. **Understanding and complexity.**
   - Low-understanding areas are training priorities.
   - High complexity with low comprehension argues for simplifying instructions.
6. **Alternatives.** Use the other slots, the game's recommended styles, and how each tactic actually did, to judge
   whether the user should switch, adapt, or keep the current tactic for now.

Separate what the game says (scores, ratings, advice) from your own reasoning, and never invent attributes or ratings.

## 4. Report

Keep it tight. Use this structure:

1. **Verdict.** Two or three sentences: which tactic suits this squad best and the single biggest issue.
2. **Slots at a glance.** A small table: slot, overall, player ability, role suitability, tactical understanding, its
   record this season (matches, W-D-L, xG for and against), one line on its character.
3. **Main issues**, ranked, three to five. Each one cites the evidence: players, ratings, scores.
4. **Suggested changes**, each concrete and doable in the game: "Move X to Y", "Change Z's in-possession role from A to
   B (16 vs 14)", "Swap instruction ...", "Focus training on ...". Lead with the high-impact, low-risk ones.
5. **Watch out.** Uncertain data (ranges), hidden information you didn't use, and anything only the user can judge
   (e.g. a player's mood or a planned signing).

Offer to dig into a specific slot, position or player next, or to set the changes up in the game. Don't run more
tools unprompted once the report is delivered.

## 5. Set a tactic up in the game

When the user asks for a tactic to be built or changed in the game:

1. **Pick the slot.** Use slot 2 unless the user names another. If slot 2 holds no tactic, or one the user keeps for
   themselves, ask which slot to use. Writing a slot makes it the one the team plays, and it stays active.
2. **Gather what you need.** `get_tactics` for the current state, `get_tactic_options` for the keys (styles,
   mentalities, each phase's formations, team instructions, player instruction groups), `get_role_fit` for the roles a
   position offers and who suits them, and `get_squad` for player ids and attributes. Choose players on ability and
   role fit; condition recovers between matches.
3. **Dry run.** Call `set_tactic` without `apply`. It checks every key, and every role against the player's role table
   at the position, and returns the planned steps. A 400 lists what's wrong.
4. **Show the plan** in plain football terms (shape in each phase, XI with roles and ratings, mentality, instructions),
   with the effects worth knowing:
   - A new style resets the mentality and every team instruction to the style's own.
   - A formation change reshuffles players across slots and resets roles: give the full lineup in the same call.
   - Bringing players in changes the team's selection, which every slot shares.
5. **Apply** with `apply: true` once the user agrees. Each step comes back Done, Failed or Skipped; the first failure
   stops the rest. Fix the cause and send only what's left.
6. **Check** with `get_tactics` (the game's effectiveness rating of the new slot is a useful verdict), and ask the user
   to confirm on screen.

Lineup rules: to change who plays, list all eleven, each with a position in both phases; otherwise list only the
players to change. Player instructions replace a phase's whole list, except groups the role or position locks (a
defensive midfielder always holds position), which keep their option. Leave out instructions a role already gives.
