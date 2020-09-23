using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TestPeristalssDB
{
    class Program
    {
        static void Main(string[] args)
        {
            int action = 0;

            // Server = IP; Port = number; Database = model name; Uid = account ID; Pwd = password
            using (MySqlConnection connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=gamedata;Uid=root;Pwd=1234;"))
            {
                connection.Open();
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("1.계정 등록\n2.계정 수정\n3.계정 삭제\n4.종료");
                    do
                    {
                        InputNumberToString("명령을 선택하세요 : ", out action);
                    }
                    while (action > 4 || action < 1);

                    if (action == 4)
                        break;

                    switch(action)
                    {
                        case 1:

                            
                            string password = string.Empty;
                            do
                            {
                                Console.Write("6자리 이상이고 특수문자, 영문, 숫자가 최소 1회 이상 포함된 비번을 입력하세요 : ");
                                password = Console.ReadLine();
                            }
                            while (!CheckPW.IsValidPassword(password));

                            Console.Write("닉네임을 입력하세요 : ");
                            string nickname = Console.ReadLine();

                            Console.WriteLine("{0} 하였습니다.", InsertStandardInfo(connection, password, nickname) ? "성공" : "실패");

                            break;
                        case 2:

                            int UUID = 0;
                            InputNumberToString("UUID를 입력하세요 : ", out UUID);

                            if(SearchStandardInfo(connection, UUID))
                            {
                                Console.WriteLine("1.닉네임 등록\n2.코인 수정\n3.캐릭터 수 수정");
                                do
                                {
                                    InputNumberToString("무엇을 수정하시겠습니까 : ", out action);
                                }
                                while (action > 3 || action < 1);

                                switch(action)
                                {
                                    case 1:

                                        string oriName = SearchStandardInfo(connection, UUID, "NickName");
                                        Console.Write("현재 닉네임은 {0}입니다. 바꿀 닉네임을 입력하세요 : ", oriName);
                                        string newName = Console.ReadLine();

                                        UpdateStandardInfo(connection, UUID, "NickName", newName);

                                        break;
                                    case 2:

                                        string oriCoin = SearchStandardInfo(connection, UUID, "Coin");
                                        Console.Write("현재 코인 수 는 {0}입니다. 바꿀 코인 수를 입력하세요 : ", oriCoin);
                                        string newCoin = Console.ReadLine();

                                        UpdateStandardInfo(connection, UUID, "Coin", newCoin);

                                        break;
                                    case 3:

                                        string oriCharacterCnt = SearchStandardInfo(connection, UUID, "CharacterCount");
                                        Console.Write("현재 캐릭터 수 는 {0}입니다. 몇 개로 수정할까요 : ", oriCharacterCnt);
                                        int newCharacterCnt = int.Parse(Console.ReadLine());

                                        UpdateStandardInfo(connection, UUID, "CharacterCount", newCharacterCnt.ToString());

                                        DeleteAt(connection, "baseTable", "UUID", UUID.ToString());

                                        for (int n = 0; n < newCharacterCnt; n++)
                                        {
                                            Console.Write("{0}번째 캐릭터 ID를 입력하세요 : ", n + 1);
                                            string newCharacterID = Console.ReadLine();
                                            InsertBaseTable(connection, UUID, n + 1, newCharacterID);
                                        }

                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("해당 정보는 없습니다.");
                            }

                            break;
                        case 3:

                            int UUIDdelete = 0;
                            InputNumberToString("UUID를 입력하세요 : ", out UUIDdelete);

                            if(SearchStandardInfo(connection, UUIDdelete))
                            {
                                string oriName = SearchStandardInfo(connection, UUIDdelete, "NickName");
                                Console.WriteLine("해당 계정은 닉네임이 {0} 입니다.", oriName);
                                Console.Write("정말 삭제하시겠습니까? (y / n) : ");
                                string del = Console.ReadLine();
                                if(del == "y")
                                {
                                    DeleteAt(connection, "stdTable", "UUID", UUIDdelete.ToString());
                                    DeleteAt(connection, "baseTable", "UUID", UUIDdelete.ToString());
                                    //Rearrange(connection);
                                }
                            }
                            else
                            {
                                Console.WriteLine("해당 정보는 없습니다.");
                            }

                            break;
                    }

                    Console.Read();
                }

                connection.Close();
            }
        }

        static bool InsertBaseTable(MySqlConnection connection, int UUID, int index, string value)
        {
            string insertQuery = string.Format("INSERT INTO baseTable(UUID,CharacterIndex,CharacterID) VALUES({0},{1},'{2}')", UUID, index, value);

            try
            {
                MySqlCommand command = new MySqlCommand(insertQuery, connection);
                if (command.ExecuteNonQuery() == 1)
                {
                    Console.WriteLine("Insert Success");
                    return true;
                }
                else
                {
                    Console.WriteLine("Insert Fail");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("연결 실패!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static bool InsertStandardInfo(MySqlConnection connection, string password, string nickname)
        {
            string insertQuery = string.Format("INSERT INTO stdTable(PW,NickName,Coin,CharacterCount) VALUES('{0}','{1}', 0, 0)", password, nickname);

            try
            {
                MySqlCommand command = new MySqlCommand(insertQuery, connection);
                if (command.ExecuteNonQuery() == 1)
                {
                    Console.WriteLine("Insert Success");
                    return true;
                }
                else
                {
                    Console.WriteLine("Insert Fail");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("연결 실패!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static bool DeleteAt(MySqlConnection connection, string tableName, string column, string value)
        {
            string deleteQuery = string.Format("DELETE FROM {0} WHERE {1} = {2}", tableName, column, value);

            try
            {
                MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                if (command.ExecuteNonQuery() == 1)
                {
                    Console.WriteLine("Delete Success");
                    return true;
                }
                else
                {
                    Console.WriteLine("Delete Fail");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("연결 실패!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #region Auto Increment
        //static void Rearrange(MySqlConnection connection)
        //{
        //    //string setTemp = "SET @CNT = 1000000000;";
        //    string updateQuery = "UPDATE gamedata.stdTable SET gamedata.stdTable.UUID = @CNT := @CNT+1";

        //    try
        //    {
        //        MySqlCommand command = new MySqlCommand();
        //        command.Connection = connection;
        //        command.Parameters.Add("@CNT", MySqlDbType.Int32).Value = 1000000000;
        //        command.Parameters["@CNT"].Direction = System.Data.ParameterDirection.Output;
        //        command.CommandText = updateQuery;
        //        if (command.ExecuteNonQuery() == 1)
        //        {
        //            Console.WriteLine("Update Success");
        //        }
        //        else
        //        {
        //            Console.WriteLine("Update Fail");
        //        }

        //        //long id = command.LastInsertedId;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("연결 실패!!");
        //        Console.WriteLine(ex.ToString());
        //    }
        //}
        #endregion

        static bool SearchbaseTable(MySqlConnection connection, int index)
        {
            string searchQuery = string.Format("SELECT * FROM baseTable");

            try
            {
                MySqlCommand command = new MySqlCommand(searchQuery, connection);

                MySqlDataReader table = command.ExecuteReader();
                while (table.Read())
                {
                    if (table["Idx"].ToString() == index.ToString())
                    {
                        table.Close();
                        return true;
                    }
                }
                table.Close();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("연결 실패!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static bool SearchStandardInfo(MySqlConnection connection, int UUID)
        {
            string searchQuery = string.Format("SELECT * FROM stdTable");

            try
            {
                MySqlCommand command = new MySqlCommand(searchQuery, connection);

                MySqlDataReader table = command.ExecuteReader();
                while(table.Read())
                {   
                    if(table["UUID"].ToString() == UUID.ToString())
                    {
                        table.Close();
                        return true;
                    }
                }
                table.Close();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("연결 실패!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static string SearchStandardInfo(MySqlConnection connection, int UUID, string column)
        {
            string searchQuery = string.Format("SELECT {0} FROM stdTable WHERE UUID = {1};", column, UUID);

            try
            {
                MySqlCommand command = new MySqlCommand(searchQuery, connection);

                MySqlDataReader table = command.ExecuteReader();
                
                while (table.Read())
                {
                    string value = table[0].ToString();
                    table.Close();
                    return value;
                }
                table.Close();
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("연결 실패!!");
                Console.WriteLine(ex.ToString());
                return string.Empty;
            }
        }

        static bool UpdateStandardInfo(MySqlConnection connection, int UUID, string column, string value)
        {
            string updateQuery = string.Empty;

            if(column == "NickName")
            {
                updateQuery = string.Format("UPDATE stdTable SET {0} = '{1}' WHERE UUID = {2}", column, value, UUID);
            }
            else
            {
                updateQuery = string.Format("UPDATE stdTable SET {0} = {1} WHERE UUID = {2}", column, value, UUID);
            }

            try
            {
                MySqlCommand command = new MySqlCommand(updateQuery, connection);
                if (command.ExecuteNonQuery() == 1)
                {
                    Console.WriteLine("Update Success");
                    return true;
                }
                else
                {
                    Console.WriteLine("Update Fail");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("연결 실패!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static void InputNumberToString(string explan, out int result)
        {
            while (true)
            {
                Console.Write(explan);
                if (!int.TryParse(Console.ReadLine(), out result))
                {
                    Console.WriteLine("숫자형 문자열을 입력하지 않았습니다.");
                }
                else
                    break;
            }
        }
    }
}