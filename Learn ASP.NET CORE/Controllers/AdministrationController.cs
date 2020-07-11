using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Learn_ASP.NET_CORE.Models;
using Learn_ASP.NET_CORE.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Learn_ASP.NET_CORE.Controllers
{
    // Authorize attribute bisa digunakan di action level dan controller level,
    // jika kita gunakan di controller level itu artinya hanya bisa diakses oleh member dari role tersebut
    // dan cara menoverridenya kita bisa gunakan attribute Authorize lagi di action method dari controller itu
    // jika suatu action ingin di akses oleh semuanya termasuk anonymous user, seperti orang yang datang ke website kita
    // hanya melihat saja. kita bisa gunakan attribute [AllowAnonymous]

    // Hanya bisa diakses oleh member yang memiliki role admin
    //[Authorize(Roles = "Admin")]

    // Hanya bisa diakses oleh member yang memiliki salah satu role, bisa itu admin atau user
    //[Authorize(Roles = "Admin,User")]

    // Hanya bisa diakses oleh member yang memiliki kedua role, yaitu admin dan user
    //[Authorize(Policy = "AdminRolePolicy")]
    public class AdministrationController : Controller
    {
        // roleManager, digunakan untuk kita membuat role. seperti admin dan user etc
        private readonly RoleManager<IdentityRole> roleManager;
        // kita melakukan dependency injection ke UserManager dengan generic type ke ApplicationUser kita
        // untuk mendapatkan semua user yang ada di aplikasi kita
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
                                        UserManager<ApplicationUser> userManager,
                                        ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        public IActionResult ListRoles()
        {
            // untuk mendapatkan semua role, kita bisa menggunakan Roles properti dari roleManager service class
            // yang nantinya akan me-return IEnumerable<IdentityRole>
            var allRoles = roleManager.Roles;
            return View(allRoles);
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            // mendapatkan semua user, dengan properti Users dari userManager service
            var allUser = userManager.Users;
            return View(allUser);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            // cek apakah model telah valid
            if (ModelState.IsValid)
            {
                // buat role, dari built in class yaitu IdentityRole
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };
                // kita create lewar roleManager
                IdentityResult result = await roleManager.CreateAsync(identityRole);
                // cek apakah berhasil di buat
                if (result.Succeeded)
                {
                    // jika berhasil, alihkan ke list semua role
                    return RedirectToAction("ListRoles", "Administration");
                }
                // loop setiap error
                foreach (IdentityError error in result.Errors)
                {
                    // lalu tambahkan ke ModelState
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            // cari role berdasarkan id. by default tipe id dari IdentityRole adalah string
            var findedRole = await roleManager.FindByIdAsync(id);
            // cek apakah role yang sudah ketemu ini null (kosong)
            if (findedRole == null)
            {
                // jika kosong maka kita mengembalikan NotFound error page
                ViewBag.ErrorMessage = $"Role dengan id = {id} tidak ditemukan";
                return View("NotFound");
            }
            // jika ada, kita simpan ke EditRoleViewModel id beserta rolenamenya
            var model = new EditRoleViewModel
            {
                Id = findedRole.Id,
                RoleName = findedRole.Name
            };
            // kita loop user dari semua user di usermanager instance
            // properti Users, untuk mendapatkan semua user yang ada
            foreach (var users in userManager.Users.ToList())
            {
                // cek apakah user berada di role yang kita cari
                if (await userManager.IsInRoleAsync(users, model.RoleName))
                {
                    // jika ada maka kita tambahkan ke Users properti di model variabel
                    // model sudah menyiapkan Users properti yang bertipe List<string>
                    model.Users.Add(users.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            // cari role berdasarkan id, disini kita gunakan id dari EditRoleViewModel
            var role = await roleManager.FindByIdAsync(model.Id);
            // jika null (kosong) maka return NotFound view dengan error message
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role dengan id = {role.Id} tidak ditemukan";
                return View("NotFound");
            }
            else
            {
                // jika tidak, kita update properti name dari model.Rolename
                role.Name = model.RoleName;
                // setelah itu kita update
                var updatedRole = await roleManager.UpdateAsync(role);
                // cek apakah updatenya berhasil
                if (updatedRole.Succeeded)
                {
                    // jika ya, maka alihkan ke list dari semua role
                    return RedirectToAction("ListRoles");
                }
                // loop error, yang kita dapatkan dari properti Errors
                foreach (var error in updatedRole.Errors)
                {
                    // tambahkan setiap iterasi error ke ModelState
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            // ambil roleId yang ada di querystring
            ViewBag.roleId = roleId;
            // cari role berdasarkan id
            var role = await roleManager.FindByIdAsync(roleId);
            // cek apakah role null (kosong)
            if (role == null)
            {
                // jika kosong, maka return NotFound view dan error message
                ViewBag.ErrorMessage = $"Role with id {role.Id} cannot be found";
                return View("NotFound");
            }
            // jika role tidak kosong, itu artinya ada. kita pun membuat list dari UserRoleViewMode
            var model = new List<UserRoleViewModel>();
            // lakukan perulangan ke semua user yang ada di userManager instance
            foreach (var user in userManager.Users)
            {
                // buat variabel dari class UserRoleViewModel, dan masukkan value dari user ke dalam
                // masing-masing properti
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    Username = user.UserName
                };
                // cek apakah user berada dalam role yang tadi kita cari berdasarkan id
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    // jika ya, maka properti IsSelected dari variabel yang kita buat bernilai true
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    // jika tidak, maka properti IsSelected dari variabel yang kita buat bernilai false
                    userRoleViewModel.IsSelected = false;
                }
                // tambahkan setiap user yang di loop, ke dalam variabel model. dimana ia menyimpan
                // list dari UserRoleViewModel
                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            // cari role berdasarkan id, yang kita dapaktan dari query string roleid
            var findRole = await roleManager.FindByIdAsync(roleId);
            // cek apakah bernilai null
            if (findRole == null)
            {
                // jika ya, maka return NotFound
                ViewBag.ErrorMessage = $"Role with id {roleId} cannot be found";
                return View("NotFound");
            }
            // lakukan perulangan terhadap model, dimana model ini bersikan list dari UserRoleViewModel
            for (int i = 0; i < model.Count; i++)
            {
                // cari single user berdasarkan id
                var user = await userManager.FindByIdAsync(model[i].UserId);
                // buar variabel dari IdentityResult untuk menyimpan hasil user
                IdentityResult result = null;

                // cek apakah properti IsSelected di user di select atau di pilih (true), dan user tersebut tidak ada di dalam role
                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, findRole.Name)))
                {
                    // jika ya, maka tambahkan user tersebut ke role itu
                    result = await userManager.AddToRoleAsync(user, findRole.Name);
                }
                // cek apakah properti isSelected di user tidak di select atau di pilih (false), dan user tersebut ada di dalam role
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, findRole.Name))
                {
                    // jika ya, maka hapus user tersebut dari role itu
                    result = await userManager.RemoveFromRoleAsync(user, findRole.Name);
                }
                else
                {
                    // statement continue ini, untuk kondisi dimana user ada di dalam role dan kita tidak unselect user dari role maka kita tidak ingin melakukan apa-apa jadi kita continue ke user berikutya
                    // juga untuk kondisi dimana user tidak ada di dalam role, dan kita tidak melakukan select user dari role maka kita tidak melakukan apa-apa jadi kita continue ke user berikutnya
                    continue;
                }
                // jika result yang berisikan user-user dari sukses
                if (result.Succeeded)
                {
                    // maka kita cek apakah variabel i di dalam loop lebih kecil dari jumlah list dari user roleviewmodel
                    // dikurangi 1
                    if (i < (model.Count - 1))
                        // jika ya, maka masih ada user. dan kita lakukan lagi perulangan
                        continue;
                    else
                        // jika tidak, maka sudah selesai looping dan kita alihkan ke EditRole view dan id
                        // karena EditRole menerima parameter id
                        return RedirectToAction("EditRole", new { id = roleId });
                }
            }
            return RedirectToAction("EditRole", new { id = roleId });
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            // cari single user berdasarkan id
            var user = await userManager.FindByIdAsync(id);
            // cek apakah user ada atau tidak
            if (user == null)
            {
                // jika null (tidak ada) maka return NotFound view
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }

            // cari semua role yang ada di user
            var userRoles = await userManager.GetRolesAsync(user);
            // cari semua claims yang ada di user
            var userClaims = await userManager.GetClaimsAsync(user);

            // input masing-masing value dari user ke EditUserViewModel
            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                City = user.City,
                Claims = userClaims.Select(c => c.Type + " : " + c.Value).ToList(),
                Roles = userRoles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            // cari single user berdasarkan id
            var user = await userManager.FindByIdAsync(model.Id);
            // cek apakah user null 
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                // update properti dengan data dari model
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.City = model.City;
                // update user
                var updatedUser = await userManager.UpdateAsync(user);
                // cek jika user berhasil di update
                if (updatedUser.Succeeded)
                {
                    // jika berhasil, alihkan user ke ListUsers
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in updatedUser.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {user.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                // userManager memiliki DeleteAsync() method untuk menghapus user
                var resultDelete = await userManager.DeleteAsync(user);

                if (resultDelete.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                foreach (var error in resultDelete.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("ListUsers");
            }
        }

        [HttpPost]
        // Kita gunakan Policy properti untuk memberikan nama dari Policy yang kita daftarkan di service
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                try
                {
                    var deletedResult = await roleManager.DeleteAsync(role);

                    if (deletedResult.Succeeded)
                    {
                        return RedirectToAction("ListRoles");
                    }
                    foreach (var error in deletedResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("ListRoles");
                }
                catch (DbUpdateException e)
                {

                    logger.LogError($"Error deleting role {e}");

                    ViewBag.ErrorTitle = $"{role.Name} role is in use";
                    ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users in this role. If you want to delete this role, please remove the users from the role and then try to delete";
                    return View("Error");
                }
            }
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRolesViewModel>();

            foreach (var role in roleManager.Roles.ToList())
            {
                var userRoleViewModel = new UserRolesViewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]

        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            // dapatkan semua roles dari variabel user tadi
            var roles = await userManager.GetRolesAsync(user);
            // hapus semua role yang ada user tersebut
            var result = await userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            // tambahkan role jika user tersebut memiliki properti isSelected bernilai true
            // lalu pilih rolename properti untuk menampilkan ke view
            result = await userManager.AddToRolesAsync(user,
                model.Where(e => e.IsSelected).Select(x => x.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel
            {
                UserId = userId
            };

            foreach (Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim()
                {
                    ClaimType = claim.Type
                };
                // jika user sudah memiliki claim, kita buat isSelected properti menjadi true. jadi nanti
                // di tampilan UI akan tertampilkan di checked
                if (existingUserClaims.Any(e => e.Type == claim.Type && e.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }

            // dapatkan semua claim yang dimiliki user
            var claims = await userManager.GetClaimsAsync(user);
            // lalu hapus semua claimnya
            var result = await userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }
            // tambahkan claim yang properti IsSelected bernilai true ke UI
            result = await userManager.AddClaimsAsync(user,
                    model.Claims.Select(c => new Claim(c.ClaimType,
                                c.IsSelected ? "true" : "false")));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = model.UserId });
        }

        [AllowAnonymous]
        public ViewResult AccessDenied()
        {
            return View();
        }

    }
}
