using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Areas.Admin.Models
{
    public class PhraseViewModel
    {
        public GlobalPhrase GlobalPhrase { get; set; }
        [Display(Name = "Phrase type:")]
        [Required]
        public string SelectedPhraseType { get; set; }
        [Display(Name = "Phrase language:")]
        [Required]
        public string SelectedLanguage { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }

        public PhraseViewModel()
        {
            GlobalPhrase = new GlobalPhrase();
            SelectedLanguage = "eng";
            SelectedPhraseType = "Word";
        }
    }
}