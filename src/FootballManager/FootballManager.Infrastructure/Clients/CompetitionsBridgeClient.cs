using System.Globalization;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Competitions;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Competitions;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Dtos.Teams;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads competitions' stats screens through the MCPFM plugin. A competition is reached through a list holding it (the
// user's league from his club; the others through the user's continent's nations and their leagues), read with the
// stat's table and stage as contexts. A table's players are read in full from their rows, with the user's manager and
// team as contexts, so the game answers with what the club's scouting knows of them.
internal sealed class CompetitionsBridgeClient(GameBridge bridge, GameVocabulary vocabulary) : ICompetitionsGameClient
{
    // Where each known competition is, by id, once listed.
    private readonly Dictionary<int, string> _paths = [];
    // Shared by every request: two tool calls at once would otherwise read tables side by side.
    private static readonly SemaphoreSlim TableReads = new(1, 1);
    private CompetitionInfo? _userLeague;

    public async Task<CompetitionInfo> GetUserLeagueAsync(CancellationToken cancellationToken = default)
    {
        if (_userLeague is not null)
        {
            return _userLeague;
        }

        var league = await bridge.GetRecordAsync<BridgeCompetitionDto>(
            BridgeQuery.FromHuman().Base(CompetitionConstants.UserLeague).Props(CompetitionConstants.CompetitionProperties),
            cancellationToken);

        _userLeague = league.Values.Adapt<CompetitionInfo>();
        _userLeague.IsUserLeague = true;
        _paths[_userLeague.Id] = CompetitionConstants.UserLeague;
        return _userLeague;
    }

    public async Task<int?> GetUserTeamIdAsync(CancellationToken cancellationToken = default)
    {
        var team = await bridge.GetRecordAsync<BridgeTeamDto>(
            BridgeQuery.FromHuman().Base(TeamConstants.OwnTeam).Props([GameConstants.UniqueId, GameConstants.Name]),
            cancellationToken);

        return team.Values.Id;
    }

    // The user's league first, then each nation's leagues in the game's order; a league split in parts by its parts,
    // which are the ones with tables.
    public async Task<List<CompetitionInfo>> GetCompetitionsAsync(CancellationToken cancellationToken = default)
    {
        var userLeague = GetUserLeagueAsync(cancellationToken);
        var nations = bridge.GetListAsync<BridgeNationDto>(
            ForMen(BridgeQuery.FromHuman()).List(CompetitionConstants.Nations).Props([GameConstants.Name]),
            cancellationToken,
            gapsExpected: true);

        await Task.WhenAll(userLeague, nations);

        var nationNames = (await nations).Items.Select(nation => nation.Values.Name).ToList();
        var leaguePaths = nationNames.Select((_, index) => $"{TeamConstants.Item(CompetitionConstants.Nations, index)}.{CompetitionConstants.Leagues}").ToList();
        var leagues = await Task.WhenAll(leaguePaths.Select(path => CompetitionListAsync(path, cancellationToken)));

        var found = new List<(CompetitionInfo Competition, string Path)>();
        var parents = new List<(string Path, string? Nation)>();

        for (var nation = 0; nation < leagues.Length; nation++)
        {
            for (var index = 0; index < leagues[nation].Items.Count; index++)
            {
                var league = leagues[nation].Items[index].Values;
                var path = TeamConstants.Item(leaguePaths[nation], index);

                if (league.ChildCount > 0)
                {
                    parents.Add(($"{path}.{CompetitionConstants.Children}", nationNames[nation]));
                }
                else
                {
                    found.Add((Competition(league, nationNames[nation]), path));
                }
            }
        }

        var children = await Task.WhenAll(parents.Select(parent => CompetitionListAsync(parent.Path, cancellationToken)));

        for (var parent = 0; parent < parents.Count; parent++)
        {
            for (var index = 0; index < children[parent].Items.Count; index++)
            {
                found.Add((Competition(children[parent].Items[index].Values, parents[parent].Nation), TeamConstants.Item(parents[parent].Path, index)));
            }
        }

        var competitions = new List<CompetitionInfo> { await userLeague };

        foreach (var (competition, path) in found.Where(league => league.Competition.Id != competitions[0].Id))
        {
            competitions.Add(competition);
            _paths.TryAdd(competition.Id, path);
        }

        return competitions;
    }

    // One table at a time: the game answers a table's rows one frame after another, and several at once leave some
    // rows' players unanswered. A table read short is read once more.
    public async Task<CompetitionStatTable> GetStatTableAsync(
        int competitionId, CompetitionStat stat, CancellationToken cancellationToken = default)
    {
        var path = await PathAsync(competitionId, cancellationToken);
        await TableReads.WaitAsync(cancellationToken);

        try
        {
            var rows = await TableRowsAsync(path, stat, cancellationToken);

            if (rows.Any(row => row.PlayerId is null))
            {
                rows = await TableRowsAsync(path, stat, cancellationToken);
            }

            var header = await bridge.GetRecordAsync<BridgeStatHeaderDto>(
                OnTable(stat).Base($"{path}.{CompetitionConstants.Table}").Props([CompetitionConstants.Label]),
                cancellationToken);

            return new CompetitionStatTable
            {
                Stat = stat,
                Label = header.Values.Label,
                Rows = Rows(rows)
            };
        }
        finally
        {
            TableReads.Release();
        }
    }

