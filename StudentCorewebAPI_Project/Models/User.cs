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
        [JsonIgnore]
        public string? PasswordHash { get; set; }
        [JsonIgnore]
        public string? PasswordSalt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? ResetOTP { get; set; }
        public DateTime? OTPExpiryTime { get; set; }
    }
}
