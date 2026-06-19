namespace AgenticFlow.Domain.Exceptions;

/// <summary>
/// Exception thrown by an AI runner/backend invocation.
/// </summary>
public class AIRunnerException : DomainException
{
    public AIRunnerException(string message) : base(message) { }

    public AIRunnerException(string message, Exception? innerException)
        : base(message, innerException) { }
}
