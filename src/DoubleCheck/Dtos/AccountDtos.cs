namespace DoubleCheck.Dtos;

public record WalletTransactionResponse(
    decimal Amount,
    string Type,
    string Reason,
    Guid? RelatedSessionId,
    DateTime CreatedAt);

public record BalanceResponse(
    decimal Balance,
    IReadOnlyList<WalletTransactionResponse> RecentTransactions);
