using AutoMapper;
using Luxa.Data;
using Luxa.Data.Enums;
using Luxa.Interfaces;
using Luxa.Models;
using Luxa.Services;
using Luxa.ViewModel;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace Luxa.Controllers
{
    public class AccountController(SignInManager<UserModel> signInManager,
                                   UserManager<UserModel> userManager,
                                   NotificationService notificationService,
                                   IUserService userService,
                                   IMapper mapper,
                                   IServiceScopeFactory scopeFactory,
                                   IFollowService followService) : Controller
    {
        private readonly SignInManager<UserModel> _signInManager = signInManager;
        private readonly UserManager<UserModel> _userManager = userManager;
        private readonly NotificationService _notificationService = notificationService;
        private readonly IUserService _userService = userService;
        private readonly IMapper _mapper = mapper;
        private readonly IServiceScopeFactory _scopeFactory= scopeFactory;
        private readonly IFollowService _followService = followService;

        //Atrybut do routingu (reszta kodu w program.cs)
        [Route("signin", Name = "SignIn")]
        [HttpGet]
        public IActionResult SignIn()
        {
            var model = new SignInVM();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInVM signInVM)
        {
            if (!ModelState.IsValid
                /*|| string.IsNullOrEmpty(signInVM.UserName)
                || string.IsNullOrEmpty(signInVM.Password)*/
                )
            {
                ModelState.AddModelError("", "Niepoprawna próba logowania");
                return View(signInVM);
            }
            var result = await _signInManager.PasswordSignInAsync(signInVM.UserName!, signInVM.Password!, signInVM.RememberMe, false);
            if (result.Succeeded)
            {
                var user = _userService.GetCurrentLoggedInUserWithPhotos(User);
                if (user != null)
                {
                    await _userService.UpdateReputation(user);
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Niepoprawna próba logowania");
            return View(signInVM);
        }

        //google
        [HttpGet]
        public IActionResult SignInGoogle()
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse), "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(SignIn));
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = new UserModel
            {
                UserName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                Email = info.Principal.FindFirstValue(ClaimTypes.Email)
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return RedirectToAction(nameof(SignIn));
            }

            await _userManager.AddLoginAsync(user, info);
            var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Regular);
            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("SignIn");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("signup", Name = "SignUp")]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpVM signUpVM)
        {
            if (!ModelState.IsValid
                //|| string.IsNullOrEmpty(signUpVM.UserName)
                //|| string.IsNullOrEmpty(signUpVM.Email)
                )
            {
                return View(signUpVM);
            }

            UserModel user = new()
            {
                UserName = signUpVM.UserName,
                Email = signUpVM.Email,
            };
            var resultAddUser = await _userManager.CreateAsync(user, signUpVM.Password!);
            if (resultAddUser.Errors.Any())
            {
                foreach (var error in resultAddUser.Errors)
                {
                    switch (error.Code)
                    {
                        case "PasswordTooShort":
                            ModelState.AddModelError("", "Hasło musi zawierać minimum 6 znaków");
                            break;
                        case "DuplicateUserName":
                            ModelState.AddModelError("", "Użytkownik o danej nazwie użytkownika już istnieje, wybierz inną nazwę");
                            break;
                        default:
                            ModelState.AddModelError("", $"Nastąpił błąd {error.Code}, spróbuj ponownie");
                            break;
                    }
                }
                return View(signUpVM);
            }
            var resultAddRole = await _userManager.AddToRoleAsync(user, UserRoles.Regular);
            foreach (var error in resultAddRole.Errors)
            {
                ModelState.AddModelError("", $"Nastąpił błąd {error.Code}, spróbuj ponownie");
            }
            if (resultAddRole.Errors.Any())
            {
                return View(signUpVM);
            }
            if (resultAddUser.Succeeded && resultAddRole.Succeeded)
            {
                await _notificationService.AssignDefaultNotificationToNewAccount(user);
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }
            return View(signUpVM);
        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            if (HttpContext.Request.Cookies[".AspNetCore.Session"] != null)
            {
                Response.Cookies.Delete(".AspNetCore.Session");
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "admin,moderator")]
        public async Task<IActionResult> UsersList()
        {
            var usersListVM = new List<UserEntryVM>();
            List<UserModel> users = await _userService.GetAllUsers();
            var tasks = users.Select(async user =>
            {
                using var scope = _scopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
                var roles = await userManager.GetRolesAsync(user);
                //var notifications = await _notificationService.GetNotificationsForUser(user.Id);
                return new UserEntryVM
                {
                    User = user,
                    Roles = roles,
                };
            });
            usersListVM = [.. (await Task.WhenAll(tasks))];
            return View(usersListVM);
        }

        //W fazie rozwoju
        [Authorize(Roles = "admin,moderator")]
        public IActionResult CreateUser()
        {
            return View();
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserVM createUserVM)
        {
            if (!ModelState.IsValid)
            {
                TempData["errorMessage"] = "Wystąpił niezidentyfikowany błąd";
                return View();
            }
            var user = _mapper.Map<UserModel>(createUserVM);
            var resultAddUser = await _userManager.CreateAsync(user, createUserVM.Password!);
            var resultAddRole = await _userManager.AddToRoleAsync(user, createUserVM.Role);
            if (resultAddUser.Succeeded && resultAddRole.Succeeded)
            {
                TempData["successMessage"] = "Utworzono użytkownika";
                return RedirectToAction("UsersList");
            }
            foreach (var error in resultAddUser.Errors.Concat(resultAddRole.Errors))
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }
        [Authorize(Roles = "admin,moderator")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            await _userService.RemoveUserById(Id);
            return Ok();
        }

        [Authorize(Roles = "admin,moderator")]
        public async Task<IActionResult> EditUser(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
                return RedirectToAction("Error", "Home");
            var editUserVM = _mapper.Map<EditUserVM>(user, async opt =>
                opt.Items["Roles"] = await _userManager.GetRolesAsync(user)
            );
            return View(editUserVM);
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpPost]
        public async Task<IActionResult> EditUser(string Id, EditUserVM editUserVM)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            user.FirstName = editUserVM.FirstName;
            user.LastName = editUserVM.LastName;
            user.Country = editUserVM.Country;
            if (await _userService.SaveUser(user))
                return RedirectToAction("UsersList", "Account");
            else
                return View(editUserVM);

        }
        [Authorize]
        public IActionResult UserNotifications()
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if(userId==null)
            //	return RedirectToAction("SignIn");
            var user = _userService.GetCurrentLoggedInUser(User);
            if (user == null)
                return RedirectToAction("SignIn");
            var notifications = _notificationService.GetNotificationsForUser(user.Id);
            var userNotificationsVM = new UserNotificationsVM
            {
                User = user,
                Notifications = notifications
            };
            return View(userNotificationsVM);
        }
        [Authorize]
        [HttpPost]
        [Route("Account/UserNotifications/{notificationId}")]
        public async Task<IActionResult> UserNotifications(int notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return (userId!=null && await _notificationService.MarkNotificationAsViewed(userId, notificationId)) ? Ok() : NotFound();
        }
        [Authorize]
        public async Task<IActionResult> LoadMorePhotosToProfile(int pageNumber, int pageSize, string userName)
        {
            return ViewComponent("ProfilePhoto", new { pageNumber, pageSize, userName });
        }

        //profil avatar i tlo
        [Authorize]
        //[HttpGet]
        public async Task<IActionResult> UserProfile(string userName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("SignIn");
            var profileUser = await _userService.GetUserByUserName(userName);
            if (profileUser == null || profileUser.UserName == null)
                return NotFound();

            var avatarUrl = !string.IsNullOrEmpty(profileUser.AvatarUrl) ? profileUser.AvatarUrl : "/assets/blank-profile-picture.png";
            var backgroundUrl = !string.IsNullOrEmpty(profileUser.BackgroundUrl) ? profileUser.BackgroundUrl : "/assets/prostokat.png";
            var isFollowing = await _userService.IsFollowing(currentUser.Id, profileUser.Id);
            var followerCount = await _followService.GetFollowersCount(profileUser.Id);

            var model = new UserProfileVM
            {
                UserName = profileUser.UserName,
                AvatarUrl = avatarUrl,
                BackgroundUrl = backgroundUrl,
                Description = profileUser.Description,
                IsCurrentUserProfile = currentUser.UserName == userName,
                IsFollowing = isFollowing,
                FollowerCount = followerCount,
                PendingFollowRequests = currentUser.UserName == userName
                    ? await _userService.GetPendingFollowRequests(currentUser.Id)
                    : []
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("SignIn");
            if (await _userService.UpdateUserPhoto(currentUser, avatar, TypeOfProfilePhoto.avatar))
            return RedirectToAction("UserProfile", new { userName = currentUser.UserName });
            else
                return RedirectToAction("Error", "Home");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadBackground(IFormFile background)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("SignIn");
            if (await _userService.UpdateUserPhoto(currentUser, background, TypeOfProfilePhoto.background))
                return RedirectToAction("UserProfile", new { userName = currentUser.UserName });
            else
                return RedirectToAction("Error", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Follow(string userName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var followee = await _userService.GetUserByUserName(userName);

            if (followee == null || currentUser == null || currentUser.Id == followee.Id)
                return NotFound();

            var existingFollowRequest = await _followService.GetFollowModelByUserIds(currentUser.Id, followee.Id);

            if (existingFollowRequest == null)
            {
                var followRequest = new FollowModel
                {
                    FollowerId = currentUser.Id,
                    FolloweeId = followee.Id,
                    IsApproved = !followee.IsPrivate
                };

                if(!await _followService.Create(followRequest))
                    return RedirectToAction("Error", "Home");

                if (followee.IsPrivate)
                {
                    await _notificationService.SendFollowRequestNotification(followee, currentUser);
                    TempData["FollowMessage"] = "Prośba o obserwację została wysłana i oczekuje na akceptację.";
                    TempData["FollowStatus"] = "pending";
                }
                else
                {
                    TempData["FollowMessage"] = "Obserwujesz teraz użytkownika.";
                    TempData["FollowStatus"] = "approved";
                }
            }
            else if (!existingFollowRequest.IsApproved)
            {
                TempData["FollowMessage"] = "Twoja prośba o obserwację jest w trakcie rozpatrywania.";
                TempData["FollowStatus"] = "pending";
            }

            return RedirectToAction("UserProfile", new { userName });
        }

        [Authorize]
        public async Task<IActionResult> Unfollow(string userName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var followee = await _userService.GetUserByUserName(userName);

            if (followee == null || currentUser == null || currentUser.Id == followee.Id)
                return NotFound();

            var existingFollowRequest = await _followService.GetFollowModelByUserIds(currentUser.Id, followee.Id);

            if (existingFollowRequest == null)
                return RedirectToAction("Error", "Home");

            if(!await _followService.Delete(existingFollowRequest))
                return RedirectToAction("Error", "Home");

            TempData["FollowMessage"] = "Przestałeś obserwować użytkownika.";
            TempData["FollowStatus"] = "none";
            return RedirectToAction("UserProfile", new { userName });
        }
    }

}
