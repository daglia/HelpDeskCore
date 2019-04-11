using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Models.Models;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace HelpDesk.Web.Controllers
{
    public class TechnicianController : Controller
    {
        private readonly MyContext _dbContext;
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Photo, string> _photoRepo;
        private readonly IRepository<FailureLog, int> _failureLogRepo;
        private readonly IRepository<Failure, int> _failureRepo;
        private readonly IRepository<Survey, string> _surveyRepo;
        private readonly IMapper _mapper;

        public TechnicianController(MyContext dbContext, MembershipTools membershipTools, IRepository<Failure, int> failureRepo, IRepository<FailureLog, int> failureLogRepo, IRepository<Photo, string> photoRepo, IRepository<Survey, string> surveyRepo, IMapper mapper)
        {
            _dbContext = dbContext;
            _membershipTools = membershipTools;
            _failureRepo = failureRepo;
            _failureLogRepo = failureLogRepo;
            _photoRepo = photoRepo;
            _surveyRepo = surveyRepo;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FailureList()
        {
            var techId = _membershipTools.UserManager.GetUserAsync(HttpContext.User).Result.Id;
            try
            {
                var data = _failureRepo
                    .GetAll()
                    .Select(x => _mapper.Map<FailureViewModel>(x))
                    .Where(x => x.TechnicianId == techId)
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
                    ControllerName = "Home",
                    ErrorCode = "500"
                };
                return RedirectToAction("Error", "Home");

            }
        }

        [HttpGet]
        public ActionResult Detail(int id)
        {
            try
            {
                var x = _failureRepo.GetById(id);
                var data = _mapper.Map<FailureViewModel>(x);
                data.ClientId = x.ClientId;

                data.PhotoPath = _photoRepo.GetAll(y => y.FailureId == id).Select(y => y.Path).ToList();
                var failureLogs = _failureLogRepo
                    .GetAll()
                    .Where(y => y.FailureId == data.FailureId)
                    .OrderByDescending(y => y.CreatedDate)
                    .ToList();
                data.FailureLogs.Clear();
                foreach (FailureLog failureLog in failureLogs)
                {
                    data.FailureLogs.Add(_mapper.Map<FailureLogViewModel>(failureLog));
                }

                return View(data);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Detail",
                    ControllerName = "Technician",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public IActionResult TechnicianStartWork(FailureViewModel model)
        {
            try
            {
                var failure = _failureRepo.GetById(model.FailureId);
                failure.Technician = _membershipTools.UserManager.FindByIdAsync(failure.TechnicianId).Result;
                switch (failure.Technician.TechnicianStatus)
                {
                    case TechnicianStatuses.Available:
                        model.TechnicianStatus = TechnicianStatuses.OnWay;
                        _failureLogRepo.Insert(new FailureLog()
                        {
                            FailureId = model.FailureId,
                            Message = "Teknisyen yola çıktı.",
                            FromWhom = IdentityRoles.Technician
                        });
                        //todo: Kullanıcıya mail gitsin.
                        break;
                    case TechnicianStatuses.OnWay:
                        failure.StartingTime = DateTime.Now;
                        model.TechnicianStatus = TechnicianStatuses.OnWork;
                        _failureLogRepo.Insert(new FailureLog()
                        {
                            FailureId = model.FailureId,
                            Message = "Teknisyen işe başladı.",
                            FromWhom = IdentityRoles.Technician
                        });
                        break;
                    case TechnicianStatuses.OnWork:
                        model.TechnicianStatus = TechnicianStatuses.Available;
                        failure.FinishingTime = DateTime.Now;
                        _failureLogRepo.Insert(new FailureLog()
                        {
                            FailureId = model.FailureId,
                            Message = "İş tamamlandı.",
                            FromWhom = IdentityRoles.Technician
                        });
                        return RedirectToAction("CreateInvoice", "Technician", new
                        {
                            id = model.FailureId
                        });
                    default:
                        break;
                }

                failure.Report = model.Report;
                failure.Technician.TechnicianStatus = model.TechnicianStatus;
                if (model.RepairProcess == RepairProcesses.Successful)
                {
                    failure.FinishingTime = DateTime.Now;
                }
                else if (model.RepairProcess == RepairProcesses.Failed)
                    failure.FinishingTime = DateTime.Now;
                
                _failureRepo.Update(failure);

                TempData["Message"] = $"{model.FailureId} no lu arıza için yola çıkılmıştır";
                return RedirectToAction("Detail", "Technician", new
                {
                    id = model.FailureId
                });
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "TechnicianStartWork",
                    ControllerName = "Technician",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }
        public IActionResult TechnicianReport(int id)
        {
            try
            {
                var failure = _failureRepo.GetById(id);
                var data = _mapper.Map<FailureViewModel>(failure);
                return View(data);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "TechnicianReport",
                    ControllerName = "Technician",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public IActionResult CreateInvoice(int id)
        {
            var failure = _failureRepo.GetById(id);
            var data = _mapper.Map<FailureViewModel>(failure);
            return View(data);
        }

        [HttpPost]
        public ActionResult CreateInvoice(FailureViewModel model)
        {
            try
            {
                var failure = _failureRepo.GetById(model.FailureId);
                if (model.HasWarranty)
                {
                    failure.Price = 0m;
                }
                else
                {
                    failure.Price = model.Price;
                }
                failure.HasWarranty = model.HasWarranty;
                failure.Report = model.Report;
                failure.RepairProcess = model.RepairProcess;
                _failureRepo.Update(failure);
                TempData["Message"] = $"{model.FailureId} no lu arıza için tutar girilmiştir.";

                //var survey = new SurveyRepo().GetById(model.FailureId);
                var survey = new Survey();
                _surveyRepo.Insert(survey);
                failure.SurveyId = survey.Id;
                _surveyRepo.Update(survey);

                var user =_membershipTools.UserManager.FindByIdAsync(failure.ClientId).Result;
                var clientNameSurname = _membershipTools.GetNameSurname(failure.ClientId);

                var uri = new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps
                };
                var hostComponents = Request.Host.ToUriComponent();
                string siteUrl = uri.Scheme + System.Uri.SchemeDelimiter + hostComponents;

                EmailService emailService = new EmailService();
                var body = $"Merhaba <b>{clientNameSurname.Result}</b><br>{failure.Description} adlı arıza kaydınız kapanmıştır.<br>Değerlendirmeniz için aşağıda linki bulunan anketi doldurmanızı rica ederiz.<br> <a href='{siteUrl}/failure/survey?code={failure.SurveyId}' >Anket Linki </a> ";
                emailService.Send(new EmailModel() { Body = body, Subject = "Değerlendirme Anketi" }, user.Email);

                return RedirectToAction("Detail", "Technician", new
                {
                    id = model.FailureId
                });
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "CreateInvoice",
                    ControllerName = "Technician",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}