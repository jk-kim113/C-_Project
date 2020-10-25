using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_CardBattle
{
    class DefinedProtocol
    {
        public enum eFromServer
        {
            OverlapCheck_ID,
            JoinGame,
            LogIn,
            EnrollUserInfo,

            end
        }

        public enum eToServer
        {
            OverlapCheckResult_ID,
            CompleteJoin,
            LogInResult,

            end
        }
    }
}
