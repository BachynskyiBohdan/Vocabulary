using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Areas.Admin.Models
{
    public class GlossaryViewModel
    {
        public Glossary Glossary { get; set; }
        [Display(Name = "Glossary language:")]
        [Required]
        public string SelectedLanguage { get; set; }
        public IList<SelectListItem> Languages { get; set; }

        public GlossaryViewModel()
        {
            Glossary = new Glossary();
            SelectedLanguage = "";
            Languages = new List<SelectListItem>();
        }
    }
}