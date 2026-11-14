namespace FootballManager.Api.Requests.Training;

public class SetTrainingRequest
{
    /// <summary>Players whose own training changes.</summary>
    public List<PlayerTrainingRequest>? Players { get; set; }

    /// <summary>Sessions of this week's or next week's schedule to replace.</summary>
    public List<SessionRequest>? Sessions { get; set; }
}
