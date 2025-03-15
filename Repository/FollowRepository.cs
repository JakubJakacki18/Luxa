using Luxa.Data;
using Luxa.Interfaces;
using Luxa.Models;
using Microsoft.EntityFrameworkCore;

namespace Luxa.Repository
{
    public class FollowRepository(ApplicationDbContext context) : IFollowRepository
    {
        public async Task<bool> Create(FollowModel model)
        {
            await context.AddAsync(model);
            return await SaveAsync();
        }

        public async Task<bool> Delete(FollowModel model)
        {
            context.Remove(model);
            return await SaveAsync();
        }

        public async Task<IEnumerable<FollowModel>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<FollowModel> GetOne(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(FollowModel model)
        {
            throw new NotImplementedException();
        }
        private async Task<bool> SaveAsync()
            => await context.SaveChangesAsync() > 0;

        public async Task<int> GetFollowersCount(string userId)
            => await context.FollowRequests.CountAsync(fr => fr.FolloweeId == userId && fr.IsApproved);
        public async Task<FollowModel?> GetFollowModelByUserIds(string followerId,string followeeId) 
            => await context.FollowRequests.FirstOrDefaultAsync(fr => fr.FollowerId == followerId && fr.FolloweeId == followeeId);
    }
}
