using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.App_LocalResources;

namespace Vocabulary.Web.Areas.User.Models
{
    public class GlossaryListViewModel
    {
        public IList<Glossary> Glossaries { get; set; }

        [Display(Name = "ChooseTranslationLanguage", ResourceType = typeof(GlobalRes))]
        public string SelectedLanduage { get; set; }

        public Dictionary<string, string> Languages { get; set; }

        public GlossaryListViewModel()
        {
            Glossaries = new List<Glossary>();
            SelectedLanduage = "3";
            Languages = new Dictionary<string, string>();
        }
    }
}