using StudentCorewebAPI_Project.Models;

namespace StudentCorewebAPI_Project.Repository
{
    public interface IChatRepository
    {
        Task<(bool Success, string ErrorMessage)> SendMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetChatHistoryAsync(Guid senderId, Guid receiverId);
    }
}
