using Luxa.Data;
using Luxa.Data.Enums;
using Luxa.Interfaces;
using Luxa.Models;
using Luxa.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Luxa.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly ApplicationDbContext _context; //wstrzykiwanie kontekstu
        private readonly IPhotoRepository _photoRepository;
        private readonly UserManager<UserModel> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ITagService _tagService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PhotoService(ApplicationDbContext context,
            UserManager<UserModel> userManager,
            IWebHostEnvironment hostEnvironment,
            IPhotoRepository photoRepository,
            ITagService tagService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
            _photoRepository = photoRepository;
            _tagService = tagService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Photo?> GetImageByIdAsync(int id)
        {
            return await _photoRepository.GetOne(id);
        }

        public async Task<IEnumerable<Photo>> GetAllImagesAsync()
        {
            return await _context.Photo.ToListAsync();
        }
        
        public async Task<bool> Create(Photo photo, UserModel user, string tags)
        {
            if (!_tagService.Add(tags))
                return false;
            List<TagModel> tagsToPhoto = _tagService.GetTagsFromString(tags);
            photo.Owner = user;
            //save image into wwwroot
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(photo.ImageFile.FileName);
            string extension = Path.GetExtension(photo.ImageFile.FileName);
            photo.Name = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/Image", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await photo.ImageFile.CopyToAsync(fileStream);
            }
            if (!await _photoRepository.Create(photo))
                return false;
            return AddTagsToPhoto(photo, tagsToPhoto);
        }


        public Task<bool> Delete()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(int id, Photo photo, UserModel user)
        {
            throw new NotImplementedException();
        }

        
        public bool IsPhotoLiked(int idPhoto, List<Photo> photos)
            => photos.Select(e => e.Id).Contains(idPhoto);

        public async Task<bool> LikePhoto(int idPhoto, UserModel user)
        {
            var photo = await _photoRepository.GetOne(idPhoto);
            var userPhoto = new UserPhotoModel
            {
                PhotoId = idPhoto,
                Photo = photo,
                User = user,
                UserId = user.Id
            };
            return _photoRepository.AddLikeToPhoto(userPhoto);
        }

        public bool UnlikePhoto(int idPhoto, UserModel user)
        {
            var userPhoto = _photoRepository.GetUserPhotoModelByPhoto(idPhoto, user);
            return (userPhoto != null) && _photoRepository.RemoveLikeToPhoto(userPhoto);
        }

        public async Task<List<PhotoWithIsLikedVM>> GetPhotosWithIsLikedAsync(int pageNumber, int pageSize,
            UserModel user)
        {
            var allPhotos = await _photoRepository.GetPhotosAsync(pageNumber, pageSize)
                .Include(photo => photo.Owner)
                .ToListAsync();
            foreach (var photo in allPhotos)
            {
                _photoRepository.LikeCount(photo);
            }
            var likedPhotos = await GetLikedPhotos(user);
            var likedPhotoIds = new HashSet<int>(likedPhotos.Select(p => p.Id));
            var photosWithIsLiked = allPhotos.Select(photo => new PhotoWithIsLikedVM
            {
                Photo = photo,
                IsLiked = likedPhotoIds.Contains(photo.Id),
                OwnerName = photo.Owner.UserName
            }).ToList();
            return photosWithIsLiked;


        }

        public async Task<List<PhotoWithIsLikedVM>> GetPhotosWithIsLikedForProfileAsync(int pageNumber, int pageSize,
            UserModel user)
        {
            var allPhotos = await _photoRepository.GetPhotosOwnByUserAsync(pageNumber, pageSize, user)
                .Include(photo => photo.Owner)
                .ToListAsync();
            foreach (var photo in allPhotos)
            {
                _photoRepository.LikeCount(photo);
            }
            var likedPhotos = await GetLikedPhotos(user);
            var likedPhotoIds = new HashSet<int>(likedPhotos.Select(p => p.Id));
            var photosWithIsLiked = allPhotos.Select(photo => new PhotoWithIsLikedVM
            {
                Photo = photo,
                IsLiked = likedPhotoIds.Contains(photo.Id),
                OwnerName = photo.Owner.UserName
            }).ToList();
            return photosWithIsLiked;
        }

        public async Task<List<Photo>> GetLikedPhotos(UserModel user)
            => await _photoRepository.GetLikedPhotos(user).ToListAsync();




        public bool AddTagsToPhoto(Photo photo, List<TagModel> tagNames)
        {
            //var photo = await _photoRepository.GetPhotoIncludedPhotoTags(idPhoto);
            //if (photo == null)
            //{
            //	throw new Exception("Nie znaleziono zdjęcia");
            //}
            foreach (var tag in tagNames)
            {
                if (photo.PhotoTags.All(pt => pt.TagId != tag.Id))
                {
                    photo.PhotoTags.Add(new PhotoTagModel { PhotoId = photo.Id, TagId = tag.Id });
                }
            }

            return _photoRepository.Save();
        }

        private bool IncrementViewCountAsync(List<Photo> photos)
        {
            foreach (var item in photos)
            {
                item.Views++;
            }
            return _photoRepository.Save();
        }

        public async Task<List<PhotoWithIsLikedVM>> GetPhotosWithIsLikedForDiscoverAsync(int pageNumber,
            int pageSize,
            UserModel user,
            string? tag = "",
            string? category = "",
            bool order = false,
            string? sortBy = "")
        {
            var photos = _photoRepository.GetPhotosAsync();
            if (tag != "" && tag != null)
            {
                photos = photos.Where(p => p.PhotoTags.Any(pt => pt.Tag.TagName == tag));
            }
            var enumCategory = GetEnumCategory(category);
            if (enumCategory != null)
            {
                photos = photos.Where(p => p.Category == enumCategory);
            }
            string sortByWithDirection = sortBy + GetOrder(order);
            photos = sortByWithDirection switch
            {
                "date" => photos.OrderBy(p => p.AddTime),
                "views" => photos.OrderBy(p => p.Views),
                "date_Desc" => photos.OrderByDescending(p => p.AddTime),
                "views_Desc" => photos.OrderByDescending(p => p.Views),
                _ => photos.OrderByDescending(p => p.AddTime),
            };
            photos = photos.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var listPhotos = await photos.Include(p => p.Owner).ToListAsync();
            foreach (var photo in listPhotos)
            {
                _photoRepository.LikeCount(photo);
            }
            var likedPhotos = await GetLikedPhotos(user);
            var likedPhotoIds = new HashSet<int>(likedPhotos.Select(p => p.Id));
            var photosWithIsLiked = listPhotos.Select(photo => new PhotoWithIsLikedVM
            {
                Photo = photo,
                IsLiked = likedPhotoIds.Contains(photo.Id),
                OwnerName = photo.Owner.UserName
            }).ToList();
            return photosWithIsLiked;
        }
        private string GetOrder(bool order)
            => (order) ? "" : "_Desc";

        private CategoryOfPhotos? GetEnumCategory(string? category)
        {
            if (category == null)
            {
                return null;
            }

            foreach (CategoryOfPhotos item in Enum.GetValues(typeof(CategoryOfPhotos)))
            {
                if (category.Equals(item.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }

        public void IncrementViewsCountIfNotViewed(List<PhotoWithIsLikedVM> photos)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            List<Photo> viewedPhotos = [];
            if (httpContext != null)
            {
                foreach (var photo in photos)
                {
                    var sessionKey = $"viewed_photo_{photo.Photo.Id}";
                    if (httpContext.Session.GetString(sessionKey) == null)
                    {
                        httpContext.Session.SetString(sessionKey, "viewed");
                        viewedPhotos.Add(photo.Photo);
                    }
                }
                IncrementViewCountAsync(viewedPhotos);
            }
        }
        /*public Task<Photo> GetImageByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Photo>> GetAllImagesAsync()
        {
            throw new NotImplementedException();
        }*/

        public async Task<Photo?> GetPhotoAsync(int id)
        {
            return await _photoRepository.GetOne(id);
        }

        public async Task EditPhotoAsync(Photo photo)
        {
            await _photoRepository.Update(photo);
        }

        public async Task<bool> PhotoExistsAsync(int id)
            => await _photoRepository.PhotoExistsAsync(id);

        public async Task<IEnumerable<Photo>> GetPhotosWithOwner()
            => await _photoRepository.GetPhotosWithOwner();
        public string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }

        public async Task<Photo?> GetOneWithOwner(int id)
            => await _photoRepository.GetOneWithOwner(id);

        public async Task<bool> Delete(Photo photo)
            => await _photoRepository.Delete(photo);
    }
}