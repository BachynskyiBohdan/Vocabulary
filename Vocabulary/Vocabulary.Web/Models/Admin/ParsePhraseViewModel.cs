using System.Collections.Generic;
using System.Web.Mvc;

namespace Vocabulary.Web.Models.Admin
{
    public class ParsePhraseViewModel
    {
        public string PhraseLanguage { get; set; }
        public string TranslationLanguage { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }
        public string WebAddresses { get; set; }
    }
}
