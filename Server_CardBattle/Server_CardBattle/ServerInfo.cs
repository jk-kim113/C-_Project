using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_CardBattle
{
    class ServerInfo
    {
        public class RoomInfo
        {
            public int _roomNumber;
            public string _name;
            public bool _isLock;
            public string _pw;
            public int _currentMember;
            public List<long> _memberIdx;
            public int _currentOrderIdx;
        }
    }
}
