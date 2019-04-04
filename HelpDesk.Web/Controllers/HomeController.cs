using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Models.Enums;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.Models.Entities;

namespace HelpDesk.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Failure, int> _failureRepo;

        public HomeController(IRepository<Failure,int> failureRepo, MembershipTools membershipTools)
        {
            _membershipTools = membershipTools;
            _failureRepo = failureRepo;
        }

        public IActionResult Index()
        {
            var userManager = _membershipTools.UserManager;
            var signInManager = _membershipTools.SignInManager;

            //if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            //    return RedirectToAction("Logout", "Account");

            if (!User.IsInRole("User") && User.Identity.IsAuthenticated)
            {
                ViewBag.pendingFailureCount = _failureRepo.GetAll().Where(x => x.OperationStatus == OperationStatuses.Pending).Count();

                ViewBag.availableTechnicianCount = userManager.Users.Where(x => x.TechnicianStatus == TechnicianStatuses.Available).Count();
            }

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
