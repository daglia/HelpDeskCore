using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HelpDesk.BLL.Account;
using HelpDesk.DAL;
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

        public AdminController(MembershipTools membershipTools, MyContext dbContext) :base(membershipTools)
        {
            _membershipTools = membershipTools;
            _dbContext = dbContext;
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
    }
}