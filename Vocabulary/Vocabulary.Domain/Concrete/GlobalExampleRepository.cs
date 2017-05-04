using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Concrete
{
    public class GlobalExampleRepository : BaseRepository, IGlobalExampleRepository
    {
        public IQueryable<GlobalExample> GlobalExamples
        {
            get { return DataContext.GlobalExamples; }
        }

        public GlobalExample Get(Expression<Func<GlobalExample, bool>> predicate)
        {
            return GetFirst<GlobalExample>(predicate);
        }

        public GlobalExample GetById(decimal id)
        {
            return GetFirst<GlobalExample>(t => t.Id == id);
        }

        public IQueryable<GlobalExample> GetAll(Expression<Func<GlobalExample, bool>> predicate)
        {
            return GetAll<GlobalExample>(predicate);
        }

        public IQueryable<GlobalExample> GetAll()
        {
            return GetAll<GlobalExample>();
        }

        public bool Add(GlobalExample entity)
        {
            return Add<GlobalExample>(entity);
        }

        public bool Update(GlobalExample entity)
        {
            var r = GetFirst<GlobalExample>(t => t.Id == entity.Id);
            if (r == null) return false;

            Attach(r);

#warning Проследить за массивом Audio

            r.Audio = entity.Audio; 
            r.Phrase = entity.Phrase;
            r.Translation = entity.Translation;
            r.PhraseId = entity.PhraseId;
            r.TranslationLanguageId = entity.TranslationLanguageId;

            return SaveChanges();
        }

        public bool Delete(GlobalExample entity)
        {
            return Delete<GlobalExample>(entity);
        }
    }
}
