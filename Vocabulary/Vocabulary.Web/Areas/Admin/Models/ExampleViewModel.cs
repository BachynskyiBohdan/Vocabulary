using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Areas.Admin.Models
{
    public class ExampleViewModel
    {
        public GlobalExample GlobalExample { get; set; }
        [Display(Name = "Translation:")]
        [Required]
        public string SelectedTranslation { get; set; }
        public IEnumerable<SelectListItem> Translations { get; set; }

        public ExampleViewModel()
        {
            GlobalExample = new GlobalExample();
            SelectedTranslation = "eng";
        }
    }
}