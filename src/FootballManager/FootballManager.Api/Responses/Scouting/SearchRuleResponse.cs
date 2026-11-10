using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Scouting;

public class SearchRuleResponse
{
    public SearchRuleKind Kind { get; set; }
    public string? Name { get; set; }
    public string? Comparison { get; set; }
    public string? Value { get; set; }
}
