using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class UserInfo
    {
        public int _index;
        public long _UUID;
        public string _nickName;
        public int _level;
        public bool _isEmpty;
        public bool _isReady;
        public bool _isFinishReadCard;

        public int[] _pickedCardArr;
        public int _unLockSlotCnt;
        public int[] _rotateInfoArr;

        public bool IsEmptyCardSlot()
        {
            for (int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_pickedCardArr[n] == 0)
                    return true;
            }

            return false;
        }

        public int _nowCardCount
        {
            get
            {
                int cnt = 0;
                for (int n = 0; n < _unLockSlotCnt; n++)
                {
                    if (_pickedCardArr[n] != 0)
                        cnt++;
                }

                return cnt;
            }
        }

        public int AddCard(int cardIndex)
        {
            int emptySlot = -1;
            if (EmptyCardSlotIndex(out emptySlot))
                _pickedCardArr[emptySlot] = cardIndex;

            return emptySlot;
        }

        bool EmptyCardSlotIndex(out int index)
        {
            index = -1;
            for (int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_pickedCardArr[n] == 0)
                {
                    index = n;
                    return true;
                }
            }

            return false;
        }

        public bool IsComplete(out int completeCnt)
        {
            completeCnt = 0;
            for(int n = 0; n < _rotateInfoArr.Length; n++)
            {
                if (_rotateInfoArr[n] >= 4)
                    completeCnt++;
            }

            if (completeCnt >= 1)
                return true;
            else
                return false;
        }
    }
}
