using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Remote;
using Akka.Configuration;
using System.Configuration;

namespace _3PC.DeployTarget
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("DeployTarget"))
            {
                Console.ReadKey();
            }
        }
    }
}
