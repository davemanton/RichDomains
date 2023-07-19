namespace Application.Exceptions;

public class ValidationException : Exception
{
    public readonly IDictionary<string, string> Errors;

    public ValidationException(string message, 
                               IDictionary<string, string> errors)
        : base(message)
    {
        Errors = errors;
    }
}