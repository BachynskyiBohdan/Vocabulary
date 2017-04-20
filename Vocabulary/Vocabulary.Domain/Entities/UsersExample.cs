using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    public class UsersExample
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public decimal UserPhraseId { get; set; }
        public string Phrase { get; set; }
        public decimal UserTranslationId { get; set; }
        public string Translation { get; set; }

        public byte[] Audio { get; set; }

        public UsersExample()
        {
            UserPhraseId = 0;
            Phrase = "";
            UserTranslationId = 0;
            Translation = "";
            Audio = null;
        }
        public UsersExample(UsersExample e)
        {
            UserPhraseId = e.UserPhraseId;
            Phrase = e.Phrase;
            UserTranslationId = e.UserTranslationId;
            Translation = e.Translation;
            Audio = e.Audio;
        }
        public UsersExample(GlobalExample e)
        {
            UserPhraseId = 0;
            Phrase = e.Phrase;
            UserTranslationId = 0;
            Translation = e.Translation;
            Audio = e.Audio;
        }
    }
}
