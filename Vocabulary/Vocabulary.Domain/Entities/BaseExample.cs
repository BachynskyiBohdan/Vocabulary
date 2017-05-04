using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    public class BaseExample
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public decimal PhraseId { get; set; }
        public string Phrase { get; set; }
        public decimal TranslationLanguageId { get; set; }
        public string Translation { get; set; }

        [NotMapped]
        public bool IsUsersExample;

        public byte[] Audio { get; set; }

        public BaseExample()
        {
            PhraseId = 0;
            Phrase = "";
            TranslationLanguageId = 0;
            Translation = "";
            Audio = null;
        }
        public BaseExample(BaseExample e)
        {
            PhraseId = e.PhraseId;
            Phrase = e.Phrase;
            TranslationLanguageId = e.TranslationLanguageId;
            Translation = e.Translation;
            Audio = e.Audio;
        }
    }
}
