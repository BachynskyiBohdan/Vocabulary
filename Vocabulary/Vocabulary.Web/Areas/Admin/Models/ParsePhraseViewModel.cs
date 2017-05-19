using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Vocabulary.Web.Areas.Admin.Models
{
    public class ParsePhraseViewModel
    {
        public string PhraseLanguage { get; set; }
        public string TranslationLanguage { get; set; }
        public IList<SelectListItem> Languages { get; set; }
        [Display(Name = "Enter parse string here. Separation by character '—'")]
        public string ParseString { get; set; }

        public decimal? GlossaryId { get; set; }

        public ParsePhraseViewModel()
        {
            Languages = new List<SelectListItem>();
            GlossaryId = null;
        }
    }
}
