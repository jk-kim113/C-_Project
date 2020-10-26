using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_CardBattle
{
    class DefinedProtocol
    {
        public enum eFromClient
        {
            OverlapCheck_ID,
            JoinGame,

            LogIn,
            MyInfo,

            ChooseCard,
            CreateRoom,
            EnterRoom,
            ExitRoom,

            AddAI,

            Ready,
            GameStart,

            end
        }

        public enum eToClient
        {
            CompleteJoin,

            OverlapCheckResult_ID,
            LogInResult,

            ShowRoomInfo,
            ShowUserInfo,

            AfterCreateRoom,
            SuccessEnterRoom,
            FailEnterRoom,

            ShowExit,
            ShowMaster,
            ShowReady,
            ShowAI,
            CanPlay,

            GameStart,
            NextTurn,
            ChooseInfo,
            ChooseResult,

            GameResult,

            end
        }

        public enum eFromServer
        {
            OverlapCheck_ID,
            JoinGame,
            LogIn,
            EnrollUserInfo,

            ExitServer,

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
