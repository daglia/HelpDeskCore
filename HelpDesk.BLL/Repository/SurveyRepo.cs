using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;

namespace HelpDesk.BLL.Repository
{
    public class SurveyRepo : RepositoryBase<Survey, string>
    {
        private readonly MyContext _dbContext;
        public SurveyRepo(MyContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Rapor4()
        {
            //var teknisyenSorgu = from ariza in _dbContext.Failures join  
            //    join teknisyen in _dbContext.Users on ariza.TechnicianId equals teknisyen.Id
            //    join anket in _dbContext.Surveys on ariza.SurveyId equals anket.Id
            //    where teknisyen.Id == ariza.TechnicianId & anket.Id == ariza.SurveyId
            //    group new
            //        {
            //            ariza,
            //            teknisyen
            //        }
            //        by new
            //        {
            //            teknisyen.Name,
            //            teknisyen.Surname
            //        }
            //    into gp
            //    select new
            //    {
            //        isim = gp.Key.Name + " " + gp.Key.Surname,
            //        toplam = gp.Average(x => x.ariza.Survey.Solving)
            //    };


            var teknisyenSorgu = from ariza in _dbContext.Failures
                                 join teknisyen in _dbContext.Users on ariza.TechnicianId equals teknisyen.Id
                                 join anket in _dbContext.Surveys on ariza.SurveyId equals anket.Id
                                 //where teknisyen.Id == ariza.TechnicianId & anket.Id == ariza.SurveyId
                                 group new
                                 {
                                     ariza,
                                     teknisyen
                                 }
                                     by new
                                     {
                                         teknisyen.Name,
                                         teknisyen.Surname
                                     }
                                     into gp
                                 select new
                                 {
                                     isim = gp.Key.Name + " " + gp.Key.Surname,
                                     toplam = gp.Average(x => x.ariza.Survey.Solving)
                                 };

            var result = teknisyenSorgu.ToList();
            Console.WriteLine();
        }
    }
}