    private async Task<List<BridgeStatRowDto>> TableRowsAsync(string path, CompetitionStat stat, CancellationToken cancellationToken)
    {
        var rows = await bridge.GetListAsync<BridgeStatRowDto>(
            OnTable(stat).List($"{path}.{CompetitionConstants.Rows}").Props(CompetitionConstants.RowProperties),
            cancellationToken,
            gapsExpected: true);

        return rows.Items.Select(row => row.Values).ToList();
    }

    public async Task<List<CompetitionPlayerProfile>> GetPlayersAsync(
        int competitionId, IReadOnlyList<CompetitionRowReference> rows, CancellationToken cancellationToken = default)
    {
        var path = await PathAsync(competitionId, cancellationToken);
        var keys = vocabulary.KeysAsync(cancellationToken);
        var statusKeys = vocabulary.StatusKeysAsync(cancellationToken);
        var players = rows.Select(row => bridge.GetRecordAsync<BridgeScoutedPlayerDto>(
            OnPlayer(path, row).Props(ScoutingConstants.PlayerBasics), cancellationToken)).ToList();
        var contracts = rows.Select(row => bridge.GetRecordAsync<BridgePlayerContractDto>(
            OnPlayer(path, row).Props(CompetitionConstants.ContractProperties), cancellationToken)).ToList();

        await Task.WhenAll(players.Cast<Task>().Concat(contracts).Append(keys).Append(statusKeys));

        var scouted = (await Task.WhenAll(players))
            .Select(player => player.Values)
            .ToList()
            .BuildAdapter()
            .AddParameters(MappingConfig.TacticKeysParameter, await keys)
            .AddParameters(MappingConfig.StatusKeysParameter, await statusKeys)
            .AdaptToType<List<ScoutedPlayer>>();

        var terms = await Task.WhenAll(contracts);

        return scouted.Select((player, index) => new CompetitionPlayerProfile
        {
            Player = player,
            Contract = terms[index].Values.Adapt<PlayerContract>()
        }).ToList();
    }

    // A row whose player the game didn't answer is left out; its rank still counts for the rows below it.
    private static List<CompetitionStatRow> Rows(List<BridgeStatRowDto> rows)
    {
        var ranks = Ranks(rows.Select(row => row.Rank).ToList());
        var ranked = new List<CompetitionStatRow>();

        for (var index = 0; index < rows.Count; index++)
        {
            if (rows[index].PlayerId is null)
            {
                continue;
            }

            var row = rows[index].Adapt<CompetitionStatRow>();
            row.Rank = ranks[index];
            row.RowIndex = index;
            ranked.Add(row);
        }

        return ranked;
    }

    // The game writes a rank once per group of tied players, in its language ("1st", "2nd"): "=" repeats the one above.
    internal static List<int> Ranks(IReadOnlyList<string?> written)
    {
        var ranks = new List<int>();
        var rank = 0;

        foreach (var text in written)
        {
            var digits = new string((text ?? string.Empty).TakeWhile(char.IsDigit).ToArray());
            rank = digits.Length > 0 ? int.Parse(digits, CultureInfo.InvariantCulture) : Math.Max(rank, 1);
            ranks.Add(rank);
        }

        return ranks;
    }

    private async Task<string> PathAsync(int competitionId, CancellationToken cancellationToken)
    {
        if (!_paths.ContainsKey(competitionId) && (await GetUserLeagueAsync(cancellationToken)).Id != competitionId)
        {
            await GetCompetitionsAsync(cancellationToken);
        }

        return _paths.TryGetValue(competitionId, out var path)
            ? path
            : throw new ArgumentException($"Competition {competitionId} isn't one whose stats the game keeps.", nameof(competitionId));
    }

    private Task<BridgeListDto<BridgeCompetitionDto>> CompetitionListAsync(string path, CancellationToken cancellationToken)
    {
        return bridge.GetListAsync<BridgeCompetitionDto>(
            ForMen(BridgeQuery.FromHuman()).List(path).Props(CompetitionConstants.CompetitionProperties),
            cancellationToken,
            gapsExpected: true);
    }

    private static CompetitionInfo Competition(BridgeCompetitionDto league, string? nation)
    {
        var competition = league.Adapt<CompetitionInfo>();
        competition.Nation ??= nation;
        return competition;
    }

    // A stat's table, for all stages. The gender goes along for the lists the path may go through.
    private static BridgeQuery OnTable(CompetitionStat stat)
    {
        return ForMen(BridgeQuery.FromHuman())
            .Context(CompetitionConstants.StatContext, CompetitionConstants.StatIds[stat])
            .Context(CompetitionConstants.StageContext, CompetitionConstants.AllStages);
    }

    private static BridgeQuery OnPlayer(string path, CompetitionRowReference row)
    {
        return OnTable(row.Stat).WithHumanAndTeam().Base($"{path}.{CompetitionConstants.Row(row.RowIndex)}");
    }

    private static BridgeQuery ForMen(BridgeQuery query)
    {
        return query.Context(CompetitionConstants.GenderContext, CompetitionConstants.Men);
    }
}
