using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class DefinedProtocol
    {
        public enum eFromClient
        {
            LogInTry,

            max
        }

        public enum eToClient
        {
            LogInResult,

            max
        }

        public enum eFromServer
        {
            CheckLogIn,

            max
        }

        public enum eToServer
        {

        }
    }
}
