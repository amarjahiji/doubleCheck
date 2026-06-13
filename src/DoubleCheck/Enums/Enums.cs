namespace DoubleCheck.Enums;

public enum MessageSender { User = 0, Ai = 1 }
public enum SessionStatus { Open = 0, Closed = 1 }
public enum SessionOutcome { None = 0, Resolved = 1, Cancelled = 2, Expired = 3 }
public enum TransactionType { Debit = 0, Credit = 1 }
public enum ApplicationStatus { Pending = 0, Approved = 1, Rejected = 2 }

public static class Roles
{
    public const string Common = "Common";
    public const string Professional = "Professional";
    public const string Admin = "Admin";
    public static readonly string[] All = { Common, Professional, Admin };
}
