using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Driver;

namespace StudentCorewebAPI_Project.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public long? Mobile { get; set; }
        public string? PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
