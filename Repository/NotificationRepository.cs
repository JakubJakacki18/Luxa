using Luxa.Data;
using Luxa.Interfaces;
using Luxa.Models;
using Microsoft.EntityFrameworkCore;

namespace Luxa.Repository
{
    public class NotificationRepository(ApplicationDbContext context) : INotificationRepository
    {
        public async Task<bool> Create(NotificationModel model)
        {
            await context.AddAsync(model);
            return await SaveAsync();
        }

        public async Task<bool> Delete(NotificationModel model)
        {
            context.Remove(model);
            return await SaveAsync();
        }

        public async Task<IEnumerable<NotificationModel>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<NotificationModel> GetOne(int id)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> SaveAsync()
            => await context.SaveChangesAsync() > 0;

        public async Task<bool> Update(NotificationModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<NotificationModel?> GetNotificationByTitle(string title)
            => await context.Notifications.SingleOrDefaultAsync(n => n.Title == title);
    }
}
