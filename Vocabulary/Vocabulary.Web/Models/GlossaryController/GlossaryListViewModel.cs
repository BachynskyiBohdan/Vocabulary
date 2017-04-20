using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Models.GlossaryController
{
    public class GlossaryListViewModel
    {
        public IList<Glossary> Glossaries { get; set; }

        [Display(Name = "Choose translation language:")]
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