using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Concrete
{
    public class UsersPhraseRepository : BaseRepository, IUsersPhraseRepository
    {
        public IQueryable<UsersPhrase> UsersPhrases
        {
            get { return DataContext.UsersPhrases; }
        }

        public UsersPhrase Get(Expression<Func<UsersPhrase, bool>> predicate)
        {
            return GetFirst<UsersPhrase>(predicate);
        }

        public UsersPhrase GetById(decimal id)
        {
            return GetFirst<UsersPhrase>(t => t.Id == id);
        }

        public IQueryable<UsersPhrase> GetAll(Expression<Func<UsersPhrase, bool>> predicate)
        {
            return GetAll<UsersPhrase>(predicate);
        }

        public IQueryable<UsersPhrase> GetAll()
        {
            return GetAll<UsersPhrase>();
        }

        public bool Add(UsersPhrase entity)
        {
            return Add<UsersPhrase>(entity);
        }

        public bool Update(UsersPhrase entity)
        {
            var r = GetFirst<UsersPhrase>(t => t.Id == entity.Id);
            if (r == null) return false;
             
            Attach(r);

            r.Frequency = entity.Frequency;
            r.Phrase = entity.Phrase;
            r.PhraseType = entity.PhraseType;
            r.Transcription = entity.Transcription;
            r.LanguageId = entity.LanguageId;
            r.Audio = entity.Audio;
            r.LearningState = r.LearningState;
            r.UserId = entity.UserId;

            DataContext.Entry(r).State = EntityState.Modified;

            return SaveChanges();
        }

        public bool Delete(UsersPhrase entity)
        {
            return Delete<UsersPhrase>(entity);
        }
    }
}
