using HelpDesk.DAL;
using HelpDesk.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.BLL.Repository.Abstracts
{
    public abstract class RepositoryBase<T, TId> : IRepository<T, TId> where T : BaseEntity<TId>
    {
        internal readonly MyContext DbContext;
        internal readonly DbSet<T> DbObject;

        internal RepositoryBase(MyContext dbContext)
        {
            DbContext = dbContext;
            DbObject = DbContext.Set<T>();
        }
        public IQueryable<T> GetAll() => DbObject;

        public IQueryable<T> GetAll(Func<T, bool> predicate) => DbObject.Where(predicate).AsQueryable();

        public T GetById(TId id) => DbObject.Find(id);

        public virtual void Insert(T entity)
        {
            DbObject.Add(entity);
            DbContext.SaveChanges();
        }

        public virtual void Delete(T entity)
        {
            DbObject.Remove(entity);
            Save();
        }

        public virtual void Update(T entity)
        {
            DbObject.Attach(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
            entity.UpdatedDate = DateTime.Now;
            Save();
        }
        public void Save() => DbContext.SaveChanges();
    }
}