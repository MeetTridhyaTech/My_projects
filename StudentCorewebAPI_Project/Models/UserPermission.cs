
using System;

namespace StudentCorewebAPI_Project.Models;

public class AssignMenuToRole
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MenuId { get; set; }

    public Guid RoleID { get; set; }
    public Menu Menu { get; set; }
    public Role Role { get; set; }
}
