using System;
using System.Linq;
using System.Linq.Expressions;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Concrete
{
    public class GlobalTranslationRepository : BaseRepository, IGlobalTranslationRepository
    {
        public IQueryable<GlobalTranslation> GlobalTranslations
        {
            get { return DataContext.GlobalTranslations; }
        }

        public GlobalTranslation Get(Expression<Func<GlobalTranslation, bool>> predicate)
        {
            return GetFirst<GlobalTranslation>(predicate);
        }

        public GlobalTranslation GetById(decimal id)
        {
            return GetFirst<GlobalTranslation>(t => t.Id == id);
        }

        public IQueryable<GlobalTranslation> GetAll(Expression<Func<GlobalTranslation, bool>> predicate)
        {
            return GetAll<GlobalTranslation>(predicate);
        }

        public IQueryable<GlobalTranslation> GetAll()
        {
            return GetAll<GlobalTranslation>();
        }

        public bool Add(GlobalTranslation entity)
        {
            return Add<GlobalTranslation>(entity);
        }

        public bool Update(GlobalTranslation entity)
        {
            var r = GetFirst<GlobalTranslation>(t => t.Id == entity.Id);
            if (r == null) return false;
            
            Attach(r);

            r.LanguageId = entity.LanguageId;
            r.GlobalPhraseId = entity.GlobalPhraseId;
            r.TranslationPhrase = entity.TranslationPhrase;

            return SaveChanges();
        }

        public bool Delete(GlobalTranslation entity)
        {
            return Delete<GlobalTranslation>(entity);
        }
    }
}
