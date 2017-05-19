using System.Collections.Generic;
using System.Web.Mvc;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Areas.User.Models
{
    public class VocabularyListViewModel
    {
        public List<UsersPhrase> Phrases { get; set; }
        public List<UsersTranslation> Translations { get; set; }

        public List<SelectListItem> Languages { get; set; }

        public VocabularyListViewModel()
        {
            Phrases = new List<UsersPhrase>();
            Translations = new List<UsersTranslation>();

            Languages = new List<SelectListItem>();
        }
    }
}