using Luxa.Models;

namespace Luxa.Interfaces
{
    public interface IFollowService
    {
        Task<int> GetFollowersCount(string userId);
        Task<FollowModel?> GetFollowModelByUserIds(string followerId, string followeeId);
        Task<bool> Create(FollowModel model);
        Task<bool> Delete(FollowModel model);
    }
}
