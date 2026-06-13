using DoubleCheck.Common;
using DoubleCheck.Enums;

namespace DoubleCheck.Entities;

public class ProfessionalApplication : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal RequestedRate { get; set; }
    public string? Bio { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime? DecidedAt { get; set; }
    public ICollection<ProfessionalApplicationCategory> Categories { get; set; } = new List<ProfessionalApplicationCategory>();
}
