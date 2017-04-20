using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IGlobalPhraseRepository
    {
        IQueryable<GlobalPhrase> GlobalPhrases { get; }
        GlobalPhrase Get(Expression<Func<GlobalPhrase, bool>> predicate);
        GlobalPhrase GetById(decimal id);
        IQueryable<GlobalPhrase> GetAll(Expression<Func<GlobalPhrase, bool>> predicate);
        IQueryable<GlobalPhrase> GetAll();

        bool Add(GlobalPhrase entity);
        bool Update(GlobalPhrase entity);
        bool Delete(GlobalPhrase entity);
    }
}
