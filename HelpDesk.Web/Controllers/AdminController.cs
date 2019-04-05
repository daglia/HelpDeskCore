using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.BLL.Account;
using HelpDesk.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly MembershipTools _membershipTools;
        private readonly MyContext _dbContext;

        public AdminController(MembershipTools membershipTools, MyContext dbContext)
        {
            _membershipTools = membershipTools;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult UserList()
        {
            return View(_membershipTools.UserStore.Users.ToList());
        }
    }
}