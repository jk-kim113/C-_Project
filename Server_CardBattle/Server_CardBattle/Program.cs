using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_CardBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            //MainServer server = new MainServer();
            UpgradeServer server = new UpgradeServer();
            
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
