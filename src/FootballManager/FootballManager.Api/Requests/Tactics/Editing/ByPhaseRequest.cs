namespace FootballManager.Api.Requests.Tactics.Editing;

public class ByPhaseRequest<T> where T : class
{
    public T? InPossession { get; set; }
    public T? OutOfPossession { get; set; }
}
