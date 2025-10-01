using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        public ChatHub(ApplicationDbContext context) => _context = context;

        // Đặt tên group theo userId
        private static string UserGroup(int userId) => $"user-{userId}";

        // Client sẽ gọi ngay sau khi start connection: RegisterUser(senderId)
        public async Task RegisterUser(int userId)
        {
            // Add connection hiện tại vào group của user
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

            // (tuỳ chọn) thông báo online cho người khác
            await Clients.Others.SendAsync("UserConnected", userId);
        }

        // Gửi tin nhắn 1-1
        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            // (Khuyến nghị) nếu có auth, xác thực senderId khớp Context.UserIdentifier

            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(senderId);

            var payload = new
            {
                id = newMessage.Id,
                senderId = senderId,
                senderName = sender?.Username ?? $"User {senderId}",
                receiverId = receiverId,
                content = message,
                sentAt = newMessage.SentAt.ToString("HH:mm dd/MM/yyyy")
            };

            // ✅ Fanout qua group-per-user (Redis backplane sẽ pub/sub giữa các pod)
            await Clients.Group(UserGroup(senderId)).SendAsync("ReceiveMessage", payload);
            await Clients.Group(UserGroup(receiverId)).SendAsync("ReceiveMessage", payload);
        }

        public async Task MarkAsRead(int messageId)
        {
            var msg = await _context.Messages.FindAsync(messageId);
            if (msg is null) return;
            msg.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task UserTyping(int senderId, int receiverId)
        {
            // Bắn typing tới group của người nhận
            await Clients.Group(UserGroup(receiverId)).SendAsync("UserIsTyping", senderId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Không cần tự RemoveFromGroup — SignalR tự xoá theo ConnectionId
            await base.OnDisconnectedAsync(exception);
        }
    }
}
