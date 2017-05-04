using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    public class GlobalExample : BaseExample
    {
        public GlobalExample() : base() { }
        public GlobalExample(GlobalExample e) : base(e) { }
    }
}
