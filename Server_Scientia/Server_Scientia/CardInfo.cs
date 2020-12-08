using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class CardInfo
    {
        Dictionary<eCardField, Card[]> _cardDeck = new Dictionary<eCardField, Card[]>();

        const int _maxFieldCnt = 3;
        int _currentCardCnt = 0;

        public bool _IsEmpty { get { return _currentCardCnt == 0; } }

        public void InitCardDeck()
        {
            for(int n = 0; n < (int)eCardField.max; n++)
                _cardDeck.Add((eCardField)n, new Card[_maxFieldCnt]);
        }

        public void AddCard(eCardField field, int cardIndex)
        {
            for(int n = 0; n < _cardDeck[field].Length; n++)
            {
                if(_cardDeck[field][n] == null)
                    _cardDeck[field][n] = new Card();

                if (_cardDeck[field][n]._IsEmpty)
                {
                    _cardDeck[field][n].Add(cardIndex);
                    _cardDeck[field][n]._IsEmpty = false;
                    break;
                }
            }

            _currentCardCnt++;
        }

        public int FieldCount(eCardField field)
        {
            int temp = 0;
            for(int n = 0; n < _cardDeck[field].Length; n++)
            {
                if (_cardDeck[field][n] != null && !_cardDeck[field][n]._IsEmpty)
                    temp++;
            }

            return temp;
        }

        public bool IsOver()
        {
            foreach (eCardField key in _cardDeck.Keys)
            {
                for(int n = 0; n < _cardDeck[key].Length; n++)
                {
                    if (_cardDeck[key][n] == null || _cardDeck[key][n]._IsEmpty)
                        return false;
                }
            }

            return true;
        }

        public bool IsContain(eCardField field, int cardIndex)
        {
            for (int n = 0; n < _cardDeck[field].Length; n++)
            {
                if (_cardDeck[field][n] != null && _cardDeck[field][n]._CardIndex == cardIndex)
                    return true;
            }

            return false;
        }

        public int[] GetFieldCard(eCardField field)
        {
            int[] temp = new int[_cardDeck[field].Length];

            for (int n = 0; n < temp.Length; n++)
                temp[n] = _cardDeck[field][n]._CardIndex;

            return temp;
        }

        public bool GetFlaskOnCard(int cardIndex, out int flaskCnt)
        {
            flaskCnt = 0;
            foreach (eCardField key in _cardDeck.Keys)
            {
                for(int n = 0; n < _cardDeck[key].Length; n++)
                {
                    if (_cardDeck[key][n]._CardIndex == cardIndex)
                    {
                        flaskCnt = _cardDeck[key][n]._FlaskCount;
                        _cardDeck[key][n]._FlaskCount = 0;
                        return true;
                    }
                        
                }
            }

            return false;
        }

        public void PickCard(int cardIndex)
        {
            foreach(eCardField field in _cardDeck.Keys)
            {
                for(int n = 0; n < _cardDeck[field].Length; n++)
                {
                    if (_cardDeck[field][n]._CardIndex == cardIndex)
                    {
                        _cardDeck[field][n]._CardCount--;
                        return;
                    }
                }
            }
        }

        public int CardCount(int cardIndex)
        {
            foreach (eCardField field in _cardDeck.Keys)
            {
                for (int n = 0; n < _cardDeck[field].Length; n++)
                {
                    if (_cardDeck[field][n]._CardIndex == cardIndex)
                        return _cardDeck[field][n]._CardCount;
                }
            }

            return -1;
        }

        public void ReturnCard(int cardIndex)
        {
            foreach (eCardField field in _cardDeck.Keys)
            {
                for (int n = 0; n < _cardDeck[field].Length; n++)
                {
                    if (_cardDeck[field][n]._CardIndex == cardIndex)
                    {
                        _cardDeck[field][n]._CardCount++;
                        return;
                    }
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
