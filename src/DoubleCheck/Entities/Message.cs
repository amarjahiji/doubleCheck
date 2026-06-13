using DoubleCheck.Common;
using DoubleCheck.Enums;

namespace DoubleCheck.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
    public MessageSender Sender { get; set; }
    public string Content { get; set; } = string.Empty;
}
