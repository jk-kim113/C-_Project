using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Scientia
{
    class Program
    {
        static void Main(string[] args)
        {
            MainDB db = new MainDB();
            //db.Test();
            db.MainLoop();

            string exitOrder = string.Empty;
            do
            {
                exitOrder = Console.ReadLine();

            }
            while (!exitOrder.Equals("Exit"));

            db.ExitProgram();
        }
    }
}
