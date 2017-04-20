using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Models.Admin
{
    public class TranslationViewModel
    {
        public GlobalTranslation GlobalTranslation { get; set; }
        [Display(Name = "Translation language:")]
        [Required]
        public string SelectedLanguage { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }

        public TranslationViewModel()
        {
            GlobalTranslation = new GlobalTranslation();
            SelectedLanguage = "eng";
        }
    }
}