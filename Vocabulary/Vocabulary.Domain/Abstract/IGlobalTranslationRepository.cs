using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IGlobalTranslationRepository
    {
        IQueryable<GlobalTranslation> GlobalTranslations { get; }
        GlobalTranslation Get(Expression<Func<GlobalTranslation, bool>> predicate);
        GlobalTranslation GetById(decimal id);
        IQueryable<GlobalTranslation> GetAll(Expression<Func<GlobalTranslation, bool>> predicate);
        IQueryable<GlobalTranslation> GetAll();

        bool Add(GlobalTranslation entity);
        bool Update(GlobalTranslation entity);
        bool Delete(GlobalTranslation entity);
    }
}
