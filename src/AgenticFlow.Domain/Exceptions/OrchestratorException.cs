namespace AgenticFlow.Domain.Exceptions;

/// <summary>
/// Exception thrown by the orchestration engine.
/// </summary>
public class OrchestratorException : DomainException
{
    public OrchestratorException(string message) : base(message) { }

    public OrchestratorException(string message, Exception? innerException)
        : base(message, innerException) { }
}
