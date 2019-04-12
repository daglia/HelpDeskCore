using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Helpers;
using HelpDesk.BLL.Repository;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.Models;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HelpDesk.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly MembershipTools _membershipTools;
        private readonly MyContext _dbContext;
        private readonly IRepository<Failure,int> _failureRepo;
        private readonly IRepository<Survey, string> _surveyRepo;

        public AdminController(MembershipTools membershipTools, MyContext dbContext, IRepository<Failure,int> failureRepo, IRepository<Survey, string> surveyRepo) :base(membershipTools)
        {
            _membershipTools = membershipTools;
            _dbContext = dbContext;
            _failureRepo = failureRepo;
            _surveyRepo = surveyRepo;
        }

       
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            try
            {
                var user = await _membershipTools.UserManager.FindByIdAsync(id);
               
                if (user == null)
                    return RedirectToAction("EditUser");

                var roller = GetRoleSelectList();
                foreach (var role in _membershipTools.UserManager.GetRolesAsync(user).Result)
                {
                    foreach (var selectListItem in roller)
                    {
                        if (selectListItem.Value == role)
                            selectListItem.Selected = true;
                    }
                }

                ViewBag.RoleList = roller;

                var model = new UserProfileViewModel()
                {
                    AvatarPath = user.AvatarPath,
                    Name = user.Name,
                    Email = user.Email,
                    Surname = user.Surname,
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName
                };
                if (await _membershipTools.UserManager.IsInRoleAsync(user, "Technician"))
                {
                    model.TechnicianStatus = TechnicianStatuses.Available;
                    user.TechnicianStatus = TechnicianStatuses.Available;
                    _dbContext.Update(user);
                }
                else
                {
                    model.TechnicianStatus = null;
                    user.TechnicianStatus = null;
                    _dbContext.Update(user);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Admin",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRoles(UpdateUserRoleViewModel model)
        {
            //var userId = Request.Form[1].ToString();
            //var rolIdler = Request.Form[2].ToString().Split(',');

            var userId = model.Id;
            var rolIdler = model.Roles;
            var roleManager = _membershipTools.RoleManager;
            var seciliRoller = new string[rolIdler.Count];
            for (var i = 0; i < rolIdler.Count; i++)
            {
                var rid = rolIdler[i];
                seciliRoller[i] = roleManager.FindByIdAsync(rid).Result.ToString();
            }

            var userManager = _membershipTools.UserManager;
            var user = userManager.FindByIdAsync(userId).Result;
            var Roles = _membershipTools.UserManager.GetRolesAsync(user).Result;
            foreach (var identityUserRole in Roles)
            {
                await userManager.RemoveFromRoleAsync(user, identityUserRole);
            }

            for (int i = 0; i < seciliRoller.Length; i++)
            {
                await userManager.AddToRoleAsync(user, seciliRoller[i]);
            }

            return RedirectToAction("EditUser", new {id = userId});
        }

        [HttpGet]
        public IActionResult UserList()
        {
            return View(_membershipTools.UserStore.Users.ToList());
        }
        [HttpGet]
        [HttpGet]
        [Authorize(Roles = "Admin,Operator")]
        public ActionResult Reports()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Rapor1()
        {
            var surveyRepo = _surveyRepo;
            var question1 = surveyRepo.GetAll().Select(x => x.Satisfaction).Sum() / surveyRepo.GetAll().Select(x => x.Satisfaction).Count();
            var question2 = surveyRepo.GetAll().Select(x => x.TechPoint).Sum() / surveyRepo.GetAll().Select(x => x.TechPoint).Count();
            var question3 = surveyRepo.GetAll().Select(x => x.Speed).Sum() / surveyRepo.GetAll().Select(x => x.Speed).Count();
            var question4 = surveyRepo.GetAll().Select(x => x.Pricing).Sum() / surveyRepo.GetAll().Select(x => x.Pricing).Count();
            var question5 = surveyRepo.GetAll().Select(x => x.Solving).Sum() / surveyRepo.GetAll().Select(x => x.Solving).Count();
     


            var data = new List<ReportData>();
            data.Add(new ReportData()
            {
                question = "Genel Memnuniyet",
                point = question1

            });
            data.Add(new ReportData()
            {
                question = "Teknisyen",
                point = question2
            });
            data.Add(new ReportData()
            {
                question = "Hız",
                point = question3
            });
            data.Add(new ReportData()
            {
                question = "Fiyat",
                point = question4
            });
            data.Add(new ReportData()
            {
                question = "Çözüm Odaklılık",
                point = question5
            });
            return Json(new ResponseData()
            {
                message = $"{data.Count} adet kayıt bulundu",
                success = true,
                data = data
            });
        }
        [HttpGet]
        public async Task<JsonResult>  Rapor2()
        {
            var user =_membershipTools.UserManager.Users.ToList();
            var arizaRepo = _failureRepo;
            var sonArizalar = new List<FailureViewModel>();
            foreach (var item in user)
            {
                if (await _membershipTools.UserManager.IsInRoleAsync(item, "Technician"))
                {
                    var ariza = arizaRepo.GetAll().Last(x => x.TechnicianId == item.Id);
                    if (ariza != null)
                    {

                        sonArizalar.Add(new FailureViewModel()
                        {
                            TechnicianId = item.Id,
                            StartingTime = ariza.StartingTime,
                            FinishingTime = ariza.FinishingTime
                        });
                    }
                }
            }
            return Json(new ResponseData()
            {
                message = $"{sonArizalar.Count} adet kayıt bulundu",
                success = true,
                data = sonArizalar
            });
        }
        [HttpGet]

        public JsonResult Rapor3()
        {
            var arizaRepo =_failureRepo;
            var arizalar = arizaRepo.GetAll(x => x.RepairProcess != null);
            var toplamS = new TimeSpan();
            foreach (var ariza in arizalar)
            {
                toplamS += (TimeSpan)(ariza.FinishingTime - ariza.StartingTime);
            }
            return Json(new ResponseData()
            {
                message = $" adet kayıt bulundu",
                success = true,
            });
        }

        [HttpGet]
        public JsonResult Rapor4()
        {
            var arizaRepo = _failureRepo.GetAll();
            var userRepo =_membershipTools.UserManager.Users.ToList();
            var anketRepo = _surveyRepo.GetAll();

            var teknisyenSorgu = from ariza in arizaRepo
                                 join teknisyen in userRepo on ariza.TechnicianId equals teknisyen.Id
                                 join anket in anketRepo on ariza.SurveyId equals anket.Id
                                 where teknisyen.Id == ariza.TechnicianId & anket.Id == ariza.SurveyId
                                 group new
                                 {
                                     ariza,
                                     teknisyen
                                 }
                                 by new
                                 {
                                     teknisyen.Name,
                                     teknisyen.Surname
                                 }
                               into gp
                                 select new
                                 {
                                     isim = gp.Key.Name + " " + gp.Key.Surname,
                                     toplam = gp.Average(x => x.ariza.Survey.Solving)
                                 };

            var data = teknisyenSorgu.ToList();

            return Json(new ResponseData()
            {
                message = $"{data.Count} adet kayıt bulundu",
                success = true,
                data = data
            });
        }

        [HttpPost]
        public async Task<JsonResult> SendCode(string id)
        {
            try
            {
                var user = await _membershipTools.UserStore.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new ResponseData
                    {
                        message = "Kullanıcı bulunamadı",
                        success = false
                    });
                }

                user.ActivationCode = StringHelpers.GetCode();
                await _membershipTools.UserStore.UpdateAsync(user);
                _membershipTools.UserStore.Context.SaveChanges();

                var uri = new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps
                };
                var hostComponents = Request.Host.ToUriComponent();
                string SiteUrl = uri.Scheme + System.Uri.SchemeDelimiter + hostComponents;

                EmailService emailService = new EmailService();
                string body = $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızı aktif etmek için aşağıdaki linke tıklayınız<br> <a href='{SiteUrl}/account/activation?code={user.ActivationCode}' >Aktivasyon Linki </a> ";
                await emailService.SendAsync(new HelpDesk.Models.Models.EmailModel() { Body = body, Subject = "Üyelik Aktivasyonu" }, user.Email);
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    message = $"Bir hata oluştu: {ex.Message}",
                    success = false
                });
            }
            return Json(new ResponseData()
            {
                message = "Şifre sıfırlama maili gönderilmiştir",
                success = true
            });
        }

        [HttpPost]
        public async Task<JsonResult> SendPassword(string id)
        {
            try
            {
                var user = await _membershipTools.UserStore.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new ResponseData()
                    {
                        message = "Kullanıcı bulunamadı",
                        success = false
                    });
                }

                string newPassword = StringHelpers.GetCode().Substring(0, 6);

                var hashPassword = _membershipTools.UserManager.PasswordHasher.HashPassword(user, newPassword);

                await _membershipTools.UserStore.SetPasswordHashAsync(user, hashPassword);

                await _membershipTools.UserStore.Context.SaveChangesAsync();

                EmailService emailService = new EmailService();
                string body = $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızın parolası sıfırlanmıştır<br> Yeni parolanız: <b>{newPassword}</b> <p>Yukarıdaki parolayı kullanarak sitemize giriş yapabilirsiniz.</p>";
                emailService.Send(new HelpDesk.Models.Models.EmailModel() { Body = body, Subject = $"{user.UserName} Şifre Kurtarma" }, user.Email);

                return Json(new ResponseData()
                {
                    message = "Şifre sıfırlama maili gönderilmiştir",
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    message = $"Bir hata oluştu: {ex.Message}",
                    success = false
                });
            }
        }
    }
}