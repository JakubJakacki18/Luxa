using Luxa.Models;
using Luxa.Repository;

namespace Luxa.Interfaces
{
    public interface INotificationRepository : IAsyncRepository<NotificationModel>
    {
        Task<NotificationModel?>GetNotificationByTitle(string title);

    }
}
