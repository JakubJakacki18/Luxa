using Luxa.Interfaces;
using Luxa.Models;

namespace Luxa.Services
{
    public class FollowService(IFollowRepository followRepository) : IFollowService
    {
        public async Task<int> GetFollowersCount(string userId)
            => await followRepository.GetFollowersCount(userId);

        public async Task<FollowModel?> GetFollowModelByUserIds(string followerId, string followeeId)
            => await followRepository.GetFollowModelByUserIds(followerId, followeeId);
        public async Task<bool> Create(FollowModel model)
            => await followRepository.Create(model);
        public async Task<bool> Delete(FollowModel model)
            => await followRepository.Delete(model);


    }
}
