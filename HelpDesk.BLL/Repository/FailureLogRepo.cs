using System;
using System.Collections.Generic;
using System.Text;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;

namespace HelpDesk.BLL.Repository
{
    public class FailureLogRepo : RepositoryBase<FailureLog,int>
    {
        private readonly MyContext _dbContext;
        public FailureLogRepo(MyContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
