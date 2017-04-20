using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    public class UsersTranslation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public string TranslationPhrase { get; set; }
        public decimal LanguageId { get; set; }
        public decimal UserPhraseId { get; set; }

        public UsersTranslation()
        {
            Id = 0;
            TranslationPhrase = "";
            LanguageId = 1;
            UserPhraseId = 0;
        }
        public UsersTranslation(GlobalTranslation t)
        {
            TranslationPhrase = t.TranslationPhrase;
            LanguageId = t.LanguageId;
        }
    }
}
