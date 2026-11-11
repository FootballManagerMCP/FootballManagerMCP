using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Trials;

public class TrialPlayerResponse
{
    public int PlayerId { get; set; }
    public required string Name { get; set; }

    /// <summary>Where he stands with a trial: after the change once sent, now on a dry run.</summary>
    public TrialState? Trial { get; set; }

    /// <summary>As the game reads back after the change (it shortlists the players it's offered a trial to); for a dry run, as it is now.</summary>
    public bool? IsShortlisted { get; set; }

    /// <summary>Only when it differs from the first player's (withdrawing an offer rather than ending a trial, say).</summary>
    public string? Choice { get; set; }

    /// <summary>Only when it says something other than the first player's.</summary>
    public string? Confirmation { get; set; }
}
