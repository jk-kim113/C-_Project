using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Scientia
{
    class DefinedProtocol
    {
        public enum eFromServer
        {
            CheckLogIn,
            OverlapCheck_ID,
            CheckEnroll,
            CheckCharacterInfo,

            max
        }

        public enum eToServer
        {
            LogInResult,
            OverlapResult_ID,
            EnrollResult,
            ShowCharacterInfo,
            CompleteCharacterInfo,

            max
        }
    }
}
