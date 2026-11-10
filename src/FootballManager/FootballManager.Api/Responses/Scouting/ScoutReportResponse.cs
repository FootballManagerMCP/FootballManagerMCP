namespace FootballManager.Api.Responses.Scouting;

public class ScoutReportResponse
{
    public string? Date { get; set; }
    public string? Scout { get; set; }
    public string? Ability { get; set; }
    public string? Potential { get; set; }
    public List<string>? Pros { get; set; }
    public List<string>? Cons { get; set; }
    public string? TransferFee { get; set; }
    public string? WageDemands { get; set; }
    public string? TransferInterest { get; set; }
    public string? LoanInterest { get; set; }
}
