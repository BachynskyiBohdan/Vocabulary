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
    public class UsersExampleRepository : BaseRepository, IUsersExampleRepository
    {
        public IQueryable<UsersExample> UsersExamples
        {
            get { return DataContext.UsersExamples; }
        }

        public UsersExample Get(Expression<Func<UsersExample, bool>> predicate)
        {
            return GetFirst<UsersExample>(predicate);
        }

        public UsersExample GetById(decimal id)
        {
            return GetFirst<UsersExample>(t => t.Id == id);
        }

        public IQueryable<UsersExample> GetAll(Expression<Func<UsersExample, bool>> predicate)
        {
            return GetAll<UsersExample>(predicate);
        }
        public IQueryable<UsersExample> GetAll()
        {
            return GetAll<UsersExample>();
        }

        public bool Add(UsersExample entity)
        {
            return Add<UsersExample>(entity);
        }

        public bool Update(UsersExample entity)
        {
            var r = GetFirst<UsersExample>(t => t.Id == entity.Id);
            if (r == null) return false;

            Attach(r);

            r.Audio = entity.Audio;
            r.Phrase = entity.Phrase;
            r.Translation = entity.Translation;
            r.PhraseId = entity.PhraseId;
            r.TranslationLanguageId = entity.TranslationLanguageId;

            return SaveChanges();
        }

        public bool Delete(UsersExample entity)
        {
            return Delete<UsersExample>(entity);
        }
    }
}
