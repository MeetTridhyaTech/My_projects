using Microsoft.AspNetCore.SignalR;
using StudentCorewebAPI_Project.Models;
using StudentCorewebAPI_Project.Repository;
using StudentCorewebAPI_Project.Repository_Interface;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace StudentCorewebAPI_Project.SignalR
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<Guid, string> _connections = new();
        private readonly IChatRepository _chatRepository;

        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.GetHttpContext()?.Request.Query["userId"];

            if (Guid.TryParse(userIdStr, out var userId))
            {
                _connections[userId] = Context.ConnectionId;
                await Clients.All.SendAsync("UserConnected", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (userId != Guid.Empty)
            {
                _connections.TryRemove(userId, out _);
                await Clients.All.SendAsync("UserDisconnected", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Guid receiverId, string message)
        {
            var senderIdClaim = Context.User?.Claims?.FirstOrDefault(c => c.Type == "UserId");

            if (!Guid.TryParse(senderIdClaim?.Value, out var senderId))
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized or invalid sender ID.");
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", "Message cannot be empty.");
                return;
            }

            var chat = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message
            };

            var (success, error) = await _chatRepository.SendMessageAsync(chat);
            if (!success)
            {
                await Clients.Caller.SendAsync("Error", error);
                return;
            }

            if (_connections.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", chat);
            }

            await Clients.Caller.SendAsync("MessageSent", chat);
        }

        public async Task<List<ChatMessage>> GetChatHistory(Guid receiverId)
        {
            var senderIdClaim = Context.User?.Claims?.FirstOrDefault(c => c.Type == "UserId");

            if (!Guid.TryParse(senderIdClaim?.Value, out var senderId))
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized.");
                return new List<ChatMessage>();
            }

            return await _chatRepository.GetChatHistoryAsync(senderId, receiverId);
        }

        public async Task SendTypingNotification(Guid recieverId)
        {
            var senderIdClaim = Context.User?.Claims?.FirstOrDefault(c => c.Type == "UserId");

            if(!Guid.TryParse(senderIdClaim?.Value, out var senderId))
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized or invalid sender ID.");
                return;
            }

            if(_connections.TryGetValue(recieverId, out var recieverConnectionId))
            {
                await Clients.Client(recieverConnectionId).SendAsync("UserTyping", senderId);
            }
        }
    }
}
