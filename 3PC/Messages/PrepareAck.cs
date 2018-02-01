using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3PC.Messages
{
    class PrepareAck
    {
        public static PrepareAck Instance = new PrepareAck();
        private PrepareAck() { }
    }
}
