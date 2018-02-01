using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3PC.Messages
{
    class AgreeRequest
    {
        public static AgreeRequest Instance = new AgreeRequest();
        private AgreeRequest() { }
    }
}
