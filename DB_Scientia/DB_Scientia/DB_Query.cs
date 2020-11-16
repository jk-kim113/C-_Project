using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DB_Scientia
{
    public struct CharacterInfo
    {
        public string _nickName;
        public int _chracterIndex;
        public int _accountLevel;
        public int _slotIndex;
    }

    class DB_Query
    {
        //Server=127.0.0.1;Port=3306;Database=cardbattle;Uid=root;Pwd=1234;
        string _connectionString;

        public DB_Query(string serverIP, string port, string database, string uID, string Pwd)
        {
            _connectionString = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};", serverIP, port, database, uID, Pwd);
        }

        public bool CheckLogIn(string id, string pw)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT Pw FROM userinfo WHERE ID = '{0}';", id);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        if (table["Pw"].ToString().Equals(pw))
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
                finally
                {
                    connection.Close();
                }
            }
        }

        public long SearchUUID(string id)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT UUID FROM userinfo WHERE ID = '{0}';", id);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        long uuid = long.Parse(table["UUID"].ToString());
                        table.Close();
                        return uuid;
                    }

                    table.Close();
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("연결 실패!!");
                    Console.WriteLine(ex.ToString());
                    return 0;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public bool SearchID(string id)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT * FROM userinfo WHERE ID = '{0}';", id);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        if (table["ID"].ToString().Equals(id))
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
                finally
                {
                    connection.Close();
                }
            }
        }

        public bool SearchNickName(string nickname)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT * FROM userinfo WHERE ID = '{0}';", nickname);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        if (table["ID"].ToString().Equals(nickname))
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
                finally
                {
                    connection.Close();
                }
            }
        }

        public void SearchCharacterInfo(long uuid, List<CharacterInfo> characInfoList)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT NickName,CharacterIndex,AccountLevel,SlotIndex FROM characterinfo WHERE UUID = '{0}';", uuid);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();
                    
                    while (table.Read())
                    {
                        CharacterInfo characInfo;
                        characInfo._nickName = table["NickName"].ToString();
                        characInfo._chracterIndex = int.Parse(table["CharacterIndex"].ToString());
                        characInfo._accountLevel = int.Parse(table["AccountLevel"].ToString());
                        characInfo._slotIndex = int.Parse(table["SlotIndex"].ToString());

                        characInfoList.Add(characInfo);
                    }

                    table.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("연결 실패!!");
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public void SearchCardReleaseInfo(string nickName, List<int> cardRelease)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT CardIndex FROM cardreleaseinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        cardRelease.Add(int.Parse(table["CardIndex"].ToString()));
                    }

                    table.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("연결 실패!!");
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public bool InsertUserInfo(string id, string pw)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string insertQuery = string.Format("INSERT INTO userinfo(ID, PW) VALUES ('{0}','{1}');", id, pw);

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
                finally
                {
                    connection.Close();
                }
            }
        }

        public bool InsertCharacterInfo(long uuid, string nickName, int characIndex, int slot)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string insertQuery = string.Format("INSERT INTO characterinfo(NickName, UUID, CharacterIndex, SlotIndex) VALUES ('{0}',{1},{2},{3});", nickName, uuid, characIndex, slot);

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
                finally
                {
                    connection.Close();
                }
            }
        }

        public void InsertCardReleaseInfo(string nickName, int cardIndex)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string insertQuery = string.Format("INSERT INTO cardreleaseinfo(NickName, CardIndex) VALUES ('{0}',{1});", nickName, cardIndex);

                try
                {
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);

                    if (command.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Insert Success");
                    }
                    else
                    {
                        Console.WriteLine("Insert Fail");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("연결 실패!!");
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
