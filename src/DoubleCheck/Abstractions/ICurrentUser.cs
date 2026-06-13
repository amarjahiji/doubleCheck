namespace DoubleCheck.Abstractions;

/// <summary>The authenticated caller, resolved from the JWT.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
