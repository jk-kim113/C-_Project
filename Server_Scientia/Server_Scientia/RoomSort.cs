using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class RoomSort
    {   
        public class RoomInfo
        {
            public int _roomNumber;
            public string _name;
            public bool _isLock;
            public string _pw;
            public string _mode;
            public string _rule;
            public string _master;

            public List<UserInfo> _userList;
            public int _currentMemberCnt;

            public CardInfo _cardInfo;

            public int EmptyIndex()
            {
                for(int n = 0; n < _userList.Count; n++)
                {
                    if (_userList[n]._isEmpty)
                        return n;
                }

                return -1;
            }
        }

        public class UserInfo
        {
            public int _index;
            public long _UUID;
            public string _nickName;
            public int _level;
            public bool _isEmpty;
            public bool _isReady;
        }

        public class CardInfo
        {
            public Dictionary<eCardField, List<int>> _cardGroup = new Dictionary<eCardField, List<int>>();

            public bool IsReady()
            {
                foreach(eCardField key in _cardGroup.Keys)
                {
                    if (_cardGroup[key].Count != 3)
                        return false;
                }

                return true;
            }
        }

        Dictionary<string, List<RoomInfo>> _roomInfoDic = new Dictionary<string, List<RoomInfo>>();
        public Dictionary<string, List<RoomInfo>> _RoomList { get { return _roomInfoDic; } }

        public void CreateRoom(string mode, RoomInfo room)
        {
            if(_roomInfoDic.ContainsKey(mode))
            {
                _roomInfoDic[mode].Add(room);
            }
            else
            {
                List<RoomInfo> temp = new List<RoomInfo>();
                temp.Add(room);

                _roomInfoDic.Add(mode, temp);
            }

            HeapSort(mode);
        }

        public RoomInfo GetRoom(int roomNum)
        {
            foreach(string key in _roomInfoDic.Keys)
            {
                for(int n = 0; n < _roomInfoDic[key].Count; n++)
                {
                    if (_roomInfoDic[key][n]._roomNumber == roomNum)
                        return _roomInfoDic[key][n];
                }
            }

            return new RoomInfo();
        }

        void HeapSort(string mode)
        {
            for (int n = (_roomInfoDic[mode].Count - 1) / 2; n >= 0; --n)
            {
                UpHeap(mode, n, _roomInfoDic[mode].Count);
            }

            for(int n = _roomInfoDic[mode].Count - 1; n > 0; --n)
            {
                RoomInfo temp = _roomInfoDic[mode][n];
                _roomInfoDic[mode][n] = _roomInfoDic[mode][0];
                _roomInfoDic[mode][0] = temp;
                UpHeap(mode, 0, n);
            }
        }

        void UpHeap(string mode, int root, int max)
        {
            while(root < max)
            {
                int child = root * 2 + 1;
                if (child + 1 < max && _roomInfoDic[mode][child]._userList.Count > _roomInfoDic[mode][child + 1]._userList.Count)
                    ++child;

                if (child < max && _roomInfoDic[mode][root]._userList.Count > _roomInfoDic[mode][child]._userList.Count)
                {
                    RoomInfo temp = _roomInfoDic[mode][root];
                    _roomInfoDic[mode][root] = _roomInfoDic[mode][child];
                    _roomInfoDic[mode][child] = temp;
                }
                else
                    break;
            }
        }
    }
}
