using System.ComponentModel.DataAnnotations;

namespace StudentCorewebAPI_Project.Models
{
    public class AddUser
    {
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public long? Mobile { get; set; }
    }

}
