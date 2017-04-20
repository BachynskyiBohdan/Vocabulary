using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Abstract
{
    public interface IGlossaryRepository
    {
        IQueryable<Glossary> Glossaries { get; }
        Glossary Get(Expression<Func<Glossary, bool>> predicate);
        Glossary GetById(decimal id);
        IQueryable<Glossary> GetAll(Expression<Func<Glossary, bool>> predicate);
        IQueryable<Glossary> GetAll();

        bool Add(Glossary entity);
        bool Update(Glossary entity);
        bool Delete(Glossary entity);

        bool SaveChanges();
    }
}
