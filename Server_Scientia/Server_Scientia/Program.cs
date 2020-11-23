using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Server_Scientia
{
    class Program
    {
        static void Main(string[] args)
        {
            MainServer server = new MainServer();

            server.MainLoop();

            string exitOrder = string.Empty;
            do
            {
                exitOrder = Console.ReadLine();

                if (exitOrder.Equals("Connect"))
                    server.ConnectDB();
            }
            while (!exitOrder.Equals("Exit"));

            server.ExitProgram();
        }
    }
}
