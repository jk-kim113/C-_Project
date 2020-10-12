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
            Connect,

            ChooseCard,
            CreateRoom,
            EnterRoom,

            end
        }

        public enum eToClient
        {
            CheckConnect,
            ShowRoomInfo,
            ShowUserInfo,

            AfterCreateRoom,
            SuccessEnterRoom,
            FailEnterRoom,

            GameStart,
            NextTurn,
            ChooseInfo,
            ChooseResult,

            GameResult,

            end
        }
    }
}
