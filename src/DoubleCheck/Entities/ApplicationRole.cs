using Microsoft.AspNetCore.Identity;

namespace DoubleCheck.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string name) : base(name) { }
}
