# MCPFM

MCPFM lets an AI assistant work inside your **Football Manager 27** save. It reads the game's own data live (squad,
tactics, matches, opponents, scouting, inbox, the whole player database) and can act in the game the way you would:
set up a tactic, send a scout, shortlist a player, run a player search. It works with any AI client that supports the
[Model Context Protocol](https://modelcontextprotocol.io) (Claude Code, Claude Desktop, Codex and others).

Things you can ask once it's set up:

- "How is my tactic working with this squad? Who's playing out of position?"
- "Scout Dundee United before Saturday and build a tactic for the match in slot 2."
- "Why did we lose to Hibs? Compare the numbers and tell me what my substitutions changed."
- "Find left-backs under 23 my scouts rate B or better, and shortlist the best two."
- "What's in my inbox? Anything urgent?"

The AI reads what the game shows you. Your scouts' knowledge (the fog of war) applies, and hidden attributes stay
hidden unless you ask for them.

> MCPFM is a fan project. It isn't affiliated with or endorsed by Sports Interactive or SEGA. It's a mod: it can break
> with a game update, so back up your saves.

## Contents

- [How it works](#how-it-works)
- [Tools](#tools)
- [Safety](#safety)
- [Requirements](#requirements)
- [Setup](#setup)
- [Updating](#updating)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)
- [Known limits](#known-limits)
- [Development](#development)
- [Roadmap](#roadmap)
- [Licence](#licence)

## How it works

```
AI client  --stdio-->  MCP server  --HTTP :5120-->  API  --HTTP :5121, localhost only-->  plugin inside FM27
(Claude, ...)          src/McpServer               src/FootballManager                    src/Plugin
```

1. **The plugin** (`src/Plugin`) is a [BepInEx](https://github.com/BepInEx/BepInEx) plugin that runs inside the game.
   It reads the game's data the way the game's own screens bind it, and sends the game's own events to change things.
   It only listens on `localhost`.
2. **The API** (`src/FootballManager`) turns that raw game data into football terms: players, ratings, tactics, match
   numbers, scouting reports. It checks every change against what the game allows before sending it.
3. **The MCP server** (`src/McpServer`) exposes the API as tools your AI client can call.

Everything runs on your PC. Nothing is sent anywhere, except what your AI client itself sends to its model.

## Tools

| Area | Tool | What it does |
|---|---|---|
| Squad | `get_squad` | Your first team: positions, condition, morale, season numbers; attributes on request |
| Tactics | `get_tactics` | Your three tactic slots with the game's effectiveness ratings, and the active one in full |
| | `get_role_fit` | Every role at a position, and how each of your players rates in it |
| | `get_tactic_options` | Every style, mentality, formation and instruction a tactic can take |
| | `set_tactic` | Builds or changes a tactic in a slot: shape, roles, lineup, instructions |
| Matches | `get_matches` | Your season so far, match by match, with the shape you played |
| | `get_match` | One match in depth: both teams' numbers, goals, substitutions, every player's position, role and stats |
| Opponents | `get_team` | A scouting report on any team: record, style home and away, usual shapes and starters, squad |
| Scouting | `get_scouting` | Your recruitment set-up: scouts, focuses, budget |
| | `get_scouted_players` | Players from a scouting list (recommendations, shortlist, a focus, the player search) |
| | `run_player_search` | Runs the game's player search with filters (age, positions, grade, quick filters, interest) |
| | `create_recruitment_focus` / `remove_recruitment_focus` | Adds or removes a recruitment focus |
| | `scout_player` / `shortlist_player` | Sends a scout to watch a player; adds a player to the shortlist or takes him off |
| | `scout_team` | Sends scouts to watch every player of a team, one assignment each |
| Trials | `offer_trials` | Offers trials of 1 to 4 weeks to up to 50 players at once (free agents found with the player search, say) |
| | `end_trials` | Ends trials, or withdraws offers not yet answered, for up to 50 players at once |
| Database | `search_database` | The game's search box: any player, staff member, club, competition or nation in the world |
| Inbox | `get_inbox` | Your messages, newest first, with forwarded press articles on request |

Names and labels come in the game's display language. Settings and roles also come as the game's English keys
(`CentreForward`, `MuchHigherTempo`), which is what `set_tactic` takes.

The repo also has a **skill** for Claude, `skills/fm-tactical-analysis`, which walks the model through a full tactical
review and a match plan with these tools (see [step 8](#8-optional-install-the-tactical-analysis-skill)).

## Safety

- **Reads don't change your game.** Two small exceptions: `search_database` types its text into the game's search box,
  and `get_team` briefly makes the team it reads the game's current team, then puts yours back. Don't run `get_team`
  at the same time as `set_tactic`.
- **Every change is a dry run first.** The tools that change something (`set_tactic`, `run_player_search`,
  `create_recruitment_focus`, `remove_recruitment_focus`, `scout_player`, `scout_team`, `shortlist_player`,
  `offer_trials`, `end_trials`) only check and describe the change unless they're called with `apply: true`. Their
  descriptions tell the AI to ask you before applying.
- **Scouting shows its cost first.** Scouting has no price up front: it costs travel and expenses as the scout works.
  The dry runs of `scout_player` and `scout_team` show the season's scouting budget (spent and left) and the game's own
  confirmation message, and the game's warnings block the change. `scout_team`, `offer_trials` and `end_trials` only
  apply with the number of players you agreed to: if the plan has changed since the dry run, nothing is sent.
- **No changes during a match.** The plugin refuses every change while a match is being played.
- **Your saves are left alone.** Nothing saves the game, continues to the next day, or opens or closes a save. (The
  plugin can open and quit one save of your choice, a copy for an AI to experiment on alone: see
  [Configuration](#configuration). It's not exposed as a tool.)
- **Hidden data only on request.** Hidden attributes and current/potential ability are only read when asked for
  (`includeHidden`), and the game masks them unless you have the In-Game Editor.

## Requirements

- **Windows 10 or 11**, with **Football Manager 27 on Steam**. Tested on game version 27.0.0.2493426.
- **[BepInEx 6](https://builds.bepinex.dev/projects/bepinex_be), IL2CPP build.** Tested with 6.0.0-be.791; the setup
  below links that build.
- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).** It builds the plugin and runs the API and the
  MCP server.
- **[Git](https://git-scm.com/downloads)**, to get the code.
- **An MCP client:** [Claude Code](https://docs.claude.com/en/docs/claude-code/overview), Claude Desktop, Codex or
  another.
- Optional: **[Docker Desktop](https://www.docker.com/products/docker-desktop/)**, to run the API in a container
  instead.

## Setup

About 20 minutes, most of it the game's first start with BepInEx. Each step ends with a check: if it fails, see
[Troubleshooting](#troubleshooting) before going on.

> **Setting up with an AI?** Point your assistant at this README. Every step is a command or a file change it can do
> for you, and every check is something it can run.

The commands are for PowerShell. The examples assume the default Steam folder,
`C:\Program Files (x86)\Steam\steamapps\common\Football Manager 27`. To find yours: in Steam, right-click
Football Manager 27 > **Manage** > **Browse local files**.

### 1. Install BepInEx into the game

1. Close Football Manager.
2. Download BepInEx 6.0.0-be.791 for IL2CPP, 64-bit Windows:
   [BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.791+b782da4.zip](https://builds.bepinex.dev/projects/bepinex_be/791/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.791%2Bb782da4.zip)
3. Extract the zip **into the game folder**, next to `fm.exe`. Don't extract it into a subfolder.

**Check:** the game folder now has a `BepInEx` folder, a `dotnet` folder, `doorstop_config.ini` and `winhttp.dll`
next to `fm.exe`.

### 2. Start the game once

1. Start Football Manager 27 from Steam as usual.
2. Wait. The first start with BepInEx takes several minutes: BepInEx downloads Unity's base libraries and generates
   the "interop" assemblies the plugin is built against. The game window can look stuck while it works.
3. When the game's start screen appears, quit the game.

**Check:** `BepInEx\interop` in the game folder holds about 180 `.dll` files, among them `FM.UI.dll` and `SI.UI.dll`.
`BepInEx\LogOutput.log` starts with a line like `BepInEx 6.0.0-be.791 - fm`.

### 3. Get the code

```powershell
git clone https://github.com/<owner>/MCPFM.git C:\Code\MCPFM
cd C:\Code\MCPFM
```

Any folder works; the rest of this guide uses `C:\Code\MCPFM`.

**Check:** `dotnet --list-sdks` lists a `10.0` SDK.

### 4. Build and install the plugin

With the game **closed**:

```powershell
dotnet build src\Plugin\FootballManager.Plugin
```

The build copies the plugin to `BepInEx\plugins\MCPFM\FootballManager.Plugin.dll` in the game folder.

- If the game is somewhere else, give its folder:
  `dotnet build src\Plugin\FootballManager.Plugin -p:FM27Dir="D:\SteamLibrary\steamapps\common\Football Manager 27"`
- If the build warns that it couldn't copy the file, the game is still running: close it and build again.
- If it says access is denied, open PowerShell as administrator and build again.

**Check:** `BepInEx\plugins\MCPFM\FootballManager.Plugin.dll` exists in the game folder.

### 5. Start the game and check the plugin

1. Start Football Manager 27 and load your career save.
2. Open <http://localhost:5121/status> in a browser, or run `curl.exe http://localhost:5121/status`.

**Check:** the page starts with `bridge: MCPFM Bridge 0.11.0`. `BepInEx\LogOutput.log` has a line
`Listening on http://localhost:5121/`.

### 6. Start the API

The API has to run whenever you use the tools. Pick one way.

**Option A, with .NET (simplest).** In a terminal you leave open:

```powershell
cd C:\Code\MCPFM
dotnet run --project src\FootballManager\FootballManager.Api
```

**Option B, with Docker.** It runs in the background and starts again with Docker:

```powershell
cd C:\Code\MCPFM
docker compose up -d --build footballmanager-api
```

`docker compose up -d seq` adds [Seq](https://datalust.co/seq), a log viewer at <http://localhost:5342>. It's
optional.

**Check:** `curl.exe http://localhost:5120/health` answers `Healthy`. With your save loaded,
`curl.exe http://localhost:5120/api/v1/squad` returns your squad as JSON.

### 7. Connect your AI client

Build the MCP server once first, so the client doesn't time out waiting for the first build:

```powershell
cd C:\Code\MCPFM
dotnet build src\McpServer
```

Then add it to your client. It starts the server itself.

**Claude Code:**

```powershell
claude mcp add --scope user football-manager -- dotnet run --project C:\Code\MCPFM\src\McpServer
```

**Claude Desktop:** add this to `%APPDATA%\Claude\claude_desktop_config.json`, then restart Claude Desktop:

```json
{
  "mcpServers": {
    "football-manager": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\Code\\MCPFM\\src\\McpServer"]
    }
  }
}
```

**Codex:** add this to `%USERPROFILE%\.codex\config.toml`:

```toml
[mcp_servers.football-manager]
command = "dotnet"
args = ["run", "--project", "C:\\Code\\MCPFM\\src\\McpServer"]
```

**Other clients:** register a stdio server with the command `dotnet` and the arguments
`run --project C:\Code\MCPFM\src\McpServer`.

Some changes take a while: writing a whole tactic can take a minute or two. If your client limits how long a tool
call may run, allow at least three minutes.

**Check:** your client lists the `football-manager` server with its tools (in Claude Code, run `/mcp`).

### 8. Optional: install the tactical analysis skill

For Claude Code, copy the skill into your personal skills folder:

```powershell
Copy-Item -Recurse C:\Code\MCPFM\skills\fm-tactical-analysis $HOME\.claude\skills\
```

**Check:** in a new Claude Code session, asking "review my tactics" makes Claude use the `fm-tactical-analysis` skill.

### 9. First conversation

With the game running, your save loaded and the API started, ask your assistant:

- "Show me my squad."
- "Which of my tactic slots suits this squad best?"
- "Scout my next opponent."

## Updating

**After pulling new code:**

1. Close the game and rebuild the plugin (step 4).
2. Restart the API: stop and rerun `dotnet run`, or rerun `docker compose up -d --build footballmanager-api`.
3. Reconnect the MCP server in your client (in Claude Code: `/mcp`, then reconnect `football-manager`).

**After a game update:** BepInEx regenerates its interop assemblies on the next start, which takes a few minutes again.
Then close the game and rebuild the plugin (step 4). A game update can change what the game exposes, so some tools may
stop working until MCPFM is updated.

## Configuration

**The plugin** writes its settings to `BepInEx\config\mcpfm.bridge.cfg` in the game folder on its first start:

| Setting | Default | What it is |
|---|---|---|
| `[Bridge] Port` | `5121` | The localhost port the API talks to the game on |
| `[Session] InvestigationSave` | `MCPFM investigation` | The only save the plugin's `/load` endpoint opens and `/quit` closes: a copy of a career in your games folder, named without `.fm`, for an AI to experiment on alone. Your other saves are never opened or closed |

**The API** reads `src\FootballManager\FootballManager.Api\appsettings.json`, or environment variables:

| Setting | Default | What it is |
|---|---|---|
| `InfrastructureSettings__GameBridgeBaseUrl` | `http://localhost:5121` | Where the plugin answers. Change the port here if you changed the plugin's |
| `InfrastructureSettings__TimeoutSeconds` | `60` | How long the API waits for one answer from the game |
| `Seq__ServerUrl` | `http://localhost:5341` | Where to send logs, if a Seq server runs. Without one, logs only go to the console. The compose file's Seq answers on `http://localhost:5342` (the API container already points to it) |

The API listens on port 5120. The MCP server expects it there.

**The game folder** for the plugin build is the `FM27Dir` build property (step 4).

## Troubleshooting

**A tool fails with "Football Manager isn't answering on ..."**
The API can't reach the plugin. Check that the game is running, that <http://localhost:5121/status> answers, and that
a save is loaded: the plugin only reads a loaded career.

**`/status` doesn't answer**
- Look in `BepInEx\LogOutput.log` in the game folder for `MCPFM Bridge` and any error near it.
- No BepInEx lines at all: BepInEx isn't installed in the right folder. `winhttp.dll` must be next to `fm.exe`.
- BepInEx runs but no `MCPFM Bridge`: the plugin isn't in `BepInEx\plugins\MCPFM\`. Rebuild it with the game closed.

**The plugin build can't find `BepInEx.Core.dll` or a `.dll` under `interop`**
BepInEx isn't installed, or the game hasn't been started once with it (steps 1 and 2), or the game is in another
folder: pass `-p:FM27Dir=...` (step 4).

**The build warns that it couldn't copy `FootballManager.Plugin.dll`**
The game is running and has the plugin loaded. Close the game and build again.

**A tool fails with a 503 and "no save loaded" or similar**
Load your career first. Tools that read a match or a team need the save fully loaded, past the loading screen.

**A change is refused: "A match is being played"**
Wait until the match is over. Changes are blocked during matches on purpose.

**The API in Docker can't reach the game**
The container reaches the game through `host.docker.internal`, and the plugin only answers requests addressed to
`localhost`. `docker-compose.yml` sets both; keep those two lines if you edit it.

**Your client doesn't list the tools, or times out starting the server**
Build the MCP server first (`dotnet build src\McpServer`), check the path in your client's configuration, then
reconnect or restart the client.

**Port already in use**
Change the plugin's port in `mcpfm.bridge.cfg` and `InfrastructureSettings__GameBridgeBaseUrl` to match.

## Known limits

- **Windows and Steam only**, tested on one game version (27.0.0.2493426) with the game in French. The tools use the
  game's ids rather than its text, so other languages should work, but they haven't been tested.
- **Game updates can break it** (see [Updating](#updating)).
- **Some data is the game's, as it keeps it.** Teams far from your competitions have few match numbers. A substitute's
  position is the one of the slot he took: a reshuffle after a change isn't recorded.
- **Not yet covered:** training, replying to messages, continuing the game and match days.
- **No automated tests yet.**

## Development

```
src/
  Plugin/FootballManager.Plugin/     BepInEx plugin: HTTP bridge into the game (.NET 6, the runtime BepInEx bundles)
  FootballManager/
    FootballManager.Domain/          The football model: players, tactics, matches, scouting
    FootballManager.Application/     Services: validation and football logic over the game's data
    FootballManager.Infrastructure/  Clients for the plugin: which game paths and events make up each feature
    FootballManager.Api/             ASP.NET Core API (.NET 10)
  McpServer/                         MCP tools over the API (.NET 10)
skills/                              Claude skills that use the tools
```

- **The plugin is a generic pipe.** It reads any property path the game's screens bind (`/get`), writes values
  (`/set`), sends the game's own events (`/native`), and records what the game's screens send while you click
  (`/events`), which is how features are found. Any unknown path, such as <http://localhost:5121/help>, lists every
  endpoint. Recordings redact secrets and account ids the game passes around.
- **Which paths make up a feature lives in the API**, in the Infrastructure clients and their constants. Mappings use
  Mapster.
- **Swagger:** the API in Development mode (as `dotnet run` starts it) serves it at <http://localhost:5120/swagger>.
- **Build everything:** `dotnet build MCPFM.slnx` (the plugin's copy to the game folder warns if the game is
  running).

## Roadmap

MCPFM started as a copilot: an assistant that reads your save and helps you decide. The goal is an AI that can
manage a club on its own while you watch. Next up: training, replying to messages, and the match-day flow
(continuing the game, team talks, in-match changes).

## Licence

[MIT](LICENSE)
