using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnTCPServer
{
    class DefinedProtocol
    {
        public enum eSendMessage
        {
            Chatting_Message = 1,
            Connect_Message,
            Connect_After,
            Disconnect_Message,

            endMessage
        }

        public enum eReceiveMessage
        {
            Connect_User = 0,
            Connect_After,
            Current_User,
            Chatting_Message,
            Disconnect_Server,

            Disconnect_User = 100,

            endMessage
        }
    }
}
