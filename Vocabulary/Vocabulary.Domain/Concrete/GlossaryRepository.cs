using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Domain.Concrete
{
    public class GlossaryRepository : BaseRepository, IGlossaryRepository
    {
        public IQueryable<Glossary> Glossaries
        {
            get { return DataContext.Glossaries; }
        }

        public Glossary Get(Expression<Func<Glossary, bool>> predicate)
        {
            return GetFirst<Glossary>(predicate);
        }

        public Glossary GetById(decimal id)
        {
            return GetFirst<Glossary>(t => t.Id == id);
        }

        public IQueryable<Glossary> GetAll(Expression<Func<Glossary, bool>> predicate)
        {
            return GetAll<Glossary>(predicate);
        }
        public IQueryable<Glossary> GetAll()
        {
            return GetAll<Glossary>();
        }

        public bool Add(Glossary entity)
        {
            return Add<Glossary>(entity);
        }

        public bool Update(Glossary entity)
        {
            var r = GetFirst<Glossary>(t => t.Id == entity.Id);
            if (r == null) return false;

            Attach(r);

            r.Name = entity.Name;
            r.LanguageId = entity.LanguageId;
            r.Icon = entity.Icon;
            r.GlobalPhrases = entity.GlobalPhrases;//new Collection<GlobalPhrase>(entity.GlobalPhrases.ToList());

            return SaveChanges();
        }

        public bool Delete(Glossary entity)
        {
            return Delete<Glossary>(entity);
        }

        public new bool SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
