using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class CardInfo
    {
        Dictionary<eCardField, int[]> _cardDeck = new Dictionary<eCardField, int[]>();
        Dictionary<eCardField, int[]> _flaskOnCard = new Dictionary<eCardField, int[]>();
        Dictionary<eCardField, int> _cardCount = new Dictionary<eCardField, int>();

        const int _maxFieldCnt = 3;
        int _currentCardCnt = 0;

        public bool _IsEmpty { get { return _currentCardCnt == 0; } }

        public void InitCardDeck()
        {
            for(int n = 0; n < (int)eCardField.max; n++)
            {
                _cardDeck.Add((eCardField)n, new int[_maxFieldCnt]);
                _flaskOnCard.Add((eCardField)n, new int[_maxFieldCnt]);
                _cardCount.Add((eCardField)n, 0);
            }
        }

        public void AddCard(eCardField field, int cardIndex)
        {
            for(int n = 0; n < _cardDeck[field].Length; n++)
            {
                if(_cardDeck[field][n] == 0)
                {
                    _cardDeck[field][n] = cardIndex;
                    break;
                }
            }

            _cardCount[field]++;
            _currentCardCnt++;
        }

        public int FieldCount(eCardField field)
        {
            return _cardCount[field];
        }

        public bool IsOver()
        {
            foreach (eCardField key in _cardCount.Keys)
            {
                if (_cardCount[key] != 3)
                    return false;
            }

            return true;
        }

        public bool IsContain(eCardField field, int cardIndex)
        {
            return _cardDeck[field].Contains(cardIndex);
        }

        public int[] GetFieldCard(eCardField field)
        {
            return _cardDeck[field];
        }

        public int GetFlaskOnCard(int cardIndex)
        {
            foreach(eCardField key in _cardDeck.Keys)
            {
                for(int n = 0; n < _cardDeck[key].Length; n++)
                {
                    if (_cardDeck[key][n] == cardIndex)
                        return _flaskOnCard[key][n];
                }
            }

            return 0;
        }
    }
}
