namespace FootballManager.Api.Responses.Tactics.Editing;

public class ByPhaseResponse<T> where T : class
{
    public T? InPossession { get; set; }
    public T? OutOfPossession { get; set; }
}
