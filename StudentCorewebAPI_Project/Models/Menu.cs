using System;
using System.Collections.Generic;

namespace StudentCorewebAPI_Project.Models
{
    public class Menu
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public int Order { get; set; }
        public Guid? ParentMenuId { get; set; }
        public string RoleName { get; set; } // This property is used to store the role name associated with the menu item
        public bool IsActive { get; set; } = true;

        // Navigation property for sub-menus (self-referencing)
        public List<Menu> SubMenus { get; set; } = new List<Menu>();
    }
}
