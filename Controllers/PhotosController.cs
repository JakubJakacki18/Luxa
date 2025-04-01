using Luxa.Data;
using Luxa.Interfaces;
using Luxa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using AspNetCore;

namespace Luxa.Controllers
{
    public class PhotosController(IUserService userService, ApplicationDbContext context, UserManager<UserModel> userManager, IWebHostEnvironment hostEnvironment, IPhotoService photoService, ICommentService commentService) : Controller
    {
        private readonly IPhotoService _photoService = photoService;
        private readonly UserManager<UserModel> _userManager = userManager;
        private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;
        private readonly IUserService _userService = userService;
        private readonly ICommentService _commentService = commentService;

        [Authorize(Roles = "admin,moderator")]
        public async Task<IActionResult> Index()
            //Nie powinno się z tego co wiem przekazywać UserModela do widoku ale inaczej bez tworzenia ViewModelu
            //Bez Include się nie wyświetla ale trzeba to przerobić albo na vm albo pobawić się ViewBagami
            // => View(await _context.Photo.Include(m => m.Owner).ToListAsync());
            => View(await _photoService.GetPhotosWithOwner());

        [Authorize]
        public async Task<IActionResult> DownloadImage(int id)
        {
            var photo = await _photoService.GetImageByIdAsync(id);
            if (photo == null || string.IsNullOrEmpty(photo.Name))
                return RedirectToAction("Error", "Home");
            string filePath = Path.Combine(_hostEnvironment.WebRootPath, "Image/" + photo.Name);
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            string extension = Path.GetExtension(photo.Name);
            string contentType = _photoService.GetContentType(extension);
            return File(fileBytes, contentType, photo.Name);
        }
        
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var photo = await _photoService.GetOneWithOwner(id);
            if (photo == null)
            {
                return NotFound();
            }
            var comments = await _commentService.GetCommentsForPhoto(id);
            ViewData["Comments"] = comments;
            ViewBag.PhotoId = id;
            return View(photo);
        }

        // GET: Photos/Create
        [Authorize]
        public IActionResult Create()
            => _userManager.GetUserId(User) == null ? View("Views/Home/Index.cshtml") : View();


        // POST: Photos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Category,AddTime,ImageFile")] Photo photo, string Tags)
        {
            if (string.IsNullOrEmpty(Tags))
            {
                TempData["errorMessange"] = "Tagi nie mogą być puste. Dodajesz je za pomocą spacji lub przecinka.";
                return View();
            }
            var user = _userService.GetCurrentLoggedInUser(User);
            if (user == null)
                return Unauthorized();
            await _photoService.Create(photo, user, Tags);
            return RedirectToAction("Index", "Home");
            //return View(photo);
        }


        // GET: Photos/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {

            var photo = await _photoService.GetImageByIdAsync(id);

            if (photo == null)
            {
                return NotFound();
            }
            return View(photo);
        }

        // POST: Photos/Edit/5
        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Photo photo)
        {
            if (id != photo.Id)
                return RedirectToAction("Error", "Home");
            if (!ModelState.IsValid)
            {
                try
                {
                    await _photoService.EditPhotoAsync(photo);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _photoService.PhotoExistsAsync(photo.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction("Index", "Home");
            }
            var errors = (from state in ModelState
                from error in state.Value.Errors
                select error.ErrorMessage).ToList();
            // Wyświetl błędy w konsoli (lub zrób coś innego z błędami)
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            return View(photo);
        }

        // GET: Photos/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var photo = await _photoService.GetImageByIdAsync((int)id);
            if (photo == null)
                return NotFound();
            return View(photo);
        }

        // POST: Photos/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var photo = await _photoService.GetImageByIdAsync(id);
            if (photo == null)
                return RedirectToAction("Error", "Home");
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "image", photo.Name);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
            return (await _photoService.Delete(photo)) ? RedirectToAction(nameof(Index)) : RedirectToAction("Error","Home");
        }
        

        [HttpPost]
        [Authorize]
        public async Task<bool> LikeOrUnlikePhoto(int idPhoto)
        {
            //var idPhoto = int.Parse(idPhotoString);            
            var user = _userService.GetCurrentLoggedInUser(User);
            if (user == null)
                return false;
            var likedPhotos = await _photoService.GetLikedPhotos(user);
            return (!_photoService.IsPhotoLiked(idPhoto, likedPhotos))
                ? await _photoService.LikePhoto(idPhoto, user)
                : _photoService.UnlikePhoto(idPhoto, user);
        }
    }
}
