using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    //[MetadataType(typeof(MetadataGlossary))]
    public class Glossary
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public decimal LanguageId { get; set; }
        public string Name { get; set; }
        public byte[] Icon { get; set; }
        public virtual ICollection<GlobalPhrase> GlobalPhrases { get; set; }
        [NotMapped]
        public int Count { get; set; }
    }
}
