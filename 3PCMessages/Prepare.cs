using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3PC.Messages
{
    public class Prepare
    {
        public static Prepare Instance = new Prepare();
        private Prepare() { }
    }
}
