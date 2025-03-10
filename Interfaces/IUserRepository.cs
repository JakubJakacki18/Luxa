using Luxa.Models;

namespace Luxa.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> AddUserNotification(string userId, NotificationModel notification);
    }
}
