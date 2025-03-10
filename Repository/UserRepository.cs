using Luxa.Data;
using Luxa.Interfaces;
using Luxa.Models;
using Microsoft.EntityFrameworkCore;

namespace Luxa.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddUserNotification(string userId, NotificationModel notification)
        {
            var userNotification = new UserNotificationModel
            {
                UserId = userId,
                Notification = notification
            };
            _context.UserNotifications.Add(userNotification);
            return await SaveAsync();
        }

        private async Task<bool> SaveAsync()
            => await _context.SaveChangesAsync() > 0;
        

        public async Task<List<UserModel>> GetAllUsers()
            => await _context.Users.ToListAsync();

        public async Task<UserNotificationModel?> GetUserNotificationById(string userId, int notificationId)
            => await _context.UserNotifications.SingleOrDefaultAsync(un => un.NotificationId == notificationId && un.UserId == userId);

        public async Task<bool> Update(UserNotificationModel userNotification) 
        {
            _context.Update(userNotification);
            return await SaveAsync();
        }

    }
}
