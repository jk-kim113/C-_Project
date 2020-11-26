using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class CardInfo
    {
        public Dictionary<eCardField, List<int>> _cardGroup = new Dictionary<eCardField, List<int>>();

        public bool IsOver()
        {
            foreach (eCardField key in _cardGroup.Keys)
            {
                if (_cardGroup[key].Count != 3)
                    return false;
            }

            return true;
        }

        public bool IsEmpty()
        {
            foreach (eCardField key in _cardGroup.Keys)
            {
                if (_cardGroup[key].Count != 0)
                    return false;
            }

            return true;
        }
    }
}
