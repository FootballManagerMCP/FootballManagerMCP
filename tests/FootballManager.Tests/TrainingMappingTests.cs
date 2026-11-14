using FootballManager.Domain.Training;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Training;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Tests;

public class TrainingMappingTests
{
    private readonly TypeAdapterConfig _config = new();

    public TrainingMappingTests()
    {
        new MappingConfig().Register(_config);
    }

    [Fact]
    public void A_player_reads_as_his_training_screen_shows_him()
    {
        var player = new BridgePlayerTrainingDto
        {
            Id = 250606,
            Name = "Osian Dillon",
            Position = 256,
            PositionChoice = 0,
            RoleId = 128,
            RoleName = "Ailier",
            OutOfPossessionRole = new BridgeOptionDto { PropertyValue = 50, DisplayValue = "---" },
            Focus = new BridgeOptionDto { PropertyValue = -1, DisplayValue = "---" },
            Intensity = new BridgeOptionDto { PropertyValue = 1, DisplayValue = "Intensité normale" },
            IntensityAutomatic = true,
            FocusCanEdit = true,
            FocusHumanControlled = true,
            IntensityCanEdit = true,
            Rating = 6.2000000476837158,
            FocusChoices = [Row(-1, "---"), Row(9, "Vivacité")],
            // The rest periods are a submenu under "no work", which isn't a choice of its own; holidays are left out.
            IntensityChoices =
            [
                Row(-1, "Automatique"),
                Row(1, "Intensité normale"),
                new BridgeOptionDto { RowData = new BridgeOptionDto { PropertyValue = 9, DisplayValue = "Aucun travail" }, ChildList = [Row(10, "1 jour"), Row(11, "2 jours")] },
                new BridgeOptionDto { RowData = new BridgeOptionDto { PropertyValue = 19, DisplayValue = "Envoyer en vacances" }, ChildList = [Row(20, "1 semaine")] }
            ]
        };

        var training = player.BuildAdapter(_config)
            .AddParameters(MappingConfig.RoleKeysParameter, new Dictionary<ulong, string> { [128] = "Winger" })
            .AdaptToType<PlayerTraining>();

        Assert.Equal("MR", training.Position);
        Assert.Equal("Winger", training.RoleKey);
        Assert.True(training.FollowsMatchRole);
        Assert.Null(training.OutOfPossessionRole);
        Assert.Null(training.Focus);
        Assert.Equal(1, training.Intensity!.Id);
        Assert.True(training.IntensityIsAutomatic);
        Assert.True(training.CanEditFocus);
        Assert.True(training.CanEditIntensity);
        Assert.Equal(6.2, training.Rating);
        Assert.Equal([-1L, 9L], training.FocusChoices);
        Assert.Equal([-1L, 1L, 10L, 11L], training.IntensityChoices);
    }

    [Fact]
    public void A_submenu_row_carries_its_submenu_label()
    {
        var rows = TrainingRows.Of(
            [new BridgeOptionDto { RowData = new BridgeOptionDto { PropertyValue = 9, DisplayValue = "Aucun travail" }, ChildList = [Row(12, "3 jours")] }]);

        Assert.Equal("Aucun travail (3 jours)", Assert.Single(rows).DisplayValue);
    }

    [Fact]
    public void A_match_day_keeps_its_sessions_and_the_venue()
    {
        var day = new BridgeTrainingDayDto
        {
            Date = "2026-10-10",
            Events =
            [
                new BridgeCalendarEventDto { CalendarEvent = 1000, SessionIndex = 0, SessionType = 402, SessionName = "En déplacement", SessionCategory = 4, CanEdit = false },
                new BridgeCalendarEventDto { CalendarEvent = 1001, MatchVenue = "Extérieur" }
            ]
        };

        var training = day.Adapt<TrainingDay>(_config);

        var session = Assert.Single(training.Sessions);
        Assert.Equal(402, session.Type);
        Assert.False(session.CanEdit);
        Assert.Equal("Extérieur", training.Match);
    }

    private static BridgeOptionDto Row(long id, string name)
    {
        return new BridgeOptionDto { RowData = new BridgeOptionDto { PropertyValue = id, DisplayValue = name } };
    }
}
