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
            public long _masterUUID;
            public int _currentMember;

            public long[] _slot;
            public List<CardBattleAI> _AI;
            public int _readyCount;
            public int _currentOrder;

            public DateTime _timeStart;
            public DateTime _timeEnd;
        }

        public class UserInfo
        {
            public long _UUID;
            public string _nickName;
            public int _avatarIndex;
        }
    }
}
