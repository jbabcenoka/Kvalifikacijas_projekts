using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgrammingCoursesApp.Data;
using ProgrammingCoursesApp.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCoursesApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        //Lietotāju saraksta attēlošana - AM-01
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            return View("UsersAdministration", users);
        }

        //Lietotāja tipa mainīšana - attēlot skatu
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
                IdentityRole = userRole
            };

            return View(vm);
        }

        //Lietotāja tipa mainīšana - AM-02
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(UserEditVM editedUser)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await _userManager.FindByIdAsync(editedUser.Id);
                if (user != null)
                {
                    var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";

                    if (userRole != editedUser.IdentityRole)
                    {
                        await _userManager.RemoveFromRoleAsync(user, userRole);
                        await _userManager.AddToRoleAsync(user, editedUser.IdentityRole);
                    }

                    await _userManager.UpdateAsync(user);
                    
                    return RedirectToAction(nameof(AdminController.Index));
                }
                else
                {
                    return NotFound();
                }
            }
            return View(editedUser);
        }

        //Lietotāja dzēšana - AM-03
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser([FromBody] string id)
        {
            IdentityUser user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                if (user.Id == _userManager.GetUserId(User))
                {
                    return Json(new { isDeleted = false });
                }

                //dzēst visus lietotāja rezultātus
                var results = await _context.Results.Where(x => x.User.Id == user.Id).ToListAsync();
                foreach (var res in results)
                {
                    _context.Results.Remove(res);
                }

                //dzēst kursus, kurus izveidoja lietotājs. Tiek dzēstas kursu tēmas un tēmu uzdevumi
                var courses = await _context.Courses.Where(x => x.User.Id == user.Id).ToListAsync();
                foreach (var course in courses)
                {
                    await CoursesController.DeleteCourse(course.Id, _context);
                }

                await _context.SaveChangesAsync();

                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                   return Json( new { isDeleted = true });
                }
            }

            return Json(new { isDeleted = false });
        }
        
    }
}
