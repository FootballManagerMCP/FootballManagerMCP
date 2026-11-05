namespace FootballManager.Application.Exceptions;

/// <summary>The game refused a write, e.g. a player already in the position they were to be picked for.</summary>
public class GameRejectedException(string message, Exception? innerException = null) : Exception(message, innerException);
