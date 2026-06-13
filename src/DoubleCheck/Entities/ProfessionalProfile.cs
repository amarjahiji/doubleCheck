using DoubleCheck.Common;

namespace DoubleCheck.Entities;

public class ProfessionalProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal RatePerAnswer { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Bio { get; set; }
    public ICollection<ProfessionalCategory> Categories { get; set; } = new List<ProfessionalCategory>();
}
