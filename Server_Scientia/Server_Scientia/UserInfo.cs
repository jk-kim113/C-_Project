using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class UserInfo
    {
        int _index;
        long _uuid;
        string _nickName;
        int _level;

        public int _Index { get { return _index; } set { _index = value; } }
        public long _UUID { get { return _uuid; } }
        public string _NickName { get { return _nickName; } }
        public int _Level { get { return _level; } }

        bool _isEmpty = true;
        bool _isReady;
        bool _isFinishReadCard;

        public bool _IsEmpty { get { return _isEmpty; } set { _isEmpty = value; } }
        public bool _IsReady { get { return _isReady; } set { _isReady = value; } }
        public bool _IsFinishReadCard { get { return _isFinishReadCard; } set { _isFinishReadCard = value; } }

        const int _cardSlotCnt = 4;

        int[] _pickedCardArr = new int[_cardSlotCnt];
        int _currentCardCnt;
        int _unLockSlotCnt = 2;
        int[] _rotateInfoArr = new int[_cardSlotCnt];
        int _flaskCubeCnt;

        public int _CardSlotCnt { get { return _cardSlotCnt; } }
        public int[] _PickedCardArr { get { return _pickedCardArr; } }
        public int[] _RotateInfoArr { get { return _rotateInfoArr; } }
        public int _NowCardCnt { get { return _currentCardCnt; } }
        public int _FlaskCubeCnt { get { return _flaskCubeCnt; } }
        
        Dictionary<int, int[]> _skillPowerCnt = new Dictionary<int, int[]>();

        public void InitUserInfo(long uuid, string nickName, int level)
        {
            _uuid = uuid;
            _nickName = nickName;
            _level = level;
            _isEmpty = true;
            _currentCardCnt = 0;
        }

        public bool IsEmptyCardSlot()
        {
            for (int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_pickedCardArr[n] == 0)
                    return true;
            }

            return false;
        }

        public int AddCard(int cardIndex)
        {
            int emptySlot = -1;
            if (EmptyCardSlotIndex(out emptySlot))
            {
                _pickedCardArr[emptySlot] = cardIndex;
                _currentCardCnt++;
            }   

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

        public void AddFlaskCube(int add)
        {
            _flaskCubeCnt += add;
        }

        public int MaxSkillPower()
        {
            int temp = int.MinValue;
            int[] skillPower = new int[4];

            foreach(int key in _skillPowerCnt.Keys)
            {
                for(int n = 0; n < _skillPowerCnt[key].Length; n++)
                {
                    if (_skillPowerCnt[key][n] != 0)
                        skillPower[key] += n;
                }
            }

            for(int n = 0; n < skillPower.Length; n++)
            {
                if (skillPower[n] > temp)
                    temp = skillPower[n];
            }

            return temp;
        }

        public int AllSkillCubeCount()
        {
            int temp = 0;

            foreach(int key in _skillPowerCnt.Keys)
            {
                for(int n = 0; n < _skillPowerCnt[key].Length; n++)
                {
                    if (_skillPowerCnt[key][n] != 0)
                        temp++;
                }
            }    

            return temp;
        }

        public int MyCardCount()
        {
            int temp = 0;
            for(int n = 0; n < _pickedCardArr.Length; n++)
            {
                if (_pickedCardArr[n] != 0)
                    temp++;
            }

            return temp;
        }

        public int GetCompleteCard()
        {
            for(int n = 0; n < _rotateInfoArr.Length; n++)
            {
                if (_rotateInfoArr[n] >= 4)
                    return _pickedCardArr[n];
            }

            return 0;
        }
    }
}
