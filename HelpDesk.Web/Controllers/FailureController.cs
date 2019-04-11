using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Helpers;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HelpDesk.Web.Controllers
{
    public class FailureController : Controller
    {
        private readonly MyContext _dbContext;
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Failure, int> _failureRepo;
        private readonly IRepository<FailureLog, int> _failureLogRepo;
        private readonly IRepository<Photo, string> _photoRepo;
        private readonly IRepository<Survey, string> _surveyRepo;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;

        public FailureController(MyContext dbContext, MembershipTools membershipTools, IRepository<Failure, int> failureRepo, IRepository<FailureLog, int> failureLogRepo, IRepository<Photo, string> photoRepo, IRepository<Survey, string> surveyRepo, IHttpContextAccessor httpContext, IHostingEnvironment hostingEnvironment, IMapper mapper)
        {
            _dbContext = dbContext;
            _membershipTools = membershipTools;
            _failureRepo = failureRepo;
            _failureLogRepo = failureLogRepo;
            _photoRepo = photoRepo;
            _surveyRepo = surveyRepo;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
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
                    .Select(x => _mapper.Map<FailureViewModel>(x))
                    .Where(x => x.ClientId == clientId)
                    .OrderBy(x => x.CreatedTime)
                    .ToList();

                return View(data);
            }
            catch (Exception ex)
            {
               var mdl= new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Home",
                    ErrorCode = "500"
                };
               TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
               return RedirectToAction("Error", "Home");
                
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var x = _failureRepo.GetById(id);
                var data = _mapper.Map<FailureViewModel>(x);
                data.PhotoPath = _photoRepo.GetAll(y => y.FailureId == id).Select(y => y.Path).ToList();
                data.ClientId = x.ClientId;

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
                    ControllerName = "Failure",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public IActionResult Invoice(int id)
        {
            try
            {
                var x = _failureRepo.GetById(id);
                var data = _mapper.Map<FailureViewModel>(x);

                return View(data);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Detail",
                    ControllerName = "Failure",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(FailureViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                model.ClientId = _membershipTools.UserManager.GetUserAsync(HttpContext.User).Result.Id;

                var data = _mapper.Map<FailureViewModel, Failure>(model);

                _failureRepo.Insert(data);

                _failureLogRepo.Insert(new FailureLog()
                {
                    FailureId = data.Id,
                    Message = $"#{data.Id} - {data.FailureName} adlı arıza kaydı oluşturuldu.",
                    FromWhom = IdentityRoles.Client
                });

                if (model.PostedFile != null && model.PostedFile.Count > 0)
                {
                    model.PostedFile.ForEach(async file =>
                    {
                        if (file == null || file.Length <= 0)
                            return;

                        string fileName = "failure-" + Path.GetFileNameWithoutExtension(file.FileName);
                        string extName = Path.GetExtension(file.FileName);
                        fileName = StringHelpers.UrlFormatConverter(fileName);
                        fileName += StringHelpers.GetCode();

                        var webpath = _hostingEnvironment.WebRootPath;
                        var directorypath = Path.Combine(webpath, "Uploads/Failure");
                        var filePath = Path.Combine(directorypath, fileName + extName);

                        if (!Directory.Exists(directorypath))
                        {
                            Directory.CreateDirectory(directorypath);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        _photoRepo.Insert(new Photo()
                        {
                            FailureId = data.Id,
                            Path = "/Uploads/Failure/" + fileName + extName
                        });
                    }
                    );
                }
                await _dbContext.SaveChangesAsync();
                var photos = _photoRepo.GetAll(x => x.FailureId == data.Id).ToList();
                var photo = photos.Select(x => x.Path).ToList();
                data.PhotoPath = photo;
                _failureRepo.Update(data);

                await _dbContext.SaveChangesAsync();
                TempData["Message"] = $"{model.FailureName} adlı arızanız operatörlerimizce incelenecektir ve size 24 saat içinde dönüş yapılacaktır.";
                return RedirectToAction("Add");
            }
            //catch (DbEntityValidationException ex)
            //{
            //    TempData["Model"] = new ErrorViewModel()
            //    {
            //        Text = $"Bir hata oluştu: {EntityHelpers.ValidationMessage(ex)}",
            //        ActionName = "Add",
            //        ControllerName = "Failure",
            //        ErrorCode = 500
            //    };
            //    return RedirectToAction("Error", "Home");
            //}
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Add",
                    ControllerName = "Failure",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home" );
            }
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        public ActionResult Survey(string code)
        {
            try
            {
                var survey = _surveyRepo.GetById(code);
                if (survey == null)
                    return RedirectToAction("Index", "Home");
                var data = _mapper.Map<Survey, SurveyViewModel>(survey);
                return View(data);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Survey",
                    ControllerName = "Issue",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public ActionResult Survey(SurveyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Hata Oluştu.");
                return RedirectToAction("Survey", "Failure", model);
            }
            try
            {
                var survey = _surveyRepo.GetById(model.SurveyId);
                if (survey == null)
                    return RedirectToAction("Index", "Home");
                survey.Pricing = model.Pricing;
                survey.Satisfaction = model.Satisfaction;
                survey.Solving = model.Solving;
                survey.Speed = model.Speed;
                survey.TechPoint = model.TechPoint;
                survey.Suggestions = model.Suggestions;
                survey.IsDone = true;
                _surveyRepo.Update(survey);
                TempData["Message"] = "Anket tamamlandı.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "Survey",
                    ControllerName = "Failure",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin, Operator")]
        public ActionResult GetAllFailures()
        {
            try
            {
                var data = _failureRepo
                        .GetAll()
                        .Select(x => _mapper.Map<FailureViewModel>(x))
                        .Where(x => x.FinishingTime != null)
                        .OrderBy(x => x.OperationTime)
                        .ToList();
                return View(data);
            }
            catch (Exception ex)
            {
                var mdl = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "GetAllFailures",
                    ControllerName = "Failure",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(mdl);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}