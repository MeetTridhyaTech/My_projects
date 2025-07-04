//using Microsoft.EntityFrameworkCore;
//using StudentCorewebAPI_Project.Models;
//using StudentCorewebAPI_Project.Data;
//using StudentCorewebAPI_Project.Migrations;

//namespace StudentCorewebAPI_Project.Repository
//{
//    public class ChatRepository
//    {
//        private readonly ApplicationDbContext _context;

//        public ChatRepository(ApplicationDbContext context)
//        {
//            _context = context; 
//        }

//        //public async Task SaveMessageAsync(ChatMessage message)
//        //{
//        //    await _context.ChatMessages.AddAsync(message);
//        //    await _context.SaveChangesAsync();
//        //}

//        public async Task<(bool Success, string ErrorMessage)> SendMessageAsync(ChatMessage message)
//        {
//            var senderExists = await _context.Users.AnyAsync(u => u.Id == message.SenderId && !u.IsDeleted);
//            var receiverExists = await _context.Users.AnyAsync(u => u.Id == message.ReceiverId && !u.IsDeleted);

//            if (!senderExists || !receiverExists)
//            {
//                return (false, "Sender or receiver does not exist in the system.");
//            }

//            message.SentAt = DateTime.UtcNow;

//            await _context.ChatMessages.AddAsync(message);
//            await _context.SaveChangesAsync();

//            return (true, "Message saved successfully");
//        }

//        public async Task<List<ChatMessage>> GetChatHistoryAsync(Guid senderId, Guid receiverId)
//        {
//            return await _context.ChatMessages
//                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
//                            (m.SenderId == receiverId && m.ReceiverId == senderId))
//                .OrderBy(m => m.SentAt)
//                .ToListAsync();
//        }

//    }
//}



using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using StudentCorewebAPI_Project.Models;

namespace StudentCorewebAPI_Project.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string ErrorMessage)> SendMessageAsync(ChatMessage message)
        {
            var senderExists = await _context.Users.AnyAsync(u => u.Id == message.SenderId && !u.IsDeleted);
            var receiverExists = await _context.Users.AnyAsync(u => u.Id == message.ReceiverId && !u.IsDeleted);

            if (!senderExists || !receiverExists)
            {
                var missing = !senderExists ? "Sender" : "Receiver";
                return (false, $"{missing} does not exist in the system. SenderId: {message.SenderId}, ReceiverId: {message.ReceiverId}");
            }

            message.SentAt = DateTime.UtcNow;
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            return (true, "Message saved successfully");
        }


        public async Task<List<ChatMessage>> GetChatHistoryAsync(Guid senderId, Guid receiverId)
        {
            return await _context.ChatMessages
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}

