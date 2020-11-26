using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class RoomSort
    {
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
