using FootballManager.Domain.Competitions;
using FootballManager.Domain.Players;
using FootballManager.Domain.Tactics;
using FootballManager.Infrastructure.Clients;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Competitions;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Tests;

public class CompetitionMappingTests
{
    private readonly TypeAdapterConfig _config = new();

    public CompetitionMappingTests()
    {
        new MappingConfig().Register(_config);
    }

    [Fact]
    public void A_stat_row_reads_with_every_position_the_player_can_play()
    {
        var row = new BridgeStatRowDto
        {
            PlayerId = 2000199529,
            Name = "Cameron Gauci",
            Age = 26,
            Positions = 4680,
            Minutes = 2491,
            TeamId = 74010202,
            Team = "Llantwit Major",
            Value = 12.926342964172363,
            Rank = "1ère"
        }.Adapt<CompetitionStatRow>(_config);

        Assert.Equal([PitchPosition.DL, PitchPosition.WBL, PitchPosition.ML, PitchPosition.AML], row.Positions);
        Assert.Equal(12.93, row.Value);
        Assert.Equal("Llantwit Major", row.Team);
    }

    [Fact]
    public void Tied_players_share_the_rank_above_them()
    {
        var ranks = CompetitionsBridgeClient.Ranks(["1ère", "2e", "=", "4e", "=", "=", "7e"]);

        Assert.Equal([1, 2, 2, 4, 4, 4, 7], ranks);
    }

    [Fact]
    public void A_first_row_without_a_number_still_ranks_first()
    {
        Assert.Equal([1, 1], CompetitionsBridgeClient.Ranks(["=", null]));
    }

    [Fact]
    public void An_amateur_contract_has_no_end_date_wage_or_asking_price()
    {
        var contract = new BridgePlayerContractDto
        {
            EndDate = "1900-01-02",
            HasEndDate = false,
            Type = new BridgeSortedValueDto { DisplayValue = "Mensuel", SortValue = 1 },
            Status = new BridgeSortedValueDto { DisplayValue = "Contrat amateur", SortValue = 4 },
            Wage = new BridgeOptionDto { DisplayValue = "N/A", PropertyValue = 0 },
            Value = new BridgeTransferValueDto { AskingPrice = -1, TransferValueRangeString = "0 €" },
            TransferStatus = new BridgeTransferStatusDto { TransferStatusDescription = new BridgeTransferStatusTextDto { Text = "Non déf." } }
        }.Adapt<PlayerContract>(_config);

        Assert.Equal("Contrat amateur", contract.Status);
        Assert.Equal("Mensuel", contract.Type);
        Assert.Null(contract.EndDate);
        Assert.Null(contract.Wage);
        Assert.Null(contract.AskingPrice);
        Assert.Equal("0 €", contract.Value);
        Assert.Equal("Non déf.", contract.TransferStatus);
    }

    [Fact]
    public void A_part_time_contract_keeps_its_end_date_wage_and_asking_price()
    {
        var contract = new BridgePlayerContractDto
        {
            EndDate = "2027-05-31",
            HasEndDate = true,
            Status = new BridgeSortedValueDto { DisplayValue = "Contrat à mi-temps", SortValue = 6 },
            Wage = new BridgeOptionDto { DisplayValue = "180 € - 290 € p/m", PropertyValue = 180 },
            Value = new BridgeTransferValueDto { AskingPrice = 5000, TransferValueRangeString = "4K € - 6K €" }
        }.Adapt<PlayerContract>(_config);

        Assert.Equal("2027-05-31", contract.EndDate);
        Assert.Equal("180 € - 290 € p/m", contract.Wage);
        Assert.Equal(5000, contract.AskingPrice);
        Assert.Null(contract.TransferStatus);
    }

    [Fact]
    public void A_league_reads_with_its_nation_and_reputation()
    {
        var league = new BridgeCompetitionDto { Id = 29236869, Name = "Novira Cymru North", Reputation = 32, Nation = "Pays de Galles" }
            .Adapt<CompetitionInfo>(_config);

        Assert.Equal(29236869, league.Id);
        Assert.Equal("Pays de Galles", league.Nation);
        Assert.Equal(32, league.Reputation);
        Assert.False(league.IsUserLeague);
    }
}
