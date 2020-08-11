using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PQ_Rewind
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineParser.cmdline = args;
            RewindService.Start();
        }
    }
}
