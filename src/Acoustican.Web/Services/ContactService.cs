using Acoustican.Data;
using Acoustican.Models;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services
{
    public class ContactService(ApplicationDbContext context, ILogger<ContactService> logger) : IContactService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ContactService> _logger = logger;

        public async Task<ContactMessage> CreateContactMessageAsync(ContactMessage message)
        {
            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created contact message from {message.Email}");
            return message;
        }

        public async Task<List<ContactMessage>> GetAllContactMessagesAsync()
        {
            return await _context.ContactMessages
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<ContactMessage?> GetContactMessageByIdAsync(int id)
        {
            return await _context.ContactMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<ContactMessage?> MarkAsReadAsync(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                return null;
            }
            message.IsRead = true;
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> DeleteContactMessageAsync(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                return false;
            }
            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
