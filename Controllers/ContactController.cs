using Luxa.Interfaces;
using Luxa.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luxa.Controllers
{
    public class ContactController(IContactService contactService, IUserService userService) : Controller
    {
        private readonly IContactService _contactService = contactService;
        private readonly IUserService _userService = userService;

        [Authorize]
		public IActionResult UserContact()
        {
            return View();
        }
        [HttpPost]
		[Authorize]
		public IActionResult UserContact(ContactVM contactVM)
        {
            var user = _userService.GetCurrentLoggedInUser(User);
            var category = _contactService.GetEnumCategory(contactVM.Category);
            _contactService.CreateContact(ModelState.IsValid, user, category, contactVM.Description, contactVM.DetailedCategory);
            return View();
        }
        [HttpGet]
		[Authorize]
		public IActionResult GetDetailedCategory(string selectedValue)
        {
            var select = _contactService.GetTextAndValueToSelect(selectedValue);
            return Json(new { text = select.Item1, value = select.Item2 });
        }
		[Authorize(Roles = "admin,moderator")]
		public async Task<IActionResult> ContactList()
        {
            ViewBag.CategorySelectItems = _contactService.GetCategorySelectItems();
            ViewBag.DetailedCategorySelectItems = _contactService.GetDetailedCategorySelectItems();
            ViewBag.StateSelectItems = _contactService.GetStateSelectItems(true);
            ViewBag.StateSelectChangeItems = _contactService.GetStateSelectItems(false);
            return View(await _contactService.ShowContacts());
        }
		[Authorize(Roles = "admin,moderator")]
		[HttpPost]
        public bool EditState(string data) =>
            _contactService.PrepareToUpdateState(data).Result;
    }
}