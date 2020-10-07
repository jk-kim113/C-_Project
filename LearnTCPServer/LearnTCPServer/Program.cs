using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LearnTCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPServer server = new TCPServer();
            server.MainLoop();

            string exitOrder = string.Empty;
            do
            {
                exitOrder = Console.ReadLine();
            }
            while (!exitOrder.Equals("Exit"));

            server.ExitProgram();
        }
    }
}
