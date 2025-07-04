using System;
using System.Collections.Generic;

namespace StudentCorewebAPI_Project.DTOs
{
    public class MenuDto
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public int Order { get; set; }
        public Guid? ParentMenuId { get; set; }
        public string RoleName { get; set; } // Include this

        //public List<SubMenuDto> SubMenus { get; set; } = new List<SubMenuDto>();
    }

    //public class SubMenuDto
    //{
    //    public string Title { get; set; }
    //    public string Icon { get; set; }
    //    public string Path { get; set; }
    //    public int Order { get; set; }
    //}
}
