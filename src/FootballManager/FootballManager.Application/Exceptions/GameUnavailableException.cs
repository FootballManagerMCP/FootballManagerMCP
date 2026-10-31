namespace FootballManager.Application.Exceptions;

/// <summary>The game isn't running, the bridge plugin isn't loaded, or the game couldn't answer.</summary>
public class GameUnavailableException(string message, Exception? innerException = null) : Exception(message, innerException);
