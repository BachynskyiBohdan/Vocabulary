using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    public class EFDbContext : DbContext
    {
        public DbSet<GlobalPhrase> GlobalPhrases { get; set; }
        public DbSet<GlobalExample> GlobalExamples { get; set; }
        public DbSet<GlobalTranslation> GlobalTranslations { get; set; }
        public DbSet<Glossary> Glossaries { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UsersPhrase> UsersPhrases { get; set; }
        public DbSet<UsersExample> UsersExamples { get; set; }
        public DbSet<UsersTranslation> UsersTranslations { get; set; }

        public EFDbContext() : base("VocabularyDb") { }
    }
}
