using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelToJsonConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the Excel file name : ");
            string excelName = Console.ReadLine();

            ConvertManager cm = new ConvertManager(excelName);
            cm.Play();

        }
    }
}
