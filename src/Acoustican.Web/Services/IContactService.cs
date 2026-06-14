using Acoustican.Models;

namespace Acoustican.Services
{
    public interface IContactService
    {
        Task<ContactMessage> CreateContactMessageAsync(ContactMessage message);
        Task<List<ContactMessage>> GetAllContactMessagesAsync();
        Task<ContactMessage?> GetContactMessageByIdAsync(int id);
        Task<ContactMessage?> MarkAsReadAsync(int id);
        Task<bool> DeleteContactMessageAsync(int id);
    }
}
