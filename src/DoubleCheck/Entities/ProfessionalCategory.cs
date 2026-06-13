namespace DoubleCheck.Entities;

public class ProfessionalCategory
{
    public Guid ProfessionalProfileId { get; set; }
    public ProfessionalProfile? ProfessionalProfile { get; set; }
    public Guid CategoryId { get; set; }
}
