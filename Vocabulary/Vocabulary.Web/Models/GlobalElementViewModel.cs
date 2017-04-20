using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Models
{
    public class GlobalElementViewModel
    {
        public GlobalPhrase GlobalPhrase { get; set; }
        public GlobalTranslation GlobalTranslation { get; set; }
        public GlobalExample GlobalExample { get; set; }

        [Display(Name = "Translation language:")]
        [Required]
        public string SelectedLanguage { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }

        public GlobalElementViewModel()
        {
            GlobalPhrase = null;
            GlobalTranslation = null;
            GlobalExample = null;
            SelectedLanguage = "eng";
        }
    }
}