using DoubleCheck.Common;

namespace DoubleCheck.Entities;

public class Conversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
