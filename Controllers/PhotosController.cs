﻿using Luxa.Data;
using Luxa.Interfaces;
using Luxa.Models;
using Luxa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using AspNetCore;

namespace Luxa.Controllers
{
    public class PhotosController : Controller
    {
        private readonly IPhotoService _photoService;

        private readonly ApplicationDbContext _context; //wstrzykiwanie kontekstu

        private readonly UserManager<UserModel> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;

       
        public PhotosController(IUserService userService, ApplicationDbContext context, UserManager<UserModel> userManager, IWebHostEnvironment hostEnvironment, IPhotoService photoService, ICommentService commentService)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
            _photoService = photoService;
            _userService = userService;
            _commentService = commentService;
        }

        [Authorize(Roles = "admin,moderator")]
        public async Task<IActionResult> Index()
            //Nie powinno się z tego co wiem przekazywać UserModela do widoku ale inaczej bez tworzenia ViewModelu
            //Bez Include się nie wyświetla ale trzeba to przerobić albo na vm albo pobawić się ViewBagami
            => View(await _context.Photo.Include(m => m.Owner).ToListAsync());


        public IActionResult DownloadImage(int id)
        {
            var photo = _photoService.GetImageById(id);
            string filePath = Path.Combine(_hostEnvironment.WebRootPath, "Image/" + photo.Name);
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            if (photo == null)
            {
                return null;
            }
            string extension = Path.GetExtension(photo.Name);
            string contentType = GetContentType(extension);
            return File(fileBytes, contentType, photo.Name);
        }
        private string GetContentType(string extension)
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

        /*public async Task<IActionResult> Index()
        {
            var photos = await _photoService.GetAllImagesAsync();
            return View(photos);
        }*/




        // GET: Photos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photo
                .Include(m => m.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (photo == null)
            {
                return NotFound();
            }
            // Pobierz komentarze dla tego zdjęcia
            var comments = await _commentService.GetCommentsForPhoto(id);
            // Przekaż komentarze do widoku
            ViewData["Comments"] = comments; 

            // Przekazanie id zdjęcia do widoku
            ViewBag.PhotoId = id;


            return View(photo);
        }

        // GET: Photos/Create
        public IActionResult Create()
        {

            if (_userManager.GetUserId(User) == null)
            {
                return View("Views/Home/Index.cshtml");
            }

            return View();
            //filtr privacy
        }

        // POST: Photos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Category,AddTime,ImageFile")] Photo photo, string Tags)
        {
            var user = _userService.GetCurrentLoggedInUser(User);
            if (user == null)
                return Unauthorized();
            await _photoService.Create(photo, user, Tags);
            return RedirectToAction(nameof(Index));
            //return View(photo);
        }


        // GET: Photos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _photoService.GetImageByIdAsync(id);
                
            if (photo == null)
            {
                return NotFound();
            }
            return View(photo);
        }

        // POST: Photos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OwnerId,Name,Description,Category,Views,LikeCount,ImageFile")] Photo photo)
        {
            if (id != photo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
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
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = new List<string>();

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }

                // Wyświetl błędy w konsoli (lub zrób coś innego z błędami)
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }
            return View(photo);
        }

        // GET: Photos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (photo == null)
            {
                return NotFound();
            }

            return View(photo);
        }

        // POST: Photos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var photo = await _context.Photo.FindAsync(id);
            //delete image from wwwroot/image
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "image", photo.Name);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            //delete record
            _context.Photo.Remove(photo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhotoExists(int id)
        {
            return _context.Photo.Any(e => e.Id == id);
        }


        [HttpPost]
        public async Task<bool> LikeOrUnlikePhoto(int idPhoto)
        {
            //var idPhoto = int.Parse(idPhotoString);            
            var user = _userService.GetCurrentLoggedInUser(User);
            if (user == null)
                throw new NotImplementedException();
            var likedPhotos = await _photoService.GetLikedPhotos(user);
            return (!_photoService.IsPhotoLiked(idPhoto, likedPhotos))
                ? _photoService.LikePhoto(idPhoto, user)
                : _photoService.UnlikePhoto(idPhoto, user);
        }



        /*
        public IActionResult DownloadFile(int id, [Bind("Id,Owner,Name,Description,AddTime,ImageFile")] Photo photo)
        {
            *//*            var photo = _context.Photo.FindAsync(id).FirstOrDefaultAsync(m => m.Id == id);
            *//*
            //var filename = Path.Combine(_hostEnvironment.WebRootPath, "image", photo.Name);
            var fileName = photo.ImageFile.FileName;
            var memory = DownloadSingleFile(fileName, "wwwroot\\Image");
            return File(memory.ToArray(), "image/png", photo.ImageFile.FileName);
        }
        private MemoryStream DownloadSingleFile(string filename, string uploadPath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), uploadPath, filename);
            var memory = new MemoryStream();
            if (System.IO.File.Exists(path))
            {
                var net = new System.Net.WebClient();
                var data = net.DownloadData(path);
                var content = new System.IO.MemoryStream(data);
                memory = content;

            }
            memory.Position = 0;
            return memory;
        }*/
    }

}
