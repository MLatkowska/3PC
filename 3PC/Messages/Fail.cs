using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3PC.Messages
{
    class Fail
    {
        public static Fail Instance = new Fail();

        private Fail()
        {
        }
    }
}
