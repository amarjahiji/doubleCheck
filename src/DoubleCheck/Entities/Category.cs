using DoubleCheck.Common;

namespace DoubleCheck.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
