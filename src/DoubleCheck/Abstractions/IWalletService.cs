namespace DoubleCheck.Abstractions;

/// <summary>Simulated wallet. Implemented in Services (Amar); consumed by the verification
/// service (Drin), who mocks this interface in unit tests.</summary>
public interface IWalletService
{
    Task<decimal> GetBalanceAsync(Guid userId, CancellationToken ct = default);
    Task<bool> TryDebitAsync(Guid userId, decimal amount, Guid? sessionId, string reason, CancellationToken ct = default);
    Task CreditAsync(Guid userId, decimal amount, Guid? sessionId, string reason, CancellationToken ct = default);
}
