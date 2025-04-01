using Luxa.Models;

namespace Luxa.Interfaces
{
    public interface IPhotoRepository : IAsyncRepository<Photo>
    {
        bool Save();
        UserPhotoModel? GetUserPhotoModelByPhoto(int idPhoto, UserModel user);
        bool AddLikeToPhoto(UserPhotoModel userPhoto);
        bool RemoveLikeToPhoto(UserPhotoModel userPhoto);
        IQueryable<Photo> GetLikedPhotos(UserModel user);
        IQueryable<Photo> GetPhotosAsync();
        IQueryable<Photo> GetPhotosAsync(int pageNumber, int pageSize);
        IQueryable<Photo> GetPhotosOwnByUserAsync(int pageNumber, int pageSize, UserModel user);
        Task<Photo?> GetPhotoIncludedPhotoTags(int idPhoto);
        bool LikeCount(Photo photo);
        Task<bool> PhotoExistsAsync(int id);
        Task<IEnumerable<Photo>>GetPhotosWithOwner();
        Task<Photo?> GetOneWithOwner(int id);
    }
}
