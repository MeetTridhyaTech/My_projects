using Microsoft.AspNetCore.Mvc;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.Repository;
using StudentCorewebAPI_Project.DTOs;
using System.Security.Claims;

namespace StudentCorewebAPI_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;

        public ChatController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message cannot be empty");

            var senderIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (senderIdClaim == null)
                return Unauthorized("Invalid token");

            if (!Guid.TryParse(senderIdClaim.Value, out var senderId))
                return BadRequest("Invalid user ID in token");

            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Message = dto.Message
            };

            var (success, errorMessage) = await _chatRepository.SendMessageAsync(message);

            if (!success)
                return BadRequest(errorMessage);

            return Ok(new { message = "Message saved successfully" });
        }

        [HttpGet("history/{receiverId}")]
        public async Task<IActionResult> GetChatHistory(Guid receiverId)
        {
            var senderIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (senderIdClaim == null)
                return Unauthorized("Invalid token");

            if (!Guid.TryParse(senderIdClaim.Value, out var senderId))
                return BadRequest("Invalid user ID in token");

            var messages = await _chatRepository.GetChatHistoryAsync(senderId, receiverId);
            return Ok(messages);
        }
    }
}

