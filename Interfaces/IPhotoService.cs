using Luxa.Models;
using Luxa.ViewModel;

namespace Luxa.Interfaces
{
    public interface IPhotoService
    {
        Task<bool> Create(Photo photo, UserModel user, string tags);//post
        Task<Photo?> GetImageByIdAsync(int id);
        Task<IEnumerable<Photo>> GetAllImagesAsync();

        //List<Photo>[] Prototyp(List<Photo> photos, int columnHeight);
        ///LimitedHeightPhotosVM GetAmountOfPhotos(int quantity,int height);
        Task<List<PhotoWithIsLikedVM>> GetPhotosWithIsLikedAsync(int pageNumber, int pageSize, UserModel user);
        Task<List<PhotoWithIsLikedVM>> GetPhotosWithIsLikedForProfileAsync(int pageNumber, int pageSize, UserModel user);
        Task<List<PhotoWithIsLikedVM>> GetPhotosWithIsLikedForDiscoverAsync(int pageNumber, int pageSize, UserModel user, string? tag = "", string? category = "", bool order = false, string? sortBy = "");
        Task<List<Photo>> GetLikedPhotos(UserModel user);
        bool IsPhotoLiked(int idPhoto, List<Photo> photos);
        Task<bool> LikePhoto(int idPhoto, UserModel user);
        bool UnlikePhoto(int idPhoto, UserModel user);
        // bool IncrementViewCountAsync(List<Photo> photo);
        void IncrementViewsCountIfNotViewed(List<PhotoWithIsLikedVM> photos);
        Task EditPhotoAsync(Photo photo);
        Task<bool> PhotoExistsAsync(int id);

        Task<IEnumerable<Photo>>GetPhotosWithOwner();
        string GetContentType(string extension);
        Task<Photo?> GetOneWithOwner(int id); 
        Task<bool> Delete(Photo photo);
    }
}
