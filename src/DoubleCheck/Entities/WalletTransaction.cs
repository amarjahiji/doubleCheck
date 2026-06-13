using DoubleCheck.Common;
using DoubleCheck.Enums;

namespace DoubleCheck.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public Guid? RelatedSessionId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
