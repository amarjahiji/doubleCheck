using DoubleCheck.Dtos;

namespace DoubleCheck.Abstractions;

public interface IAccountService
{
    Task<BalanceResponse> GetBalanceAsync(Guid userId, CancellationToken ct = default);
}
