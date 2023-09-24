using AllUp.DAL;
using AllUp.Helpers;
using AllUp.Models;
using AllUp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController:Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        public UsersController(AppDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            List<AppUser> appUsers = await _db.Users.ToListAsync();
            List<UserVM> userVMs = new List<UserVM>();
            foreach (AppUser appUser in appUsers)
            {
                UserVM userVM = new UserVM
                {
                    Id = appUser.Id,
                    Name = appUser.Name,
                    Surname = appUser.Surname,
                    Email = appUser.Email,
                    UserName = appUser.UserName,
                    IsDeactive = appUser.IsDeactive,
                    Role = (await _userManager.GetRolesAsync(appUser)).FirstOrDefault()
                };
                userVMs.Add(userVM);
            }
            return View(userVMs);
        }

        #region Create
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string>
            {
                Helper.Admin,
                Helper.Member
            };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterVM registerVM, string role)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            ViewBag.Roles = new List<string>
            {
                Helper.Admin,
                Helper.Member
            };

            AppUser appUser = new AppUser
            {
                Name = registerVM.Name,
                Surname = registerVM.Surname,
                UserName = registerVM.Username,
                Email = registerVM.Email
            };

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, password: registerVM.Password);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, role);
            return RedirectToAction("Index");
        }
        #endregion




        #region Delete

        public async Task<IActionResult> Delete(string id)
        {
            AppUser userToDelete = await _userManager.FindByIdAsync(id);
            if (userToDelete == null)
            {
                return NotFound();
            }

            UserVM userVM = new()
            {
                Id = userToDelete.Id,
                UserName = userToDelete.UserName,
                Email = userToDelete.Email
            };
            return View(userVM);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            AppUser userToDelete = await _userManager.FindByIdAsync(id);
            if (userToDelete == null)
            {
                return NotFound();
            }

            IdentityResult result = await _userManager.DeleteAsync(userToDelete);

            if (!result.Succeeded)
            {
                // Обработка ошибок при удалении, если необходимо
                return View("Delete", new UserVM
                {
                    Id = userToDelete.Id,
                    UserName = userToDelete.UserName,
                    Email = userToDelete.Email
                });
            }

            return RedirectToAction("Index");
        }

        #endregion



        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
