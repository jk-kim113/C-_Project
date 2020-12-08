using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    public enum eTableType
    {
        CardData,
        ItemData,

        max
    }

    class TableManager
    {
        Dictionary<eTableType, TableBase> _tableData = new Dictionary<eTableType, TableBase>();

        TableBase Load<T>(eTableType type) where T : TableBase, new()
        {
            if (_tableData.ContainsKey(type))
                return _tableData[type];

            string LoadFileFullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + type.ToString() + ".json";

            StreamReader fileReader = new StreamReader(LoadFileFullPath, Encoding.Unicode, false);
            string json = fileReader.ReadToEnd();
            fileReader.Close();

            if (json != null)
            {
                T t = new T();
                t.LoadTable(json);
                _tableData.Add(type, t);

                return _tableData[type];
            }

            return null;
        }

        public void LoadAll()
        {
            Load<CardData>(eTableType.CardData);
            Load<ItemData>(eTableType.ItemData);
        }

        public TableBase Get(eTableType type)
        {
            if (_tableData.ContainsKey(type))
                return _tableData[type];

            return null;
        }

        public void AllClear()
        {
            _tableData.Clear();
        }
    }
}
