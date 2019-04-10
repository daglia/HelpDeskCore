using System;
using System.Collections.Generic;
using System.Text;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;

namespace HelpDesk.BLL.Repository
{
    public class FailureRepo : RepositoryBase<Failure,int>
    {
        private readonly MyContext _dbContext;

      
        public FailureRepo(MyContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
