using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IUserRepository
    {
        IQueryable<User> Users { get; }
        User Get(Expression<Func<User, bool>> predicate);
        User GetById(decimal id);
        IQueryable<User> GetAll(Expression<Func<User, bool>> predicate);
        IQueryable<User> GetAll();

        bool Add(User entity);
        bool Update(User entity);
        bool Delete(User entity);
    }
}
