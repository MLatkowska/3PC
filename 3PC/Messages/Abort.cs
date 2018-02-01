using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3PC.Messages
{
    class Abort
    {
        public static Abort Instance = new Abort();
        private Abort() { }
    }
}
