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
    public class LanguageRepository : BaseRepository, ILanguageRepository
    {
        public IQueryable<Language> Languages
        {
            get { return DataContext.Languages; }
        }

        public Language Get(Expression<Func<Language, bool>> predicate)
        {
            return GetFirst<Language>(predicate);
        }

        public Language GetById(decimal id)
        {
            return GetFirst<Language>(t => t.Id == id);
        }

        public IQueryable<Language> GetAll(Expression<Func<Language, bool>> predicate)
        {
            return GetAll<Language>(predicate);
        }

        public IQueryable<Language> GetAll()
        {
            return GetAll<Language>();
        }

        public bool Add(Language entity)
        {
            return Add<Language>(entity);
        }

        public bool Update(Language entity)
        {
            var r = GetFirst<Language>(t => t.Id == entity.Id);
            if (r == null) return false;

            Attach(r);

            r.FullName = entity.FullName;
            r.Code = entity.Code;

            return SaveChanges();
        }

        public bool Delete(Language entity)
        {
            return Delete<Language>(entity);
        }
    }
}
