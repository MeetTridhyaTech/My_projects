using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentCorewebAPI_Project.Models
{
    public class ChatMessage
    {
        [JsonIgnore]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}           
