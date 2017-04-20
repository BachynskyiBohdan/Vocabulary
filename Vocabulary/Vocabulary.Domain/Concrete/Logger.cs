using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Concrete
{
    public class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine("EF message: {0}", message);
        }
    }
}
