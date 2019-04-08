using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace HelpDesk.Web.Controllers
{
    public class OperatorController : Controller
    {   
        List<SelectListItem> Technicians = new List<SelectListItem>();

        private readonly MyContext _dbContext;
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Photo, string> _photoRepo;
        private readonly IRepository<FailureLog, int> _failureLogRepo;
        private readonly IRepository<Failure, int> _failureRepo;


        public OperatorController(MyContext dbContext, MembershipTools membershipTools, IRepository<Failure, int> failureRepo, IRepository<FailureLog, int> failureLogRepo, IRepository<Photo, string> photoRepo)
        {
            _dbContext = dbContext;
            _membershipTools = membershipTools;
            _failureRepo = failureRepo;
            _failureLogRepo = failureLogRepo;
            _photoRepo = photoRepo;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Detail(int id)
        {
            try
            {
                var x = _failureRepo.GetById(id);
                var data = Mapper.Map<FailureViewModel>(x);
                data.PhotoPath = _photoRepo.GetAll(y => y.FailureId == id).Select(y => y.Path).ToList();
                data.ClientId = x.ClientId;
                var client = await _membershipTools.UserManager.FindByIdAsync(data.ClientId);
                data.ClientName = client.Name;
                data.ClientSurname = client.Surname;
                var failureLogs =  _failureLogRepo 
                    .GetAll()
                    .Where(y => y.FailureId == data.FailureId)
                    .OrderByDescending(y => y.CreatedDate)
                    .ToList();
                data.FailureLogs.Clear();
                foreach (FailureLog failureLog in failureLogs)
                {
                    data.FailureLogs.Add(Mapper.Map<FailureLogViewModel>(failureLog));
                }

                var userId = data.ClientId;
                var userManager = _membershipTools.UserManager;
                var user = userManager.FindByIdAsync(userId).Result;
                var technicianRole =_membershipTools.UserManager.GetRolesAsync(user).Result;
                for (int i = 0; i < technicianRole.Count; i++)
                {
                    var distance = 0.0;
                    string distanceString = "";
                    var technician = _membershipTools.UserManager.FindByIdAsync(technicianRole[i]).Result;
                    if (technician.Latitude.HasValue && technician.Longitude.HasValue && data.Latitude.HasValue && data.Longitude.HasValue)
                    {
                        //var failureCoordinate = new GeoCoordinate(data.Latitude.Value, data.Longitude.Value);
                        //var technicianCoordinate = new GeoCoordinate(technician.Latitude.Value, technician.Longitude.Value);

                        //distance = failureCoordinate.GetDistanceTo(technicianCoordinate) / 1000;
                        //distanceString = $"(~{Convert.ToInt32(distance)} km)";
                    }

                    if (technician.TechnicianStatus == TechnicianStatuses.Available)
                    {
                        Technicians.Add(new SelectListItem()
                        {
                            Text = technician.Name + " " + technician.Surname + " " + distanceString,
                            Value = technician.Id
                        });
                    }
                }

                ViewBag.TechnicianList = Technicians;

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Detail",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }
        }
    }
}