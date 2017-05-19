using System.Collections.Generic;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Areas.Admin.Models
{
    public class GlobalListViewModel
    {
        public IList<GlobalPhrase> GlobalPhrases { get; set; }
        public IList<GlobalTranslation> GlobalTranslations { get; set; }
        public IList<GlobalExample> GlobalExamples { get; set; }
        public IList<Glossary> Glossaries { get; set; }

        public Dictionary<decimal, string> Languages { get; set; }

        public GlobalListViewModel()
        {
            GlobalPhrases = null;
            GlobalTranslations = null;
            GlobalExamples = null;
            Glossaries = null;
            Languages = new Dictionary<decimal, string>();
        }
    }
}