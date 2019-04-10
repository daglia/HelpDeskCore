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
using System.Device.Location;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.Models.Models;
using Microsoft.AspNetCore.Authorization;


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
        private readonly IMapper _mapper;
        
        public OperatorController(MyContext dbContext, MembershipTools membershipTools, IRepository<Failure, int> failureRepo, IRepository<FailureLog, int> failureLogRepo, IRepository<Photo, string> photoRepo, IMapper mapper)
        {
            _dbContext = dbContext;
            _membershipTools = membershipTools;
            _failureRepo = failureRepo;
            _failureLogRepo = failureLogRepo;
            _photoRepo = photoRepo;
            _mapper = mapper;
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
                var data = _mapper.Map<FailureViewModel>(x);
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
                    data.FailureLogs.Add(_mapper.Map<FailureLogViewModel>(failureLog));
                }

                var technicians = _membershipTools.UserManager.GetUsersInRoleAsync("Technician").Result;

                for (int i = 0; i < technicians.Count; i++)
                {
                    var distance = 0.0;
                    string distanceString = "";
                    var technician = technicians[i];
                    if (technician.Latitude.HasValue && technician.Longitude.HasValue && data.Latitude.HasValue && data.Longitude.HasValue)
                    {
                        var failureCoordinate = new GeoCoordinate(data.Latitude.Value, data.Longitude.Value);
                        var technicianCoordinate = new GeoCoordinate(technician.Latitude.Value, technician.Longitude.Value);

                        distance = failureCoordinate.GetDistanceTo(technicianCoordinate) / 1000;
                        distanceString = $"(~{Convert.ToInt32(distance)} km)";
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

        [HttpGet]
        public ActionResult FailureAccept(int id)
        {
            var operatorId = _membershipTools.UserManager.GetUserAsync(HttpContext.User).Result.Id;
            try
            {
                var failure = _failureRepo.GetById(id);
                if (failure == null)
                {
                    RedirectToAction("Index", "Operator");
                }
                else
                {
                    failure.OperationTime = DateTime.Now;
                    failure.OperatorId = operatorId;
                    failure.OperationStatus = OperationStatuses.Accepted;
                    _failureRepo.Update(failure);
                    RedirectToAction("Index", "Operator");
                }

                return RedirectToAction("Index", "Operator");
            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "FailureAccept",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Operator")]
        public ActionResult FailureList()
        {
            try
            {
                var data = _failureRepo
                    .GetAll()
                    .Select(x => _mapper.Map<FailureViewModel>(x))
                    .OrderBy(x => x.OperationTime)
                    .ToList();
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Admin",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Operator")]
        public async Task<ActionResult> TechnicianAdd(FailureViewModel model)
        {
            try
            {
                var failure = _failureRepo.GetById(model.FailureId);
                failure.TechnicianId = model.TechnicianId;
                failure.OperationTime = DateTime.Now;
                failure.OperatorId = _membershipTools.UserManager.GetUserAsync(HttpContext.User).Result.Id;
                failure.OperationStatus = OperationStatuses.Accepted;
                _failureRepo.Update(failure);
                var technician = _membershipTools.UserManager.FindByIdAsync(failure.TechnicianId).Result;
                TempData["Message"] =
                    $"{failure.Id} nolu arızaya {technician.Name} {technician.Surname} atanmıştır.İyi çalışmalar.";

                _failureLogRepo.Insert(new FailureLog()
                {
                    FailureId = model.FailureId,
                    Message = $"Arızaya yeni teknisyen atanmıştır: {technician.Name} {technician.Surname}",
                    FromWhom = IdentityRoles.Operator
                });

                var emailService = new EmailService();
                var body = $"Merhaba <b>{failure.Client.Name} {failure.Client.Surname}</b><br>{failure.FailureName} adlı arızanız onaylanmış ve alanında uzman teknisyenlerimizden birine atanmıştır. Sizinle yeniden iletişime geçilecektir.<br><br>İyi günler dileriz.";
                await emailService.SendAsync(new EmailModel()
                {
                    Body = body,
                    Subject = $"{failure.FailureName} adlı arızanız onaylanmıştır. | Teknik Servisçi"
                }, failure.Client.Email);

                return RedirectToAction("Detail", "Operator", new { id = model.FailureId });
            }

            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "TechnicianAdd",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Operator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Decline(FailureViewModel model)
        {
            try
            {
                var failure = _failureRepo.GetById(model.FailureId);
                if (failure.OperationStatus == OperationStatuses.Declined)
                {
                    TempData["Message"] =
                        $"{failure.Id} nolu arıza zaten reddedilmiştir.";
                    return RedirectToAction("Detail", "Operator", new {id = model.FailureId});
                }

                failure.OperationStatus = OperationStatuses.Declined;
                failure.OperationTime = DateTime.Now;
                failure.OperatorId = _membershipTools.UserManager.GetUserAsync(HttpContext.User).Result.Id;
                failure.Report = model.Report;
                _failureRepo.Update(failure);
                _failureLogRepo.Insert(new FailureLog()
                {
                    FailureId = failure.Id,
                    Message = $"Arızanız şu nedenden dolayı reddedilmiştir: {failure.Report}",
                    FromWhom = IdentityRoles.Operator
                });

                TempData["Message"] =
                    $"{failure.Id} nolu arıza reddedilmiştir.";

                var emailService = new EmailService();
                var body =
                    $"Merhaba <b>{failure.Client.Name} {failure.Client.Surname}</b><br>{failure.FailureName} adlı arızanız şu nedenden dolayı reddedilmiştir:<br><br>{failure.Report}<br><br>İyi günler dileriz.";
                await emailService.SendAsync(new EmailModel()
                {
                    Body = body,
                    Subject = $"{failure.FailureName} adlı arızanız reddedilmiştir. | Teknik Servisçi"
                }, failure.Client.Email);

                return RedirectToAction("Detail", "Operator", new {id = model.FailureId});
            }

            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Decline",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }
        }
    }
}