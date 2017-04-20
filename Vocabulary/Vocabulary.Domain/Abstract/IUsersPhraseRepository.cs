using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IUsersPhraseRepository
    {
        IQueryable<UsersPhrase> UsersPhrases { get; }
        UsersPhrase Get(Expression<Func<UsersPhrase, bool>> predicate);
        UsersPhrase GetById(decimal id);
        IQueryable<UsersPhrase> GetAll(Expression<Func<UsersPhrase, bool>> predicate);
        IQueryable<UsersPhrase> GetAll();

        bool Add(UsersPhrase entity);
        bool Update(UsersPhrase entity);
        bool Delete(UsersPhrase entity);
    }
}
