using System.Collections.Generic;
using System.Linq;
using HelpDesk.BLL.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelpDesk.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly MembershipTools _membershipTools;

        public BaseController(MembershipTools membershipTools)
        {
            _membershipTools = membershipTools;
        }
        protected List<SelectListItem> GetRoleSelectList()
        {
            var data = new List<SelectListItem>();
           _membershipTools.RoleStore.Roles
                .ToList()
                .ForEach(x =>
                {
                    data.Add(new SelectListItem()
                    {
                        Text = $"{x.Name}",
                        Value = x.Id
                    });
                });
            return data;
        }
    }
}