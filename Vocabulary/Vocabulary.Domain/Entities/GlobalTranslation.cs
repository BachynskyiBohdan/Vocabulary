using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    //[Table("GlobalTranslations")]
    public class GlobalTranslation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public string TranslationPhrase { get; set; }
        public decimal LanguageId { get; set; }
        public decimal GlobalPhraseId { get; set; }
    }
}
