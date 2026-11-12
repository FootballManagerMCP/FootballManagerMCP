namespace FootballManager.Api.Responses.Competitions;

public class CompetitionListResponse
{
    /// <summary>The user's league first, then the other leagues whose stats the game keeps, nation by nation.</summary>
    public List<CompetitionResponse> Competitions { get; set; } = [];
}
