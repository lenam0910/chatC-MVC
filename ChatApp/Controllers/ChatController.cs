using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chính - danh sách users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Trang chat với một user cụ thể
        public async Task<IActionResult> Chat(int senderId, int receiverId)
        {
            var sender = await _context.Users.FindAsync(senderId);
            var receiver = await _context.Users.FindAsync(receiverId);

            if (sender == null || receiver == null)
            {
                return NotFound();
            }

            // Lấy lịch sử tin nhắn
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                           (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.SenderId = senderId;
            ViewBag.SenderName = sender.Username;
            ViewBag.ReceiverId = receiverId;
            ViewBag.ReceiverName = receiver.Username;
            ViewBag.Messages = messages;

            return View();
        }

        // API để lấy lịch sử tin nhắn
        [HttpGet]
        public async Task<IActionResult> GetMessages(int senderId, int receiverId)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                           (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    senderName = m.Sender.Username,
                    receiverId = m.ReceiverId,
                    content = m.Content,
                    sentAt = m.SentAt.ToString("HH:mm dd/MM/yyyy"),
                    isRead = m.IsRead
                })
                .ToListAsync();

            return Json(messages);
        }
    }
}
