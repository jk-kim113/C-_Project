using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class CardInfo
    {
        Dictionary<eCardField, Card[]> _projectBoard = new Dictionary<eCardField, Card[]>();

        const int _maxFieldCnt = 3;
        int _currentCardCnt = 0;

        public bool _IsEmpty { get { return _currentCardCnt == 0; } }

        public void InitCardDeck()
        {
            for(int n = 0; n < (int)eCardField.max; n++)
                _projectBoard.Add((eCardField)n, new Card[_maxFieldCnt]);
        }

        public void AddCard(eCardField field, int cardIndex)
        {
            for(int n = 0; n < _projectBoard[field].Length; n++)
            {
                if(_projectBoard[field][n] == null)
                    _projectBoard[field][n] = new Card();

                if (_projectBoard[field][n]._IsEmpty)
                {
                    _projectBoard[field][n].Add(cardIndex);
                    _projectBoard[field][n]._IsEmpty = false;
                    break;
                }
            }

            _currentCardCnt++;
        }

        public int FieldCount(eCardField field)
        {
            int temp = 0;
            for(int n = 0; n < _projectBoard[field].Length; n++)
            {
                if (_projectBoard[field][n] != null && !_projectBoard[field][n]._IsEmpty)
                    temp++;
            }

            return temp;
        }

        public bool IsOver()
        {
            foreach (eCardField key in _projectBoard.Keys)
            {
                for(int n = 0; n < _projectBoard[key].Length; n++)
                {
                    if (_projectBoard[key][n] == null || _projectBoard[key][n]._IsEmpty)
                        return false;
                }
            }

            return true;
        }

        public bool IsContain(eCardField field, int cardIndex)
        {
            for (int n = 0; n < _projectBoard[field].Length; n++)
            {
                if (_projectBoard[field][n] != null && _projectBoard[field][n]._CardIndex == cardIndex)
                    return true;
            }

            return false;
        }

        public int[] GetFieldCard(eCardField field)
        {
            int[] temp = new int[_projectBoard[field].Length];

            for (int n = 0; n < temp.Length; n++)
                temp[n] = _projectBoard[field][n]._CardIndex;

            return temp;
        }

        public bool GetFlaskOnCard(eCardField key, int cardIndex, out int flaskCnt)
        {
            flaskCnt = 0;
            for (int n = 0; n < _projectBoard[key].Length; n++)
            {
                if (_projectBoard[key][n]._CardIndex == cardIndex)
                {
                    flaskCnt = _projectBoard[key][n]._FlaskCount;
                    _projectBoard[key][n]._FlaskCount = 0;
                    return true;
                }
            }

            return false;
        }

        public void AddFlaskOnCard(int cardIndex, int flaksCnt)
        {
            foreach (eCardField key in _projectBoard.Keys)
            {
                for (int n = 0; n < _projectBoard[key].Length; n++)
                {
                    if (_projectBoard[key][n]._CardIndex == cardIndex)
                        _projectBoard[key][n]._FlaskCount += flaksCnt;
                }
            }
        }

        public bool PickCard(eCardField field, int cardIndex)
        {
            for (int n = 0; n < _projectBoard[field].Length; n++)
            {
                if (_projectBoard[field][n]._CardIndex == cardIndex)
                {
                    if (_projectBoard[field][n]._CardCount <= 0)
                        return false;
                    else
                        _projectBoard[field][n]._CardCount--;

                    break;
                }
            }

            return true;
        }

        public int CardCount(int cardIndex)
        {
            foreach (eCardField field in _projectBoard.Keys)
            {
                for (int n = 0; n < _projectBoard[field].Length; n++)
                {
                    if (_projectBoard[field][n]._CardIndex == cardIndex)
                        return _projectBoard[field][n]._CardCount;
                }
            }

            return -1;
        }

        public void ReturnCard(eCardField field, int cardIndex)
        {
            for (int n = 0; n < _projectBoard[field].Length; n++)
            {
                if (_projectBoard[field][n]._CardIndex == cardIndex)
                {
                    _projectBoard[field][n]._CardCount++;
                    return;
                }
            }
        }

        internal class Card
        {
            int _cardIndex;
            int _flaskCount;
            int _cardCount;

            public int _CardIndex { get { return _cardIndex; } }
            public int _FlaskCount { get { return _flaskCount; } set { _flaskCount = value; } }
            public int _CardCount { get { return _cardCount; } set { _cardCount = value; } }

            bool _isEmpty;
            public bool _IsEmpty { get { return _isEmpty; } set { _isEmpty = value; } }

            public Card()
            {
                _flaskCount = 0;
                _cardCount = 0;
                _isEmpty = true;
            }

            public void Add(int cardIndex)
            {
                _cardIndex = cardIndex;
                _cardCount = 2;
            }
        }
    }
}
