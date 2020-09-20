using System;
using System.Collections.Generic;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.IO;
using LitJson;

namespace ExcelToJsonConverter
{
    class ConvertManager
    {
        List<string> _ltSheetName = new List<string>();

        Dictionary<int, Dictionary<int, List<string>>> _dicViewer; // List -> column, Dic -> sheet Dic -> total
        Dictionary<string, Dictionary<string, List<string>>> _sDicData; // Json 파일로 변환전 구조 바꾸기 -> ExcelBasic Project 찾아보기
        // 시트명 인덱스 값

        string _pathFile;

        public ConvertManager(string file)
        {
            _pathFile = System.IO.Directory.GetCurrentDirectory() + "\\" + file;
            _dicViewer = new Dictionary<int, Dictionary<int, List<string>>>();
            _sDicData = new Dictionary<string, Dictionary<string, List<string>>>();
        }

        #region JSON
        void SetFirstColumn(List<string> column, Dictionary<string, List<string>> dicData)
        {
            foreach (KeyValuePair<string, List<string>> tempKey in dicData)
            {
                column.Add(tempKey.Key.ToString());
                foreach (string tempValue in tempKey.Value)
                    column.Add(tempValue.ToString());

                break;
            }
        }

        void JSONDataSaveForm(string fileName, Dictionary<string, List<string>> dicData, StreamWriter fileWriter)
        {
            if (fileWriter != null)
            {
                List<string> firstColumn = new List<string>();
                SetFirstColumn(firstColumn, dicData);

                JsonWriter writer = new JsonWriter(fileWriter);
                writer.WriteObjectStart();
                writer.WritePropertyName(fileName.ToString());
                writer.WriteArrayStart();

                bool isFirst = true;
                int keyCount;

                foreach (KeyValuePair<string, List<string>> tempKey in dicData)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        continue;
                    }

                    keyCount = 0;
                    bool isIndex = true;

                    writer.WriteObjectStart();
                    foreach (string tempValue in tempKey.Value)
                    {
                        // 첫번째 칼럼
                        if (isIndex)
                        {
                            writer.WritePropertyName(firstColumn[keyCount++].ToString());
                            writer.Write(tempKey.Key.ToString());
                            isIndex = false;
                        }
                        writer.WritePropertyName(firstColumn[keyCount++].ToString());
                        writer.Write(tempValue.ToString());
                    }
                    writer.WriteObjectEnd();
                }
                writer.WriteArrayEnd();
                writer.WriteObjectEnd();
            }
        }

        void JSONDataParse(Dictionary<string, Dictionary<string, List<string>>> dicData)
        {
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> temp in dicData)
            {
                if (dicData.ContainsKey(temp.Key) == true)
                {
                    string SaveFileFullPath = System.IO.Directory.GetCurrentDirectory() + "\\" + temp.Key + ".json";

                    StreamWriter fileWriter = new StreamWriter(SaveFileFullPath, false, Encoding.Unicode);
                    JSONDataSaveForm(temp.Key, temp.Value, fileWriter);
                    fileWriter.Close();
                }
            }
        }

        void ConvertJSONData()
        {
            for (int n = 1; n <= _dicViewer.Count; n++)
            {
                Dictionary<string, List<string>> FSheet = new Dictionary<string, List<string>>();
                for (int j = 0; j < _dicViewer[n][1].Count; j++)
                {
                    List<string> Fcontents = new List<string>();

                    for (int i = 2; i <= _dicViewer[n].Count; i++)
                    {
                        Fcontents.Add(_dicViewer[n][i][j]);
                    }

                    if (j == 0)
                        FSheet.Add("Index", Fcontents);
                    else
                        FSheet.Add(j.ToString(), Fcontents);
                }
                _sDicData.Add(_ltSheetName[n - 1], FSheet);
            }
        }
        #endregion

        public void Play()
        {
            ExcelLoad(_pathFile);
            ConvertJSONData();
            JSONDataParse(_sDicData);
        }

        public bool ExcelLoad(string path)
        {
            object misValue = System.Reflection.Missing.Value;

            Excel.Application oXL = new Excel.Application();
            Excel.Workbooks oWBooks = oXL.Workbooks;
            Excel.Workbook oWB = oWBooks.Open(path, misValue, misValue, misValue, misValue,
                                                misValue, misValue, misValue, misValue, misValue,
                                                misValue, misValue, misValue, misValue, misValue);
            Excel.Sheets oSheets = oWB.Worksheets;
            for (int n = 1; n <= oSheets.Count; n++)
            {
                Excel.Worksheet oSheet = oSheets.get_Item(n);
                _ltSheetName.Add(oSheet.Name);
                Excel.Range oRng = oSheet.get_Range("A1").SpecialCells(Excel.XlCellType.xlCellTypeLastCell);

                int rowCount = oRng.Row;
                int colCount = ExcelColumnCount(oSheet, oRng);

                List<string> columns = ExcelColumnName(colCount);

                Dictionary<int, List<string>> dicSheet = new Dictionary<int, List<string>>();

                for (int col = 1; col <= colCount; col++)
                {
                    List<string> columnData = new List<string>();
                    int count = 0;
                    Excel.Range collCell = oSheet.Columns[col];
                    Excel.Range range = oSheet.get_Range(columns[col - 1] + 1, collCell);

                    foreach (object temp in range.Value)
                    {
                        if (count < oRng.Row)
                        {
                            count++;
                            if (temp == null)
                                columnData.Add("");
                            else
                                columnData.Add(temp.ToString());
                        }
                        else
                            break;
                    }

                    dicSheet.Add(col, columnData);
                    //제거 후 가비지 호출(range, collCell).
                    ReleaseExcelObject(range);
                    ReleaseExcelObject(collCell);
                }

                _dicViewer.Add(n, dicSheet);
            }

            oXL.Visible = false;
            oXL.UserControl = true;
            oXL.DisplayAlerts = false;
            oXL.Quit();

            return true;
        }

        int ExcelColumnCount(Excel.Worksheet oSheet, Excel.Range oRng)
        {
            int rowCount = oRng.Row;
            int colCount = oRng.Column;

            for (int n = 1; n <= colCount; n++)
            {
                Excel.Range cell = oSheet.Cells[1, n];
                if (cell.Value == null)
                {
                    //제거 후 가비지 호출.
                    ReleaseExcelObject(cell);

                    Console.WriteLine(oSheet.Name.ToString() + " Sheet에 비에있는 셀이 존재합니다.");
                    colCount = n - 1;
                    break;
                }
            }

            return colCount;
        }

        List<string> ExcelColumnName(int length)
        {
            List<string> columnList = new List<string>();

            int baseNumber = 26;
            int baseShare = length / baseNumber;
            int baseRest = length % baseNumber;

            for (int n = 0; n < length; n++)
            {
                if (n / baseNumber == 0)
                    columnList.Add(Convert.ToString((char)(65 + n)));
                else
                {
                    string tempData = Convert.ToString((char)(64 + (n / baseNumber)));
                    tempData += Convert.ToString((char)(65 + (n % baseNumber)));
                    columnList.Add(tempData);
                }
            }

            return columnList;
        }

        void ReleaseExcelObject(object obj)
        {
            try
            {
                if (obj != null)
                {
                    Marshal.ReleaseComObject(obj);
                    obj = null;
                }
            }
            catch (Exception ex)
            {
                obj = null;
                throw ex;
            }
            finally
            {
                // 가비지 컬렉션을 직접 호출.(권장하지 않음)
                GC.Collect();
            }
        }
    }
}
