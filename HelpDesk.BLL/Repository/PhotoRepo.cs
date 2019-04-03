using System;
using System.Collections.Generic;
using System.Text;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;

namespace HelpDesk.BLL.Repository
{
    public class PhotoRepo : RepositoryBase<Photo,string>
    {
        private readonly MyContext _dbContext;
        public PhotoRepo(MyContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
