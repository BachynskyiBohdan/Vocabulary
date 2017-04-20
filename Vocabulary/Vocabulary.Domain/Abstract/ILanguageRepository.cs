using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface ILanguageRepository
    {
        IQueryable<Language> Languages { get; }
        Language Get(Expression<Func<Language, bool>> predicate);
        Language GetById(decimal id);
        IQueryable<Language> GetAll(Expression<Func<Language, bool>> predicate);
        IQueryable<Language> GetAll();

        bool Add(Language entity);
        bool Update(Language entity);
        bool Delete(Language entity);
    }
}
