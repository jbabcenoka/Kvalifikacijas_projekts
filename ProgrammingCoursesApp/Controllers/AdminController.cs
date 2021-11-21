using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgrammingCoursesApp.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            return View("UsersAdministration", users);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            IdentityUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";

            var vm = new UserEditVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                OldIdentityRole = userRole,
                IdentityRole = userRole
            };

            return View(vm);
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(UserEditVM editedUser)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await _userManager.FindByIdAsync(editedUser.Id);
                if (user != null)
                {
                    user.UserName = editedUser.UserName;
                    user.Email = editedUser.Email;
                    user.PhoneNumber = editedUser.PhoneNumber;

                    if (editedUser.OldIdentityRole != editedUser.IdentityRole)
                    {
                        await _userManager.RemoveFromRoleAsync(user, editedUser.OldIdentityRole);
                        await _userManager.AddToRoleAsync(user, editedUser.IdentityRole);
                    }

                    var userUpdate = await _userManager.UpdateAsync(user);
                    if (userUpdate.Succeeded)
                    {
                        return RedirectToAction(nameof(AdminController.Index));
                    }
                    else
                    {
                        foreach (var error in userUpdate.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            return View(editedUser);
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            IdentityUser user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(AdminController.Index));
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
}
