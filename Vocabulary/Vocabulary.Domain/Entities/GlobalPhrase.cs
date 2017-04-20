using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vocabulary.Domain.Entities.Metadata;

namespace Vocabulary.Domain.Entities
{
    [MetadataType(typeof(MetadataGlobalPhrase))]
    public partial class GlobalPhrase
    {
        public decimal Id { get; set; }
        public decimal LanguageId { get; set; }
        public string Phrase { get; set; }
        public string Transcription { get; set; }
        public int? Frequency { get; set; }
        public PhraseType PhraseType { get; set; }
        public byte[] Audio { get; set; }

        public virtual ICollection<Glossary> Glossaries { get; set; }

        public GlobalPhrase()
        {
            Id = 0m;
            LanguageId = 1m;
            Phrase = "";
            Transcription = "";
            Frequency = null;
            PhraseType = PhraseType.Word;
            Audio = null;
            Glossaries = new Collection<Glossary>();
        }

        public static PhraseType ParseType(string type)
        {
            PhraseType phraseType;
            Enum.TryParse(type, out phraseType);
            return phraseType;
        }
    }

    public enum PhraseType
    {
        Word = 0,
        Phrase = 1,
        Sentence = 2,
        All = 3
    }
}
