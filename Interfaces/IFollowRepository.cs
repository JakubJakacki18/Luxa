using Luxa.Models;

namespace Luxa.Interfaces
{
    public interface IFollowRepository : IAsyncRepository<FollowModel>
    {
        Task<int> GetFollowersCount(string userId);
        Task<FollowModel?> GetFollowModelByUserIds(string followerId, string followeeId);
    }
}
