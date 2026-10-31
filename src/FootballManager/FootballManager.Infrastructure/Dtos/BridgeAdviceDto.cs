namespace FootballManager.Infrastructure.Dtos;

/// <summary>An assistant's comment in the tactical effectiveness; IsPro marks praise rather than a concern.</summary>
internal sealed class BridgeAdviceDto
{
    public string? Text { get; set; }
    public bool? IsPro { get; set; }
}
