using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class RoomSort
    {   
        public struct ShowRoom // 32
        {   
            public int _roomNumber;
            public string _name;
            public int _isLock;
            public int _currentMemberCnt;
            public int _maxMemberCnt;
            public string _mode;
            public string _rule;
            public int _isPlay;
        }

        public struct RoomInfo
        {
            public int _roomNumber;
            public string _name;
            public bool _isLock;
            public string _pw;
            public string _mode;
            public string _rule;

            public List<string> _userList;
        }

        Dictionary<string, List<RoomInfo>> _roomInfoDic = new Dictionary<string, List<RoomInfo>>();

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

        public void GetRoomList(ShowRoom[] roomList)
        {
            int k = 0;
            foreach(string key in _roomInfoDic.Keys)
            {
                for(int n = 0; n < _roomInfoDic[key].Count; n++)
                {
                    ShowRoom temp = new ShowRoom();
                    temp._name = _roomInfoDic[key][n]._name;
                    temp._isLock = _roomInfoDic[key][n]._isLock ? 0 : 1;
                    temp._currentMemberCnt = _roomInfoDic[key][n]._userList.Count;
                    temp._maxMemberCnt = 4;
                    temp._mode = _roomInfoDic[key][n]._mode;
                    temp._rule = _roomInfoDic[key][n]._rule;
                    temp._isPlay = 0;

                    roomList[k++] = temp;
                }
            }
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
