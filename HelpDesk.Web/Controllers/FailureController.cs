using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers
{
    public class FailureController : Controller
    {
        private readonly MyContext _dbContext;
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Failure, int> _failureRepo;

        public FailureController(MyContext dbContext, MembershipTools membershipTools, IRepository<Failure, int> failureRepo, IHttpContextAccessor httpContext)
        {
            _dbContext = dbContext;
            _membershipTools = membershipTools;
            _failureRepo = failureRepo;
        }
 
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var username = _membershipTools.IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

            var user = await _membershipTools.UserManager.FindByNameAsync(username);

            var clientId = user.Id;
             
            try
            {
                var data = _failureRepo
                    .GetAll()
                    .Select(x => Mapper.Map<FailureViewModel>(x))
                    .Where(x => x.ClientId == clientId)
                    .OrderBy(x => x.CreatedTime)
                    .ToList();

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Home",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");

            }
        }
    }
}