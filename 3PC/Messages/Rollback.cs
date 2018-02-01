using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3PC.Messages
{
    class Rollback
    {
        public static Rollback Instance = new Rollback();
        private Rollback() { }
    }
}
