using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IUsersExampleRepository
    {
        IQueryable<UsersExample> UsersExamples { get; }
        UsersExample Get(Expression<Func<UsersExample, bool>> predicate);
        UsersExample GetById(decimal id);
        IQueryable<UsersExample> GetAll(Expression<Func<UsersExample, bool>> predicate);
        IQueryable<UsersExample> GetAll();

        bool Add(UsersExample entity);
        bool Update(UsersExample entity);
        bool Delete(UsersExample entity);
    }
}
