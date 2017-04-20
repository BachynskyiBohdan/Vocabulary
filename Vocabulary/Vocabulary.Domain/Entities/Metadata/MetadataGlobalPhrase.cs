using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities.Metadata
{
    public partial class MetadataGlobalPhrase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public decimal LanguageId { get; set; }
        [Required]
        public string Phrase { get; set; }
        public string Transcription { get; set; }
        public int? Frequency { get; set; }
        public PhraseType PhraseType { get; set; }
        public byte[] Audio { get; set; }

        public virtual ICollection<Glossary> Glossaries { get; set; }
    }
}
