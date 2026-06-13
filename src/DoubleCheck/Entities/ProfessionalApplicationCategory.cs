namespace DoubleCheck.Entities;

public class ProfessionalApplicationCategory
{
    public Guid ProfessionalApplicationId { get; set; }
    public ProfessionalApplication? ProfessionalApplication { get; set; }
    public Guid CategoryId { get; set; }
}
