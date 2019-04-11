using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
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
        public ActionResult Reports()
        {
            try
            {
                var failureList = _failureRepo.GetAll(x => x.SurveyId != null).ToList();
                var surveyList = _surveyRepo.GetAll().Where(x => x.IsDone).ToList();
                var totalSpeed = 0.0;
                var totalTech = 0.0;
                var totalPrice = 0.0;
                var totalSatisfaction = 0.0;
                var totalSolving = 0.0;
                var count = failureList.Count;

                if (count == 0)
                {
                    TempData["Message"] = "Herhangi bir kayıt bulunamadı.";
                    return RedirectToAction("Index", "Home");
                }
                foreach (var survey in surveyList)
                {
                    totalSpeed += survey.Speed;
                    totalTech += survey.TechPoint;
                    totalPrice += survey.Pricing;
                    totalSatisfaction += survey.Satisfaction;
                    totalSolving += survey.Solving;
                }
                var totalDays = 0;

                foreach (var failure in failureList)
                {
                    if (failure.FinishingTime.HasValue)
                        totalDays += failure.FinishingTime.Value.DayOfYear - failure.CreatedDate.DayOfYear;
                }

                ViewBag.AvgSpeed = totalSpeed / count;
                ViewBag.AvgTech = totalTech / count;
                ViewBag.AvgPrice = totalPrice / count;
                ViewBag.AvgSatisfaction = totalSatisfaction / count;
                ViewBag.AvgSolving = totalSolving / count;
                ViewBag.AvgTime = totalDays / failureList.Count;

                return View(surveyList);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Reports",
                    ControllerName = "Admin",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}