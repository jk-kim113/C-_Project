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
            UpgradeServer server = new UpgradeServer();
            
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
