using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class RoomInfo
    {
        int _roomNumber;
        string _name;
        bool _isLock;
        string _pw;
        string _mode;
        string _rule;
        int _master;
        bool _isPlay;
        
        public int _RoomNumber { get { return _roomNumber; } }
        public string _Name { get { return _name; } }
        public bool _IsLock { get { return _isLock; } }
        public string _Mode { get { return _mode; } }
        public string _Rule { get { return _rule; } }
        public int _Master { get { return _master; } }
        public bool _IsPlay { get { return _isPlay; } set { _isPlay = value; } }

        const int _userCnt = 4;

        UserInfo[] _userArr = new UserInfo[_userCnt];
        int _currentMemberCnt;
        int _thisTurn;
        int _maxSkillCube;
        int _maxFlaskCube;
        bool _isFinalTurn;

        public UserInfo[] _UserArr { get { return _userArr; } }
        public int _NowMemeberCnt { get { return _currentMemberCnt; } }
        public int _ThisTurn { get { return _thisTurn; } set { _thisTurn = value; } }
        public int _MaxSkillCube { get { return _maxSkillCube; } set { _maxSkillCube = value; } }
        public int _MaxFlaskCube { get { return _maxFlaskCube; } set { _maxFlaskCube = value; } }
        public bool _IsFinalTurn { get { return _isFinalTurn; } set { _isFinalTurn = value; } }

        DateTime _gameStartTime;
        public DateTime _GameStartTime { get { return _gameStartTime; } set { _gameStartTime = value; } }

        CardInfo _cardInfo = new CardInfo();

        public CardInfo _CardDeck { get { return _cardInfo; } }
        public bool _IsCardEmpty { get { return _cardInfo._IsEmpty; } }

        public void InitRoomInfo(int roomNumber, string name, bool isLock, string pw, string mode, string rule)
        {   
            _roomNumber = roomNumber;
            _name = name;
            _isLock = isLock;
            _pw = pw;
            _mode = mode;
            _rule = rule;
            _currentMemberCnt = 0;
            _master = 0;

            _cardInfo.InitCardDeck();
        }

        public void AddUser(UserInfo user)
        {
            for(int n = 0; n < _userArr.Length; n++)
            {
                if(_userArr[n] == null || _userArr[n]._IsEmpty)
                {
                    _userArr[n] = user;
                    _userArr[n]._Index = n;
                    _userArr[n]._IsEmpty = false;
                    _currentMemberCnt++;
                    break;
                }  
            }
        }

        public UserInfo SearchUser(long uuid)
        {
            for (int n = 0; n < _userArr.Length; n++)
            {
                if (_userArr[n]._UUID == uuid)
                    return _userArr[n];
            }

            return null;
        }

        public bool CheckAllFinishGameOver()
        {
            for(int n = 0; n < _userArr.Length; n++)
            {
                if(_userArr[n] != null && !_userArr[n]._IsEmpty)
                {
                    if (!_userArr[n]._IsFinishGameOver)
                        return false;
                }
            }

            return true;
        }
    }
}
