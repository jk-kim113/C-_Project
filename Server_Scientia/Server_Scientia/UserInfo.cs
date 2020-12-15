﻿using System;
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

        const int _maxCardSlotCnt = 4;

        int[] _pickedCardArr = new int[_maxCardSlotCnt];
        int _currentCardCnt;
        int _unLockSlotCnt = 2;
        int[] _rotateInfoArr = new int[_maxCardSlotCnt];
        int _flaskCubeCnt;
        bool _isFlaskEffect;
        bool _isSkillEffect;

        public int _CardSlotCnt { get { return _maxCardSlotCnt; } }
        public int[] _PickedCardArr { get { return _pickedCardArr; } }
        public int _UnLockSlotCnt { get { return _unLockSlotCnt; } }
        public int[] _RotateInfoArr { get { return _rotateInfoArr; } }
        public int _NowCardCnt { get { return _currentCardCnt; } }
        public int _FlaskCubeCnt { get { return _flaskCubeCnt; } set { _flaskCubeCnt = value; } }
        public bool _IsFlaskEffect { get { return _isFlaskEffect; } }
        public bool _IsSkillEffect { get { return _isSkillEffect; } }

        int _applyCard;
        bool _isApplyingEffect;
        int _repetitionCnt;

        public int _ApplyCard { get { return _applyCard; } set { _applyCard = value; } }
        public bool _IsApplyingEffect { get { return _isApplyingEffect; } set { _isApplyingEffect = value; } }
        public int _RepetitionCnt { get { return _repetitionCnt; } set { _repetitionCnt = value; } }

        const int _maxSkillMove = 4;
        Dictionary<eCardField, List<SkillCube>> _skillPowerCnt = new Dictionary<eCardField, List<SkillCube>>();

        RoomInfo _room;

        public void InitUserInfo(long uuid, string nickName, int level, RoomInfo room)
        {
            _uuid = uuid;
            _nickName = nickName;
            _level = level;
            _isEmpty = true;
            _currentCardCnt = 0;

            _room = room;

            for (int n = 0; n < (int)eCardField.max; n++)
                _skillPowerCnt.Add((eCardField)n, new List<SkillCube>());
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

        public int DeleteCard(int cardIndex)
        {
            for (int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_pickedCardArr[n] == cardIndex)
                {
                    _pickedCardArr[n] = 0;
                    _rotateInfoArr[n] = 0;
                    _currentCardCnt--;
                    return n;
                }
            }

            return -1;
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

        public void AddCardSlot()
        {
            if (_unLockSlotCnt >= _maxCardSlotCnt)
                return;

            _unLockSlotCnt++;
        }

        public void AddFlaskCube(int add)
        {
            _flaskCubeCnt += add;
        }

        public int MaxSkillPower()
        {
            int temp = int.MinValue;
            int[] skillPower = new int[4];

            foreach(eCardField key in _skillPowerCnt.Keys)
            {
                for(int n = 0; n < _skillPowerCnt[key].Count; n++)
                    skillPower[(int)key] += _skillPowerCnt[key][n]._Position;
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

            foreach(eCardField key in _skillPowerCnt.Keys)
                temp += _skillPowerCnt[key].Count;

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

        public void InitSkillCube()
        {
            foreach(eCardField field in _skillPowerCnt.Keys)
            {
                _skillPowerCnt[field].Add(new SkillCube());
                Console.WriteLine("{0} 유저의 {1} 분야 카드 수 : {2}",_nickName, field, _skillPowerCnt[field].Count);
            }
        }

        public bool MoveSkillCube(eCardField field)
        {
            for(int n = 0; n < _skillPowerCnt[field].Count; n++)
            {
                if (!_skillPowerCnt[field][n]._IsFinish)
                {
                    if(++_skillPowerCnt[field][n]._Position >= _maxSkillMove - _skillPowerCnt[field].Count - 1)
                    {
                        _skillPowerCnt[field][n]._IsFinish = true;

                        if(_room._MaxSkillCube > 0)
                        {
                            _skillPowerCnt[field].Add(new SkillCube());
                            Console.WriteLine("새로운 스킬 큐브를 추가 하였습니다");
                            if (--_room._MaxSkillCube <= 0)
                                _room._IsFinalTurn = true;

                            return true;
                        }   
                    }

                    Console.WriteLine("{0} 분야의 스킬이 {1}로 증가 했습니다", field, _skillPowerCnt[field][n]._Position);
                    break;
                }
            }

            return false;
        }

        public bool IsPhysicsSpecificScore(out int count)
        {
            count = 0;

            for(int n = 0; n < _skillPowerCnt[eCardField.Physics].Count; n++)
            {
                if(_skillPowerCnt[eCardField.Physics][n]._Position == _maxSkillMove - count)
                {
                    count++;
                }
            }

            if (count > 0)
                return true;
            else
                return false;
        }

        public int[] SkillCubePos(eCardField field)
        {
            int[] skillcubePos = new int[5];

            for(int n = 0; n < _skillPowerCnt[field].Count; n++)
            {
                skillcubePos[_skillPowerCnt[field][n]._Position] = 1;
            }

            return skillcubePos;
        }

        public bool CheckNotMostField(int field)
        {
            int mostSkll = MaxSkillPower();

            int cnt = 0;
            foreach(eCardField fieldkey in _skillPowerCnt.Keys)
            {
                if (FieldSkillPower(fieldkey) == mostSkll)
                {
                    cnt++;

                    if (cnt >= _skillPowerCnt.Count)
                        return true;
                }
                else
                    break;
            }

            if (mostSkll > FieldSkillPower((eCardField)field))
                return true;

            return false;
        }

        int FieldSkillPower(eCardField field)
        {
            int temp = 0;
            for (int n = 0; n < _skillPowerCnt[field].Count; n++)
                temp += _skillPowerCnt[field][n]._Position;

            return temp;
        }

        internal class SkillCube
        {
            int _position;
            bool _isFinish;

            public int _Position { get { return _position; } set { _position = value; } }
            public bool _IsFinish { get { return _isFinish; } set { _isFinish = value; } }

            public SkillCube()
            {
                _position = 0;
                _isFinish = false;
            }
        }
    }
}
