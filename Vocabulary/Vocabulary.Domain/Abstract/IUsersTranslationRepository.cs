using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IUsersTranslationRepository
    {
        IQueryable<UsersTranslation> UsersTranslations { get; }
        UsersTranslation Get(Expression<Func<UsersTranslation, bool>> predicate);
        UsersTranslation GetById(decimal id);
        IQueryable<UsersTranslation> GetAll(Expression<Func<UsersTranslation, bool>> predicate);
        IQueryable<UsersTranslation> GetAll();

        bool Add(UsersTranslation entity);
        bool Update(UsersTranslation entity);
        bool Delete(UsersTranslation entity);
    }
}
