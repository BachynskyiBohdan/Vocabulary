using System.Collections.Generic;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Models.Admin
{
    public class FullPhraseViewModel
    {
        public GlobalPhrase Phrase { get; set; }
        public GlobalTranslation Translation { get; set; }
        public List<GlobalExample> Examples { get; set; }

        public FullPhraseViewModel()
        {
            Phrase = new GlobalPhrase();
            Translation = new GlobalTranslation();
            Examples = new List<GlobalExample>();
        }
    }
}