namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>A report's view of what signing the player would take.</summary>
internal sealed class BridgeTransferInfoDto
{
    public BridgeReportLineDto? ExpectedTransferValue { get; set; }
    public BridgeReportLineDto? ExpectedWageDemands { get; set; }
    public BridgeReportLineDto? ExpectedTransferInterest { get; set; }
    public BridgeReportLineDto? ExpectedLoanInterest { get; set; }
}
