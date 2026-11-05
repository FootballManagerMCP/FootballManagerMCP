namespace FootballManager.Domain.Tactics.Editing;

/// <summary>A value for each phase of play, in possession and out of it; either can be left out.</summary>
public sealed class ByPhase<T> where T : class
{
    public T? InPossession { get; set; }
    public T? OutOfPossession { get; set; }

    public T? For(bool inPossession)
    {
        return inPossession ? InPossession : OutOfPossession;
    }
}
