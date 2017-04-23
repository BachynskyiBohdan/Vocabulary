using System;
using System.Linq;
using System.Linq.Expressions;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Concrete
{
    public class BaseRepository : IBaseRepository
    {
        public EFDbContext DataContext { get; private set; }

        public BaseRepository()
        {
            DataContext = new EFDbContext();
            DataContext.Database.Log = Logger.Log;
        }

        public TEntity GetFirst<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return DataContext.Set<TEntity>().First(predicate);
        }

        public IQueryable<TEntity> GetAll<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return DataContext.Set<TEntity>().Where(predicate);
        }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return DataContext.Set<TEntity>().AsQueryable();
        }

        public TEntity Attach<TEntity>(TEntity entity) where TEntity : class
        {
            return DataContext.Set<TEntity>().Attach(entity);
        }

        public bool Add<TEntity>(TEntity entity) where TEntity : class
        {
            DataContext.Set<TEntity>().Add(entity);
            return SaveChanges();
        }

        public bool Delete<TEntity>(TEntity entity) where TEntity : class
        {
            DataContext.Set<TEntity>().Remove(entity);
            return SaveChanges();
        }

        public bool SaveChanges()
        {
            var result = 0;
            try
            {
                result = DataContext.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
            return result > 0;
        }

        public void Dispose()
        {
            if (DataContext != null) DataContext.Dispose();
        }

    }
}
