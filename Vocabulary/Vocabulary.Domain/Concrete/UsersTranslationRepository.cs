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
    public class UsersTranslationRepository : BaseRepository, IUsersTranslationRepository
    {
        public IQueryable<UsersTranslation> UsersTranslations
        {
            get { return DataContext.UsersTranslations; }
        }

        public UsersTranslation Get(Expression<Func<UsersTranslation, bool>> predicate)
        {
            return GetFirst<UsersTranslation>(predicate);
        }

        public UsersTranslation GetById(decimal id)
        {
            return GetFirst<UsersTranslation>(t => t.Id == id);
        }

        public IQueryable<UsersTranslation> GetAll(Expression<Func<UsersTranslation, bool>> predicate)
        {
            return GetAll<UsersTranslation>(predicate);
        }

        public IQueryable<UsersTranslation> GetAll()
        {
            return GetAll<UsersTranslation>();
        }

        public bool Add(UsersTranslation entity)
        {
            return Add<UsersTranslation>(entity);
        }

        public bool Update(UsersTranslation entity)
        {
            var r = GetFirst<UsersTranslation>(t => t.Id == entity.Id);
            if (r == null) return false;

            Attach(r);

            r.LanguageId = entity.LanguageId;
            r.UserPhraseId = entity.UserPhraseId;
            r.TranslationPhrase = entity.TranslationPhrase;

            return SaveChanges();
        }

        public bool Delete(UsersTranslation entity)
        {
            return Delete<UsersTranslation>(entity);
        }
    }
}
