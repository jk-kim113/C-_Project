using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DB_CardBattle
{
    class DB_Query
    {
        //Server=127.0.0.1;Port=3306;Database=cardbattle;Uid=root;Pwd=1234;
        string _connectionString;

        public DB_Query(string serverIP, string port, string database, string uID, string Pwd)
        {
            _connectionString = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};", serverIP, port, database, uID, Pwd);
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

        public bool SearchLogIn(string id, string pw)
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

        public bool SearchIsFirstLogIn(string id)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT IFNULL(NickName, 'Empty') 'NickName' FROM userinfo WHERE ID = '{0}';", id);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        if (table["NickName"].ToString().Equals("Empty"))
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

        public string SearchNickName(long uuid)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT NickName FROM userinfo WHERE UUID = '{0}';", uuid);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        string nickname = table["NickName"].ToString();
                        table.Close();
                        return nickname;
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
                finally
                {
                    connection.Close();
                }
            }
        }

        public int SearchAvatarIndex(long uuid)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT AvatarIndex FROM userinfo WHERE UUID = '{0}';", uuid);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        int avatarIndex;
                        if (int.TryParse(table["AvatarIndex"].ToString(), out avatarIndex))
                        {
                            table.Close();
                            return avatarIndex;
                        }
                    }

                    table.Close();
                    return -999;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("연결 실패!!");
                    Console.WriteLine(ex.ToString());
                    return -999;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public string SearchValueInUserInfo(MainDB.eTableUserInfo want, MainDB.eTableUserInfo condition, string conditionValue)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT {0} FROM userinfo WHERE {1} = '{0}';", want, condition, conditionValue);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        string wantValue = table[want.ToString()].ToString();
                        table.Close();
                        return wantValue;
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
                finally
                {
                    connection.Close();
                }
            }
        }

        public int SearchMinClearTime(long uuid)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT MinClearTime FROM gameinfo WHERE UUID = {0};", uuid);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    if (table.Read())
                    {
                        int minTime = int.Parse(table["MinClearTime"].ToString());
                        table.Close();
                        return minTime;
                    }

                    table.Close();
                    return -999;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("연결 실패!!");
                    Console.WriteLine(ex.ToString());
                    return -999;
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

        public void InsertGameInfo(long uuid)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string insertQuery = string.Format("INSERT INTO gameinfo(UUID,ClearStage,MinClearTime,TotalPlayCount) VALUES ('{0}',{1},'{2}',{3});", uuid, 0, int.MaxValue, 0);

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

        public void InsertTotalResult(long uuid, int isWin)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string insertQuery = string.Format("INSERT INTO totalresult(UUID, IsWin) VALUES ({0},{1});", uuid, isWin);

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

        public void UpdateUserInfo(long uuid, string nickname, int avatar)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string updateQuery = string.Format("UPDATE userinfo SET NickName='{0}',AvatarIndex={1} WHERE UUID={2};", nickname, avatar, uuid);

                try
                {
                    MySqlCommand command = new MySqlCommand(updateQuery, connection);

                    if (command.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Update Success");
                    }
                    else
                    {
                        Console.WriteLine("Update Fail");
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

        public void UpdateClearTime(long uuid, int cleartime)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string updateQuery = string.Format("UPDATE gameinfo SET MinClearTime={0} WHERE UUID={1};", cleartime, uuid);

                try
                {
                    MySqlCommand command = new MySqlCommand(updateQuery, connection);

                    if (command.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Update Success");
                    }
                    else
                    {
                        Console.WriteLine("Update Fail");
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

        public void UpdateTotalPlayCount(long uuid)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string updateQuery = string.Format("UPDATE gameinfo SET TotalPlayCount=TotalPlayCount+1 WHERE UUID={0};", uuid);

                try
                {
                    MySqlCommand command = new MySqlCommand(updateQuery, connection);

                    if (command.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Update Success");
                    }
                    else
                    {
                        Console.WriteLine("Update Fail");
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
