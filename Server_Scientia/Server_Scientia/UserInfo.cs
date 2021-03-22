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
        int _characterIndex;
        int _level;

        public int _Index { get { return _index; }}
        public long _UUID { get { return _uuid; } }
        public string _NickName { get { return _nickName; } }
        public int _CharacterIndex { get { return _characterIndex; } }
        public int _Level { get { return _level; } }

        bool _isEmpty = true;
        bool _isReady;
        bool _isFinishReadCard;

        public bool _IsEmpty { get { return _isEmpty; } }
        public bool _IsReady { get { return _isReady; } set { _isReady = value; } }
        public bool _IsFinishReadCard { get { return _isFinishReadCard; } set { _isFinishReadCard = value; } }

        const int _maxCardSlotCnt = 4;

        MyCard[] _myPickCard = new MyCard[_maxCardSlotCnt];
        int _currentCardCnt;
        int _unLockSlotCnt = 2;
        int _flaskCubeCnt;
        bool _isFlaskEffect;
        bool _isSkillEffect;
        int[] _flaskOnCard = new int[_maxCardSlotCnt];

        public int _CardSlotCnt { get { return _maxCardSlotCnt; } }
        public MyCard[] _MyPickCard { get { return _myPickCard; } }
        public int _UnLockSlotCnt { get { return _unLockSlotCnt; } }
        public int _NowCardCnt { get { return _currentCardCnt; } }
        public int _FlaskCubeCnt { get { return _flaskCubeCnt; } set { _flaskCubeCnt = value; } }
        public bool _IsFlaskEffect { get { return _isFlaskEffect; } }
        public bool _IsSkillEffect { get { return _isSkillEffect; } }
        public int[] _FlaskOnCard { get { return _flaskOnCard; } }

        int _applyCard;
        bool _isApplyingEffect;
        int _repetitionCnt;
        bool _isSecondEffect;
        bool _isFinishGameOver;

        public int _ApplyCard { get { return _applyCard; } set { _applyCard = value; } }
        public bool _IsApplyingEffect { get { return _isApplyingEffect; } set { _isApplyingEffect = value; } }
        public int _RepetitionCnt { get { return _repetitionCnt; } set { _repetitionCnt = value; } }
        public bool _IsSecondEffect { get { return _isSecondEffect; } set { _isSecondEffect = value; } }
        public bool _IsFinishGameOver { get { return _isFinishGameOver; } set { _isFinishGameOver = value; } }

        int _currentPhysicsEffectIndex;
        int[] _physicsEffectField = new int[2];
        Dictionary<eCardField, int> _completeCountDic = new Dictionary<eCardField, int>();
        public Dictionary<eCardField, int> _CompleteCountDic { get { return _completeCountDic; } }

        const int _maxSkillMove = 4;
        Dictionary<eCardField, List<SkillCube>> _skillCubeTrack = new Dictionary<eCardField, List<SkillCube>>();

        int _gameScore;
        public int _GameScore { get { return _gameScore; } }

        public UserInfo()
        {
            _isEmpty = true;
        }

        public void InitUserInfo(long uuid, string nickName, int characterIndex, int level, int index)
        {
            _index = index;
            _uuid = uuid;
            _nickName = nickName;
            _characterIndex = characterIndex;
            _level = level;

            _isEmpty = false;
            _currentCardCnt = 0;

            for (int n = 0; n < (int)eCardField.max; n++)
            {
                _completeCountDic.Add((eCardField)n, 0);
                _skillCubeTrack.Add((eCardField)n, new List<SkillCube>());
            }

            for (int n = 0; n < _myPickCard.Length; n++)
            {
                if (_myPickCard[n] == null)
                    _myPickCard[n] = new MyCard();
            }
        }

        public void GameStart()
        {
            for(int n = 0; n < _myPickCard.Length; n++)
                _myPickCard[n].InitMyCard();

            foreach (eCardField field in _skillCubeTrack.Keys)
            {
                _skillCubeTrack[field].Clear();
                _skillCubeTrack[field].Add(new SkillCube());
            }   
        }

        public bool IsEmptyCardSlot()
        {
            for (int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_myPickCard[n]._IsEmpty)
                    return true;
            }

            return false;
        }

        public int AddCard(int cardIndex)
        {
            int emptySlot = -1;
            if (EmptyCardSlotIndex(out emptySlot))
            {
                _myPickCard[emptySlot].Add(cardIndex);
                _currentCardCnt++;
            }   

            return emptySlot;
        }

        bool EmptyCardSlotIndex(out int index)
        {
            index = -1;
            for (int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_myPickCard[n]._IsEmpty)
                {
                    index = n;
                    return true;
                }
            }

            return false;
        }

        public int DeleteCard(int cardIndex)
        {
            for (int n = 0; n < _unLockSlotCnt; n++)
            {
                if (!_myPickCard[n]._IsEmpty && _myPickCard[n]._CardIndex == cardIndex)
                {
                    _myPickCard[n].InitMyCard();
                    _currentCardCnt--;
                    return n;
                }
            }

            return -1;
        }

        public void CompleteCard(eCardField field)
        {
            _completeCountDic[field]++;
        }

        public void SelectPhysicsEffectField(int field)
        {
            _physicsEffectField[_currentPhysicsEffectIndex++] = field;
        }

        public bool IsHaveCard(int cardIndex)
        {
            for(int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_myPickCard[n]._CardIndex == cardIndex)
                    return true;
            }

            return false;
        }

        public bool IsComplete(out int completeCnt)
        {
            completeCnt = 0;
            for(int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_myPickCard[n]._RotateInfo >= 4)
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

        public void AddFlaskOnCard(int cardIndex, int cnt)
        {
            for (int n = 0; n < _unLockSlotCnt; n++)
            {
                if (_myPickCard[n]._CardIndex == cardIndex)
                    _flaskOnCard[n] += cnt;
            }
        }

        public int MaxSkillPower()
        {
            int temp = int.MinValue;
            int[] skillPower = new int[4];

            foreach(eCardField key in _skillCubeTrack.Keys)
            {
                for(int n = 0; n < _skillCubeTrack[key].Count; n++)
                    skillPower[(int)key] += _skillCubeTrack[key][n]._Position;
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

            foreach(eCardField key in _skillCubeTrack.Keys)
                temp += _skillCubeTrack[key].Count;

            return temp;
        }

        public int MyCardCount()
        {
            int temp = 0;
            for(int n = 0; n < _myPickCard.Length; n++)
            {
                if (!_myPickCard[n]._IsEmpty)
                    temp++;
            }

            return temp;
        }

        public int GetCompleteCard()
        {
            for(int n = 0; n < _myPickCard.Length; n++)
            {
                if (_myPickCard[n]._RotateInfo >= 4)
                    return _myPickCard[n]._CardIndex;
            }

            return 0;
        }

        public bool MoveSkillCube(eCardField field, RoomInfo room)
        {
            for(int n = 0; n < _skillCubeTrack[field].Count; n++)
            {
                if (!_skillCubeTrack[field][n]._IsFinish)
                {
                    if(++_skillCubeTrack[field][n]._Position >= _maxSkillMove - _skillCubeTrack[field].Count - 1)
                    {
                        _skillCubeTrack[field][n]._IsFinish = true;

                        if(room._MaxSkillCube > 0)
                        {
                            _skillCubeTrack[field].Add(new SkillCube());
                            Console.WriteLine("새로운 스킬 큐브를 추가 하였습니다");
                            if (--room._MaxSkillCube <= 0)
                                room._IsFinalTurn = true;

                            return true;
                        }   
                    }

                    Console.WriteLine("{0} 분야의 스킬이 {1}로 증가 했습니다", field, _skillCubeTrack[field][n]._Position);
                    break;
                }
            }

            return false;
        }

        public bool IsPhysicsSpecificScore(out int count)
        {
            count = 0;

            for(int n = 0; n < _skillCubeTrack[eCardField.Physics].Count; n++)
            {
                if(_skillCubeTrack[eCardField.Physics][n]._Position == _maxSkillMove - count)
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

            for(int n = 0; n < _skillCubeTrack[field].Count; n++)
            {
                skillcubePos[_skillCubeTrack[field][n]._Position] = 1;
            }

            return skillcubePos;
        }

        public bool CheckNotMostField(int field)
        {
            int mostSkll = MaxSkillPower();

            int cnt = 0;
            foreach(eCardField fieldkey in _skillCubeTrack.Keys)
            {
                if (FieldSkillPower(fieldkey) == mostSkll)
                {
                    cnt++;

                    if (cnt >= _skillCubeTrack.Count)
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
            for (int n = 0; n < _skillCubeTrack[field].Count; n++)
                temp += _skillCubeTrack[field][n]._Position;

            return temp;
        }

        public void CaculateScore()
        {
            _gameScore = _flaskCubeCnt + SkillCubeScore() + PhysicsEffectScore() + ChemistryEffectScore() + BiologyEffectScore() + AstronomyEffectScore();
        }

        int SkillCubeScore()
        {
            int temp = 0;
            for(int n = 0; n < (int)eCardField.max; n++)
            {
                for (int m = 0; n < _skillCubeTrack[(eCardField)n].Count; m++)
                    temp += _skillCubeTrack[(eCardField)n][m]._Position;
            }

            return temp;
        }

        int PhysicsEffectScore()
        {
            int temp = 0;
            for(int n = 0; n < _currentPhysicsEffectIndex; n++)
                temp += FieldSkillPower((eCardField)_physicsEffectField[n]);

            return temp;
        }

        int ChemistryEffectScore()
        {
            int temp = 0;

            int position = 4;
            for(int n = 0; n < _skillCubeTrack[eCardField.Chemistry].Count; n++)
            {
                if (n >= 2)
                    break;

                if (_skillCubeTrack[eCardField.Chemistry][n]._Position == position--)
                    temp += AllSkillCubeCount();
                else
                    break;
            }   

            return temp;
        }

        int BiologyEffectScore()
        {
            int temp = 0;

            int position = 4;
            for (int n = 0; n < _skillCubeTrack[eCardField.Biology].Count; n++)
            {
                if (n >= 2)
                    break;

                if (_skillCubeTrack[eCardField.Biology][n]._Position == position--)
                    temp += 7;
                else
                    break;
            }

            return temp;
        }

        int AstronomyEffectScore()
        {
            int temp = 0;

            int position = 4;
            for (int n = 0; n < _skillCubeTrack[eCardField.Astronomy].Count; n++)
            {
                if (n >= 2)
                    break;

                if (_skillCubeTrack[eCardField.Astronomy][n]._Position == position--)
                    temp += _flaskCubeCnt / 3;
                else
                    break;
            }

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

        internal class MyCard
        {
            int _cardIndex;
            int _rotateInfo;
            int _flaskCount;
            bool _isEmpty;

            public int _CardIndex { get { return _cardIndex; } }
            public int _RotateInfo { get { return _rotateInfo; } set { _rotateInfo = value; } }
            public bool _IsEmpty { get { return _isEmpty; } }

            public MyCard()
            {
                InitMyCard();
            }

            public void InitMyCard()
            {
                _cardIndex = 0;
                _rotateInfo = 0;
                _flaskCount = 0;
                _isEmpty = true;
            }

            public void Add(int cardIndex)
            {
                _cardIndex = cardIndex;
                _isEmpty = false;
            }
        }
    }
}
