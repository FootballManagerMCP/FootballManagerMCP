using FluentResults;

namespace FootballManager.Application.ResultErrors;

public class GameUnavailableError(string message) : Error(message) { }
