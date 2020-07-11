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
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Learn_ASP.NET_CORE.Controllers
{
    public class AccountController : Controller
    {
        // UserManager class, digunakan untuk kita membuat user, hapus user, dan update user
        // karena memiliki beberapa method yang berhubungan dengan itu
        private readonly UserManager<ApplicationUser> userManager;
        // SignInManager class, berguna untuk urusan login, logout, dan mengecek apakah sudah sudah login
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                                ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {

            if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
            {
                return View();

            }

            if (signInManager.IsSignedIn(User) && !User.IsInRole("Admin"))
            {
                return RedirectToAction("index", "home");
            }

            return View();

        }

        // Kita membuat IsEmailSudahDigunakan() untuk mengecek apakah sebuah email sudah di gunakan oleh user apa tidak
        // ini akan dijalankan di client side, karena kita melakukan remote validation
        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailSudahDigunakan(string email)
        {
            // cari user email 
            var user = await userManager.FindByEmailAsync(email);

            //cek apakah user email null (kosong) atau tidak
            if (user == null)
            {
                // jika null (kosong), itu artinya email tersebut belum digunakan, maka kita return json true
                // alasan mengapa kita me-return json karena kita menggunakan AJAX
                return Json(true);
            }
            else
            {
                // jika user tidak null, maka berarti user dengan email tersebut sudah ada,
                // dan kita me-return error message
                return Json($"Email {email} Sudah Digunakan");
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // masukkan data user ke class IdentityUser. dimana datanya dari RegisterViewModel yang sudah di binding
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    City = model.City
                };
                // buat user dengan method CreateAsync() dari userManager instance. parameter pertama yaitu identity user
                // parameter kedua adalah password dari user tersebut (password sudah di hash)
                var createdUser = await userManager.CreateAsync(user, model.Password);

                if (createdUser.Succeeded) // cek jika user berhasil dibuat
                {
                    // Generate token, untuk konfirmasi email
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    // membuat link konfirmasi email
                    var confirmationEmailLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, confirmationEmailLink);

                    // jika user login dan user itu adalah role admin, maka alihkan ke List User view
                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }
                    // jika user berhaisl dibuat kita, artinya user berhasil signIng
                    // disini properti isPersistent, untuk mengetahui apakah kita ingin membuat session cookie atau permanent cookie
                    // await signInManager.SignInAsync(user, isPersistent: false);
                    // setelah berhasil signIn, user lalu di alihkan ke Home page
                    // return RedirectToAction("index", "home");
                    ViewBag.ErrorTitle = "Registration successful";
                    ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                            "email, by clicking on the confirmation link we have emailed you";
                    return View("Error");
                }

                // jika saat membuat user gagal, kita ingin mendapatkan error yang terjadi
                // kita bisa loop ke semua errornya dan kita tambahkah ke ModelState
                foreach (var err in createdUser.Errors)
                {
                    // Menambahkan error dengan method AddModelError()
                    ModelState.AddModelError("", err.Description);
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            // jika salah satunya bernilai null, maka alihkan ke index (home controller)
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "E-Mail cannot be confirmed";
            return View("Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("index", "home");
            }

            LoginViewModel model = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins =
                // method GetExternalAuthenticationSchemesAsync(), mengembalikan list dari configure external
                // login provider
                (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {

            model.ExternalLogins =
                (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // parameter returnUrl, adalah ketika kita ke suatu route yang membutuhkan
            // authenticated, maka returnUrl ini nantinya berisikan route tersebut, jadi setelah kita login. kita akan langsung di alihkan ke returnUrl ini

            if (ModelState.IsValid)
            {

                var user = await userManager.FindByEmailAsync(model.Email);
                // cek jika username dan passowrd correct, tetapi email belum terkonfirmasi
                if (user != null && !user.EmailConfirmed &&
                    (await userManager.CheckPasswordAsync(user, model.Password)))
                {
                    // throw error message
                    ModelState.AddModelError(string.Empty, "E-Mail belum terkonfirmasi!");
                    return View(model);
                }

                // argument ke 4, dari PasswordSignInAsync() method, adalah lockoutOnFailure, jadi ketika
                // user sudah gagal mencoba login sebanyak maksimal percobaan yang di tentukan. maka akun akan
                // di lockouk. jika true, maka akan dilockout. jika false, maka tidak. tabel AccessFailedCount kolom akan di increment 1 kali,
                // dan jika sudah sampai batas maksimal yang sudah ditentukan maka akun akan di lockout
                var signInUser = await signInManager.
                    PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

                if (signInUser.Succeeded)
                {
                    // cek apakah urlnya adalah local url, ini berguna untuk mencegah user di alihkan ke login page attacker
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        // jika bukan local url, maka user akan di alihkan ke home page
                        return RedirectToAction("index", "home");
                    }
                }

                if (signInUser.IsLockedOut)
                {
                    return View("AccountLocked");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("login");
        }

        [AllowAnonymous]
        public ViewResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            // parameter provider berisikan nama providernya, karena kita hanya me-register satu provider
            // yaitu google jadi valuenya adalah Google
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                              new { ReturnUrl = returnUrl });

            var properties = signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // ChallengeResult, me-return external provider sign-in page, dimana sign-in pagenya adalah Google

            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            // jika returnUrl bernilai null, kita berikan value "/". returnUrl, adalah ketika kita ke suatu route yang membutuhkan
            // authenticated, maka returnUrl ini nantinya berisikan route tersebut, jadi setelah kita login. kita akan langsung di alihkan ke returnUrl ini
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                // method GetExternalAuthenticationSchemesAsync(), mengembalikan list dari configure external
                // login provider
                ExternalLogins =
                (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            // jika remoteError tidak null, itu artinya kita mendapatkan informasi error dari external login provider
            if (remoteError != null)
            {
                // lalu kita tambahkan ke modelstate addmodelerror method
                ModelState.AddModelError(string.Empty, $"Error from external provider : {remoteError}");
                // dan user kita alihkan kembali ke halaman login
                return View("Login", loginViewModel);
            }

            // kita dapatkan user info dari external login provider
            var externalLoginProviderUserInfo = await signInManager.GetExternalLoginInfoAsync();
            // jika externalLoginProviderUserInfo null, maka terjadi error. dimana kita gagal mendapatkan user info
            if (externalLoginProviderUserInfo == null)
            {
                ModelState
                .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }

            // cari dan simpan email user yang diterima dari google
            var userEmailExternalProvider =
                externalLoginProviderUserInfo.Principal.FindFirstValue(ClaimTypes.Email);

            ApplicationUser user = null;

            if (userEmailExternalProvider != null)
            {
                user = await userManager.FindByEmailAsync(userEmailExternalProvider);
                // cek user ada, tetapi emailnya belum di konfirmasi
                if (user != null && !user.EmailConfirmed)
                {
                    // maka alihkan kembali ke login view, dan tampilkan error message
                    ModelState.AddModelError(string.Empty, "E-Mail belum terkonfirmasi!");
                    return View("Login", loginViewModel);
                }
            }

            // jika externalLoginProviderUserInfo tidak null, kita login dengan ExternalLoginSignInAsync method dari signInManager
            var signInResultExternalProvider = await signInManager.ExternalLoginSignInAsync
                                               (externalLoginProviderUserInfo.LoginProvider,
                                               externalLoginProviderUserInfo.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            // jika sukses login, (ini hanya akan berhasil jika externalUser ini memiliki record data di AspNetUserLogins)
            if (signInResultExternalProvider.Succeeded)
            {
                // lalu kita alihkan ke returnUrl, dimana adalah aplikasi kita
                return LocalRedirect(returnUrl);
            }
            else
            {
                if (userEmailExternalProvider != null)
                {
                    // jika user tidak ditemukan
                    if (user == null)
                    {
                        // kita membuat local account user baru
                        user = new ApplicationUser
                        {
                            UserName = externalLoginProviderUserInfo.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = externalLoginProviderUserInfo.Principal.FindFirstValue(ClaimTypes.Email)
                        };
                        // dengan bantuan dari CreateAsync() method
                        await userManager.CreateAsync(user);

                        // untuk generate email confirmation token, gunakan GenerateEmailConfirmationTokenAsync() method
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                        // buat confirmation link
                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                                          new { userId = user.Id, token = token }, Request.Scheme);

                        // simpan link confirmation email ke dalam logger
                        logger.Log(LogLevel.Warning, confirmationLink);

                        ViewBag.ErrorTitle = "Register Berhasil";
                        ViewBag.ErrorMessage = "Sebelum login, Tolong konfirmasi email anda, dengan click link yang kita kirimkan ke email anda";
                        return View("Error");
                    }
                    // setelah itu tambahkan ke AspNetUserLogins
                    await userManager.AddLoginAsync(user, externalLoginProviderUserInfo);
                    // lalu user bisa signIn
                    await signInManager.SignInAsync(user, isPersistent: false);
                    // setelah berhasi signIn, alihkan ke returnUrl
                    return LocalRedirect(returnUrl);
                }
                // If we cannot find the user email we cannot continue
                ViewBag.ErrorTitle = $"Email claim not received from: {externalLoginProviderUserInfo.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on aji@handsome.com";

                return View("Error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null && await userManager.IsEmailConfirmedAsync(user))
                {
                    // Untuk generate reset password token, kita gunakan GeneratePasswordResetTokenAsync() method
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);

                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                                            new { email = model.Email, token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, passwordResetLink);
                    return View("ForgotPasswordConfirmation");
                }
                return View("ForgotPasswordConfirmation");
            }
            return View(model);

        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            // If password reset token or email is null, most likely the
            // user tried to tamper the password reset link
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // reset the user password
                    var result =
                        await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        // jia usernya sedang di lockout akun
                        if(await userManager.IsLockedOutAsync(user))
                        {
                            // kita ubah tanggal lockout berakhirnya menjadi tanggal saat ini
                            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        }

                        return View("ResetPasswordConfirmation");
                    }
                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User);
            // cari user yang sudah memiliki password
            var userHasPassword = await userManager.HasPasswordAsync(user);
            // jika user tidak memiliki passoword
            if (!userHasPassword)
            {
                // alihkan ke AddPassword view
                return RedirectToAction("AddPassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                if (user == null)
                {
                    return RedirectToAction("Login");
                }
                // ganti password
                var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                // jika password yang baru tidak memenuhi syarat
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }
                // refresh sign-in cookie
                await signInManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmationView");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await userManager.GetUserAsync(User);
            
            // cari user yang sudah memiliki password
            var userHasPassword = await userManager.HasPasswordAsync(user);
            // jika user sudah memiliki passoword
            if (userHasPassword)
            {
                // alihkan ke ChangePassword view
                return RedirectToAction("ChangePassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                var result = await userManager.AddPasswordAsync(user, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await signInManager.RefreshSignInAsync(user);
                return View("AddPasswordConfirmation");

            }
            return View(model);
        }
    }
}
