using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Concrete
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public IQueryable<User> Users
        {
            get { return DataContext.Users; }
        }

        public User Get(Expression<Func<User, bool>> predicate)
        {
            return GetFirst<User>(predicate);
        }

        public User GetById(decimal id)
        {
            return GetFirst<User>(t => t.Id == id);
        }

        public IQueryable<User> GetAll(Expression<Func<User, bool>> predicate)
        {
            return GetAll<User>(predicate);
        }

        public IQueryable<User> GetAll()
        {
            return GetAll<User>();
        }

        public bool Add(User entity)
        {
            return Add<User>(entity);
        }

        public bool Update(User entity)
        {
            var r = GetFirst<User>(t => t.Id == entity.Id);
            if (r == null) return false;

            Attach(r);

            r.FirstName = entity.FirstName;
            r.LastName = entity.LastName;
            r.DateOfBirth = entity.DateOfBirth;
            r.DateOfBirth = entity.DateOfBirth;
            r.UserTag = entity.UserTag;
            r.Email = entity.Email;
            r.Password = entity.Password;
            r.IsPremium = entity.IsPremium;
            r.MaximumPhrases = entity.MaximumPhrases;
            r.MaximumPhrases = entity.MaximumPhrases;
            r.IconData = entity.IconData;
            r.IconMimeType = entity.IconMimeType;
            r.Description = entity.Description;
            r.RoleId = entity.RoleId;

            return SaveChanges();
        }

        public bool Delete(User entity)
        {
            return Delete<User>(entity);
        }
    }
}
