using Luxa.Data;
using Luxa.Interfaces;
using Luxa.Models;
using Microsoft.EntityFrameworkCore;

namespace Luxa.Repository
{
	public class PhotoRepository : IPhotoRepository
	{
		private readonly ApplicationDbContext _context;
		public PhotoRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		public IQueryable<Photo> GetPhotosAsync()
			=> _context.Photo.Where(photo => photo.Owner.IsPrivate == false).AsQueryable();

		public IQueryable<Photo> GetPhotosAsync(int pageNumber, int pageSize)
			=> _context.Photo
				.OrderByDescending(photo => photo.AddTime)
				.Where(photo => photo.Owner.IsPrivate == false)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize);

		public IQueryable<Photo> GetPhotosOwnByUserAsync(int pageNumber, int pageSize, UserModel user)
			=> _context.Photo
				.Where(p => p.OwnerId == user.Id)
				.OrderByDescending(photo => photo.AddTime)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize);

		public IQueryable<Photo> GetLikedPhotos(UserModel user)
			=> _context.Users
				.Where(u => u.Id == user.Id)
				.SelectMany(u => u.UserLikedPhotos)
				.Select(u => u.Photo);

		public bool Save()
			=> _context.SaveChanges() > 0;

		private async Task<bool> SaveAsync()
			=> await _context.SaveChangesAsync() > 0;

		public bool RemoveLikeToPhoto(UserPhotoModel userPhoto)
		{
			_context.UserLikedPhotos.Remove(userPhoto);
			return Save();
		}
		public bool AddLikeToPhoto(UserPhotoModel userPhoto)
		{
			_context.UserLikedPhotos.Add(userPhoto);
			return Save();
		}

		public UserPhotoModel? GetUserPhotoModelByPhoto(int idPhoto, UserModel user)
			=> _context.UserLikedPhotos.FirstOrDefault(e => e.PhotoId == idPhoto && e.UserId == user.Id);

		public async Task<Photo?> GetPhotoIncludedPhotoTags(int photoId)
			=> await _context.Photo
				.Include(p => p.PhotoTags)
				.FirstOrDefaultAsync(p => p.Id == photoId);

        public bool LikeCount(Photo photo)
        {
            photo.LikeCount = _context.UserLikedPhotos.Count(ul => ul.PhotoId == photo.Id);
            return Save();
        }

        public async Task<bool> PhotoExistsAsync(int id)
        {
            return await _context.Photo.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Photo>> GetPhotosWithOwner()
	        => await _context.Photo.Include(m => m.Owner).ToListAsync();
        

        public async Task<IEnumerable<Photo>> GetAll() 
	        => await _context.Photo.Where(photo => photo.Owner.IsPrivate == false).ToListAsync();

        public async Task<Photo?> GetOne(int id)
			=> await _context.Photo.FindAsync(id);
        
        public async Task<bool> Create(Photo model)
        {
	        await _context.AddAsync(model);
	        return await SaveAsync();	
        }
        public async Task<Photo?> GetOneWithOwner(int id)
	        => await _context.Photo
		        .Include(p => p.Owner)
		        .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<bool> Update(Photo model)
        {
	        _context.Update(model);
	        return await SaveAsync();
        }

        public async Task<bool> Delete(Photo model)
        {
	        _context.Remove(model);
	        return await SaveAsync();
        }
	}
}
