public class RoleMenuDto
{
    public string RoleName { get; set; }
    public List<MenuWithPermissionsDto> Menus { get; set; }
}

public class MenuWithPermissionsDto
{
    public string Title { get; set; }
    public List<string> Permissions { get; set; }
}
 