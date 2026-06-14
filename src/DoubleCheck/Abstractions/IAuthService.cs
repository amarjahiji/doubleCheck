using DoubleCheck.Dtos;

namespace DoubleCheck.Abstractions;

/// <summary>Registration, login, and current-user lookup (Amar).</summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<MeResponse> GetMeAsync(Guid userId, CancellationToken ct = default);
}
