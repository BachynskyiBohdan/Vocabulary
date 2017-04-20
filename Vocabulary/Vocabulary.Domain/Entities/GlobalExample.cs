using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    //[Table("GlobalExamples")]
    public class GlobalExample
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public decimal GlobalPhraseId { get; set; }
        public string Phrase { get; set; }
        public decimal GlobalTranslationId { get; set; }
        public string Translation { get; set; }

        public byte[] Audio { get; set; }
    }
}
