using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public decimal RoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string UserTag { get; set; }
        public bool IsPremium { get; set; }
        public int MaximumPhrases { get; set; }
        public byte[] IconData { get; set; }
        public string IconMimeType { get; set; }
        public string Description { get; set; }
    }
}

