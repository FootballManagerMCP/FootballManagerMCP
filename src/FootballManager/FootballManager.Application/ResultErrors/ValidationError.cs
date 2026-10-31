using FluentResults;

namespace FootballManager.Application.ResultErrors;

public class ValidationError(string propertyName, IEnumerable<string> errorMessages) : Error()
{
    public string PropertyName { get; } = propertyName;
    public IEnumerable<string> ErrorMessages { get; } = errorMessages;
}
