using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    abstract class TableBase
    {
        protected Dictionary<string, Dictionary<string, string>> _sheetData;

        public Dictionary<string, Dictionary<string, string>> _datas
        {
            get
            {
                return _sheetData;
            }
        }

        /// <summary>
        /// 테이블 파일을 정보를 읽어와서 저장소에 저장하는 함수
        /// </summary>
        /// <param name="strJson">json파일명</param>
        public abstract void LoadTable(string strJson);

        public void Add(string key, string subKey, string val)
        {
            if (!_sheetData.ContainsKey(key))
            {
                // Save
                _sheetData.Add(key, new Dictionary<string, string>());
            }

            if (!_sheetData[key].ContainsKey(subKey))
            {
                _sheetData[key].Add(subKey, val);
            }
        }

        string GetValue(int key, string subKey)
        {
            string strKey = key.ToString();
            string findVal = string.Empty;

            if (_sheetData.ContainsKey(strKey))
            {
                if (_sheetData[strKey].ContainsKey(subKey))
                    findVal = _sheetData[strKey][subKey];
                else
                    Console.WriteLine("Wrong SubKey : " + subKey);
            }
            else
                Console.WriteLine("Wrong Key : " + strKey);

            return findVal;
        }

        public string ToS(int key, string subKey)
        {
            return GetValue(key, subKey);
        }

        public int ToI(int key, string subKey)
        {
            int val = 0;
            int.TryParse(GetValue(key, subKey), out val);

            return val;
        }

        public float ToF(int key, string subKey)
        {
            float val = 0;
            float.TryParse(GetValue(key, subKey), out val);

            return val;
        }

        public int FindCount(string val, string subKey)
        {
            int count = 0;
            foreach (string key in _sheetData.Keys)
            {
                if (_sheetData[key][subKey].CompareTo(val) == 0)
                    count++;
            }

            return count;
        }
    }
}
