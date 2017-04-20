using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IGlobalExampleRepository
    {
        IQueryable<GlobalExample> GlobalExamples { get; }
        GlobalExample Get(Expression<Func<GlobalExample, bool>> predicate);
        GlobalExample GetById(decimal id);
        IQueryable<GlobalExample> GetAll(Expression<Func<GlobalExample, bool>> predicate);
        IQueryable<GlobalExample> GetAll();

        bool Add(GlobalExample entity);
        bool Update(GlobalExample entity);
        bool Delete(GlobalExample entity);
    }
}
