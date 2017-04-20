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
    public class GlobalPhraseRepository : BaseRepository, IGlobalPhraseRepository
    {
        public IQueryable<GlobalPhrase> GlobalPhrases
        {
            get { return DataContext.GlobalPhrases; }
        }

        public GlobalPhrase Get(Expression<Func<GlobalPhrase, bool>> predicate)
        {
            return GetFirst<GlobalPhrase>(predicate);
        }

        public GlobalPhrase GetById(decimal id)
        {
            return GetFirst<GlobalPhrase>(t => t.Id == id);
        }

        public IQueryable<GlobalPhrase> GetAll(Expression<Func<GlobalPhrase, bool>> predicate)
        {
            return GetAll<GlobalPhrase>(predicate);
        }

        public IQueryable<GlobalPhrase> GetAll()
        {
            return GetAll<GlobalPhrase>();
        }

        public bool Add(GlobalPhrase entity)
        {
            return Add<GlobalPhrase>(entity);
        }

        public bool Update(GlobalPhrase entity)
        {
            var r = GetFirst<GlobalPhrase>(t => t.Id == entity.Id);
            if (r == null) return false;

            Attach(r);

            r.Frequency = entity.Frequency;
            r.Glossaries = new Collection<Glossary>(entity.Glossaries.ToList());
            r.Phrase = entity.Phrase;
            r.PhraseType = entity.PhraseType;
            r.Transcription = entity.Transcription;
            r.LanguageId = entity.LanguageId;
            r.Audio = entity.Audio;

            return SaveChanges();
        }

        public bool Delete(GlobalPhrase entity)
        {
            return Delete<GlobalPhrase>(entity);
        }
    }
}
