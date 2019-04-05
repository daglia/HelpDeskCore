using System.Linq;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using HelpDesk.DAL;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace HelpDesk.BLL.Account
{
    public class MembershipTools
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly RoleStore<ApplicationRole> _roleStore;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MyContext _context;

        public MembershipTools(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,RoleStore<ApplicationRole> roleStore, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, MyContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _roleStore = roleStore;
        }

        public UserStore<ApplicationUser> UserStore
        {
            get { return new UserStore<ApplicationUser>(_context); }
        }

        public UserManager<ApplicationUser> UserManager
        {
            get { return _userManager; }
        }

        public RoleManager<ApplicationRole> RoleManager
        {
            get { return _roleManager; }
        }

        public SignInManager<ApplicationUser> SignInManager
        {
            get { return _signInManager; }
        }

        public IHttpContextAccessor IHttpContextAccessor
        {
            get { return _httpContextAccessor; }
        }

        public RoleStore<ApplicationRole> RoleStore
        {
            get { return new RoleStore<ApplicationRole>(_context); }
        }

        public async Task<string> GetNameSurname(string userId)
        {
            ApplicationUser user;
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    return "";
                }

                user = await UserManager.FindByIdAsync(id);
            }
            else
            {
                user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }
            }

            return $"{user.Name} {user.Surname}";
        }

        public async Task<string> GetRole(string userId)
        {
            ApplicationUser user;
            string role = "";
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                    return "";

                user = await UserManager.FindByIdAsync(id);
            }
            else
            {
                user = UserManager.FindByIdAsync(userId).Result;
                var roles = UserManager.GetRolesAsync(user).Result;
                ApplicationRole roleuser;
                foreach (var item in roles)
                {
                    roleuser = _roleManager.FindByNameAsync(item).Result;
                    role = roleuser.ToString();
                }
            }

            return $"{role}";
        }

        public async Task<string> GetRoleWithColor(string userId)
        {
            ApplicationUser user;
            string span = "";
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                    return "";

                user = await UserManager.FindByIdAsync(id);
            }
            else
            {
                user = UserManager.FindByIdAsync(userId).Result;
                if (user == null)
                    return null;

                var rolename = UserManager.GetRolesAsync(user).Result.FirstOrDefault();

                switch (rolename)
                {
                    case "Admin":
                        span = "label-purple";
                        break;
                    case "Technician":
                        span = "label-warning";
                        break;
                    case "Operator":
                        span = "label-success";
                        break;
                    default:
                        span = "label-primary";
                        break;
                }
            }

            return span;
        }

        public async Task<TechnicianStatuses?> GetTechnicianStatus(string userId)
        {
            ApplicationUser user;
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                    return null;

                user = await UserManager.FindByIdAsync(id);
            }
            else
            {
                user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                    return null;
            }

            return user.TechnicianStatus;
        }

        public async Task<string> GetAvatarPath(string userId)
        {
            ApplicationUser user;
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                    return "/assets/img/user.png";

                user = await UserManager.FindByIdAsync(id);
                if (user.AvatarPath == null)
                    return "/assets/img/user.png";
            }
            else
            {
                user = await UserManager.FindByIdAsync(userId);
                if (user.AvatarPath == null)
                    return "/assets/img/user.png";
            }

            return $"{user.AvatarPath}";
        }

        public async Task<string> GetNameSurnameCurrent()
        {
            ApplicationUser user;
            string id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            user = await _userManager.FindByIdAsync(id);
            return $"{user.Name} {user.Surname}";
        }

        public async Task<string> GetEmailCurrent(string userId)
        {
            ApplicationUser user;
            if (string.IsNullOrEmpty(userId))
            {
                string id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    return "";
                }

                user = await UserManager.FindByIdAsync(id);
            }
            else
            {
                user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }
            }
            return $"{user.Email}";
        }

    }
}
