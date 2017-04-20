using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Models.Admin
{
    public class GlossaryViewModel
    {
         public Glossary Glossary { get; set; }
        [Display(Name = "Glossary language:")]
        [Required]
        public string SelectedLanguage { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }

        public GlossaryViewModel()
        {
            Glossary = new Glossary();
            SelectedLanguage = "eng";
        }
    }
}