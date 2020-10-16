using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_CardBattle
{
    class CardBattleAI
    {
        public enum eAIDifficulty
        {
            Easy = 6,
            Normal = 10,
            Hard = 14
        }

        int _memoriableNum;
        List<int> _memory = new List<int>();

        public CardBattleAI(eAIDifficulty diff)
        {
            _memoriableNum = (int)diff;
        }

        public bool Check(int[] iconIdx, out int[] sameCardIdx)
        {
            sameCardIdx = new int[2];
            for (int n = 0; n < _memory.Count - 1; n++)
            {
                for(int m = n + 1; m < _memory.Count; m++)
                {
                    if(iconIdx[_memory[n]] == iconIdx[_memory[m]])
                    {
                        sameCardIdx[0] = _memory[n];
                        sameCardIdx[1] = _memory[m];
                        _memory.Remove(sameCardIdx[0]);
                        _memory.Remove(sameCardIdx[1]);
                        return true;
                    }
                }
            }

            return false;
        }

        public void SaveMemory(params int[] card)
        {
            if(_memory.Count >= _memoriableNum)
                for (int n = 0; n < card.Length; n++)
                    _memory.RemoveAt(0);

            for(int n = 0; n < card.Length; n++)
            {
                if(!_memory.Contains(card[n]))
                    _memory.Add(card[n]);
            }
        }

        public void RemoveMemory(params int[] card)
        {
            for (int n = 0; n < card.Length; n++)
            {
                if(_memory.Contains(card[n]))
                    _memory.Remove(card[n]);
            }   
        }
    }
}
