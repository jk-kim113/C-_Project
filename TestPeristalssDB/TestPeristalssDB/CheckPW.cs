using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPeristalssDB
{
    class CheckPW
    {
        const int pwLength = 6;

        static bool IsLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        static bool IsSymbol(char c)
        {
            return c > 32 && c < 127 && !IsDigit(c) && !IsLetter(c);
        }

        public static bool IsValidPassword(string password)
        {
            return
                password.Length >= pwLength &&
                password.Any(c => IsLetter(c)) &&
                password.Any(c => IsDigit(c)) &&
                password.Any(c => IsSymbol(c));
        }
    }
}
