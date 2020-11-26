using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class RoomInfo
    {
        public int _roomNumber;
        public string _name;
        public bool _isLock;
        public string _pw;
        public string _mode;
        public string _rule;
        public int _master;

        public List<UserInfo> _userList;
        public int _currentMemberCnt;

        public CardInfo _cardInfo;
        public int _thisTurn;

        public int EmptyIndex()
        {
            for (int n = 0; n < _userList.Count; n++)
            {
                if (_userList[n]._isEmpty)
                    return n;
            }

            return -1;
        }

        public UserInfo SearchUser(long uuid)
        {
            for (int n = 0; n < _userList.Count; n++)
            {
                if (_userList[n]._UUID == uuid)
                    return _userList[n];
            }

            return null;
        }
    }
}
