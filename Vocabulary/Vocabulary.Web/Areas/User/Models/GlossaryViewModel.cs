using System.Collections.Generic;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Areas.User.Models
{
    public class GlossaryViewModel
    {
        public Glossary Glossary { get; set; }
        public List<GlobalTranslation> Translations { get; set; }

        public List<bool> StateList { get; set; }

        public GlossaryViewModel()
        {
            Glossary = new Glossary();
            Translations = new List<GlobalTranslation>();
            StateList = new List<bool>();
        }
    }
}