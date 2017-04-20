using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IBaseRepository : IDisposable
    {
        EFDbContext DataContext { get; }

        TEntity GetFirst<TEntity> (Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        IQueryable<TEntity> GetAll<TEntity> (Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

        TEntity Attach<TEntity>(TEntity entity) where TEntity : class;
        bool Add<TEntity> (TEntity entity) where TEntity : class;
        bool Delete<TEntity>(TEntity entity) where TEntity : class;

        bool SaveChanges();
    }
}
