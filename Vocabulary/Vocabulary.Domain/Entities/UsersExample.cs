using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    public class UsersExample : BaseExample
    {
        public UsersExample() : base() { }
        public UsersExample(BaseExample e) : base(e) {}
        public UsersExample(GlobalExample e)
        {
            PhraseId = 0;
            Phrase = e.Phrase;
            TranslationLanguageId = 0;
            Translation = e.Translation;
            Audio = e.Audio;
        }
    }
}
