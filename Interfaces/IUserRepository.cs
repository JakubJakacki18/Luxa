using Luxa.Models;

namespace Luxa.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> AddUserNotification(string userId, NotificationModel notification);
        Task<List<UserModel>> GetAllUsers();
        Task<UserNotificationModel?> GetUserNotificationById(string userId, int notificationId);
        Task<bool> Update(UserNotificationModel userNotification);
    }
}
