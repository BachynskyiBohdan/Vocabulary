using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Vocabulary.Web.Models.Admin
{
    public class ParseExamplesViewModel
    {
        [Display(Name = "Enter parse string here. Separation by character '—'")]
        public string ParseString { get; set; }

        public string SelectedTranslation { get; set; }
        public List<SelectListItem> Translations { get; set; }
        public decimal PhraseId { get; set; }

        public ParseExamplesViewModel()
        {
            ParseString = "";
            SelectedTranslation = "";
            Translations = new List<SelectListItem>();
            PhraseId = 0;
        }
    }
}