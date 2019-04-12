using HelpDesk.BLL.Account;
using HelpDesk.BLL.Helpers;
using HelpDesk.DAL;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using EmailService = HelpDesk.BLL.Services.Senders.EmailService;

namespace HelpDesk.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly MembershipTools _membershipTools;
        private readonly MyContext _dbContext;

        public AccountController(MembershipTools membershipTools, IHostingEnvironment hostingEnvironment, MyContext dbContext) : base(membershipTools)
        {
            _membershipTools = membershipTools;
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;

            string[] roleNames = Enum.GetNames(typeof(IdentityRoles));
            foreach (string roleName in roleNames)
            {
                if (!_membershipTools.RoleManager.RoleExistsAsync(roleName).Result)
                {
                    ApplicationRole role = new ApplicationRole()
                    {
                        Name = roleName
                    };

                    IdentityResult task = _membershipTools.RoleManager.CreateAsync(role).Result;
                    Task.Run(() => task);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ApplicationUser user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new ProfilePasswordViewModel()
            {
                UserProfileViewModel = new UserProfileViewModel()
                {
                    Email = user.Email,
                    Id = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.Phone,
                    Surname = user.Surname,
                    UserName = user.UserName,
                    TechnicianStatus = user.TechnicianStatus,
                    AvatarPath = string.IsNullOrEmpty(user.AvatarPath) ? "/assets/img/user.png" : user.AvatarPath,
                    Latitude = user.Latitude,
                    Longitude = user.Longitude
                }
            };
            return View("UserProfile", data);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }
            try
            {
                ApplicationUser user = await _membershipTools.UserManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    ModelState.AddModelError("UserName", "Bu kullanıcı adı daha önceden alınmıştır");
                    return View("Register", model);
                }

                ApplicationUser newUser = new ApplicationUser()
                {
                    AvatarPath = "/assets/img/user.png",
                    EmailConfirmed = false,
                    Name = model.Name,
                    PhoneNumber = model.Phone,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.UserName,
                };
                newUser.ActivationCode = StringHelpers.GetCode();

                IdentityResult result = await _membershipTools.UserManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    switch (_membershipTools.UserManager.Users.Count())
                    {
                        case 1:
                            await _membershipTools.UserManager.AddToRoleAsync(newUser, "Admin");
                            break;
                        case 2:
                            await _membershipTools.UserManager.AddToRoleAsync(newUser, "Operator");
                            break;
                        case 3:
                            await _membershipTools.UserManager.AddToRoleAsync(newUser, "Technician");
                            break;
                        default:
                            await _membershipTools.UserManager.AddToRoleAsync(newUser, "Client");
                            break;
                    }

                    var uri = new UriBuilder()
                    {
                        Scheme = Uri.UriSchemeHttps
                    };
                    var hostComponents = Request.Host.ToUriComponent();
                    string SiteUrl = uri.Scheme + System.Uri.SchemeDelimiter + hostComponents;

                    EmailService emailService = new EmailService();
                    string body = $"Merhaba <b>{newUser.Name} {newUser.Surname}</b><br>Hesabınızı aktif etmek için aşağıdaki linke tıklayınız<br> <a href='{SiteUrl}/account/activation?code={newUser.ActivationCode}' >Aktivasyon Linki </a> ";
                    await emailService.SendAsync(new HelpDesk.Models.Models.EmailModel() { Body = body, Subject = "Sitemize Hoşgeldiniz" }, newUser.Email);
                }
                else
                {
                    string err = "";
                    foreach (IdentityError resultError in result.Errors)
                    {
                        err += resultError.Description;
                    }
                    ModelState.AddModelError(string.Empty, err);
                    return View("Register", model);
                }

                TempData["Message"] = "Kaydınız alınmıştır. Lütfen giriş yapınız";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Register",
                    ControllerName = "Account",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //var user = await _userManager.GetUserAsync(HttpContext.User);
                // user.Id;
                return RedirectToAction("Index", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    return View("Login", model);
                }

                Microsoft.AspNetCore.Identity.SignInResult result = await _membershipTools.SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı");
                return View(model);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Account",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await _membershipTools.SignInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<ActionResult> UserProfile()
        {
            try
            {
                ApplicationUser user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);
                ProfilePasswordViewModel data = new ProfilePasswordViewModel()
                {
                    UserProfileViewModel = new UserProfileViewModel()
                    {
                        Email = user.Email,
                        Id = user.Id,
                        Name = user.Name,
                        PhoneNumber = user.PhoneNumber,
                        Surname = user.Surname,
                        UserName = user.UserName,
                        AvatarPath = string.IsNullOrEmpty(user.AvatarPath) ? "/assets/img/user.png" : user.AvatarPath,
                        Latitude = user.Latitude,
                        Longitude = user.Longitude
                    }
                };

                return View(data);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "UserProfile",
                    ControllerName = "Account",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserProfile(ProfilePasswordViewModel model)
        {
            ApplicationUser user = await _membershipTools.UserManager.FindByIdAsync(model.UserProfileViewModel.Id);

            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            if (model.UserProfileViewModel.PostedFile != null &&
                   model.UserProfileViewModel.PostedFile.Length > 0)
            {
                var file = model.UserProfileViewModel.PostedFile;
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string extName = Path.GetExtension(file.FileName);
                fileName = StringHelpers.UrlFormatConverter(fileName);
                fileName += StringHelpers.GetCode();

                var webpath = _hostingEnvironment.WebRootPath;
                var directorypath = Path.Combine(webpath, "Uploads");
                var filePath = Path.Combine(directorypath, fileName + extName);


                if (!Directory.Exists(directorypath))
                {
                    Directory.CreateDirectory(directorypath);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var oldPath = user.AvatarPath;
                if (oldPath != "/assets/img/user.png")
                {
                    System.IO.File.Delete(Path.Combine(oldPath));
                }
                user.AvatarPath = "/Uploads/" + fileName + extName;
            }

            try
            {
                user.Name = model.UserProfileViewModel.Name;
                user.Surname = model.UserProfileViewModel.Surname;
                user.PhoneNumber = model.UserProfileViewModel.PhoneNumber;
                if (User.IsInRole("Technician"))
                {
                    user.TechnicianStatus = model.UserProfileViewModel.TechnicianStatus;
                    user.Latitude = model.UserProfileViewModel.Latitude;
                    user.Longitude = model.UserProfileViewModel.Longitude;
                }
                if (user.Email != model.UserProfileViewModel.Email)
                {
                    //todo tekrar aktivasyon maili gönderilmeli. rolü de aktif olmamış role çevrilmeli.
                }
                user.Email = model.UserProfileViewModel.Email;

                await _membershipTools.UserManager.UpdateAsync(user);
                TempData["Message"] = "Güncelleme işlemi başarılı.";
                return RedirectToAction("UserProfile");
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "UserProfile",
                    ControllerName = "Account",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ProfilePasswordViewModel model)
        {
            try
            {
                //ApplicationUser user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);

                var name = _membershipTools.IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var user = await _membershipTools.UserManager.FindByNameAsync(name);

                ChangePasswordViewModel data = new ChangePasswordViewModel()
                {
                    OldPassword = model.ChangePasswordViewModel.OldPassword,
                    NewPassword = model.ChangePasswordViewModel.NewPassword,
                    ConfirmNewPassword = model.ChangePasswordViewModel.ConfirmNewPassword
                };

                model.ChangePasswordViewModel = data;
                if (!ModelState.IsValid)
                {
                    return RedirectToAction("Index", "Home");
                }

                IdentityResult result = await _membershipTools.UserManager.ChangePasswordAsync(await _membershipTools.UserManager.GetUserAsync(HttpContext.User),
                    model.ChangePasswordViewModel.OldPassword, model.ChangePasswordViewModel.NewPassword);

                if (result.Succeeded)
                {
                    EmailService emailService = new EmailService();
                    string body = $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızın şifresi değiştirilmiştir. <br> Bilginiz dahilinde olmayan değişiklikler için hesabınızı güvence altına almanızı öneririz.</p>";
                    emailService.Send(new HelpDesk.Models.Models.EmailModel() { Body = body, Subject = "Şifre Değiştirme hk." }, user.Email);

                    return RedirectToAction("Logout", "Account");
                }
                else
                {
                    string err = "";
                    foreach (IdentityError resultError in result.Errors)
                    {
                        err += resultError + " ";
                    }
                    ModelState.AddModelError("", err);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "ChangePassword",
                    ControllerName = "Account",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            try
            {
                ApplicationUser user = await _membershipTools.UserManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, $"{model.Email} mail adresine kayıtlı bir üyeliğe erişilemedi");
                    return View(model);
                }

                string newPassword = StringHelpers.GetCode().Substring(0, 6);

                var hashPassword = _membershipTools.UserManager.PasswordHasher.HashPassword(user, newPassword);

                await _membershipTools.UserStore.SetPasswordHashAsync(user, hashPassword);

                var result = await _membershipTools.UserStore.Context.SaveChangesAsync();

                if (result == 0)
                {
                    var mdl = new ErrorViewModel()
                    {
                        Text = $"Bir hata oluştu:",
                        ActionName = "TechnicianAdd",
                        ControllerName = "Operator",
                        ErrorCode = "500"
                    };
                    TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                    return RedirectToAction("Error", "Home");
                }

                EmailService emailService = new EmailService();
                string body = $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızın parolası sıfırlanmıştır<br> Yeni parolanız: <b>{newPassword}</b> <p>Yukarıdaki parolayı kullanarak sitemize giriş yapabilirsiniz.</p>";
                emailService.Send(new HelpDesk.Models.Models.EmailModel() { Body = body, Subject = $"{user.UserName} Şifre Kurtarma" }, user.Email);
            }

            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "RecoverPassword",
                    ControllerName = "Account",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
            TempData["Message"] = $"{model.Email} mail adresine yeni şifre gönderildi.";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Activation(string code)
        {
            try
            {
                ApplicationUser user = _membershipTools.UserManager.Users.FirstOrDefault(x => x.ActivationCode == code);

                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        ViewBag.Message = $"<span class='alert alert-success'>Bu hesap daha önce aktive edilmiştir.</span>";
                    }
                    else
                    {
                        user.EmailConfirmed = true;
                        _dbContext.SaveChanges();
                        ViewBag.Message = $"<span class='alert alert-success'>Aktivasyon işleminiz başarılı</span>";
                    }
                }
                else
                {
                    ViewBag.Message = $"<span class='alert alert-danger'>Aktivasyon başarısız</span>";
                }
            }
            catch (Exception)
            {
                ViewBag.Message = "<span class='alert alert-danger'>Aktivasyon işleminde bir hata oluştu</span>";
            }

            return RedirectToAction("Login", "Account");
        }
    }
}