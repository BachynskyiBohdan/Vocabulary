using System.Collections.Generic;
using System.Web.Mvc;

namespace Vocabulary.Web.Models.Admin
{
    public class ParsePhraseViewModel
    {
        public string PhraseLanguage { get; set; }
        public string TranslationLanguage { get; set; }
        public IList<SelectListItem> Languages { get; set; }
        public string ParseString { get; set; }

        public ParsePhraseViewModel()
        {
            Languages = new List<SelectListItem>();
        }
    }
}
