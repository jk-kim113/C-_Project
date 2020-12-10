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

        public long SearchUUIDwithNickName(string nickName)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT UUID FROM characterinfo WHERE NickName = '{0}';", nickName);

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

        public int SearchCharacterIndex(string nickName)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT CharacterIndex FROM characterinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        return int.Parse(table["CharacterIndex"].ToString());
                    }

                    table.Close();
                    return -1;
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

            return -1;
        }

        public void SearchLevelInfo(string nickName, List<int> level)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT AccountLevel,PhysicsLevel,ChemistryLevel,BiologyLevel,AstronomyLevel FROM characterinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        level.Add(int.Parse(table["AccountLevel"].ToString()));
                        level.Add(int.Parse(table["PhysicsLevel"].ToString()));
                        level.Add(int.Parse(table["ChemistryLevel"].ToString()));
                        level.Add(int.Parse(table["BiologyLevel"].ToString()));
                        level.Add(int.Parse(table["AstronomyLevel"].ToString()));
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

        public int SearchAccountLevel(string nickName)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT AccountLevel FROM characterinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        return int.Parse(table["AccountLevel"].ToString());
                    }

                    table.Close();
                    return -1;
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

                return -1;
            }
        }

        public void SearchExpInfo(string nickName, List<int> exp)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT AccountExp,PhysicsExp,ChemistryExp,BiologyExp,AstronomyExp FROM characterinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        exp.Add(int.Parse(table["AccountExp"].ToString()));
                        exp.Add(int.Parse(table["PhysicsExp"].ToString()));
                        exp.Add(int.Parse(table["ChemistryExp"].ToString()));
                        exp.Add(int.Parse(table["BiologyExp"].ToString()));
                        exp.Add(int.Parse(table["AstronomyExp"].ToString()));
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

        public void SearchCardRentalInfo(string nickName, List<int> cardRental)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT CardIndex FROM cardrentalinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        cardRental.Add(int.Parse(table["CardIndex"].ToString()));
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

        public void SearchRentalTimeInfo(string nickName, List<float> rentaltime)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT TimeRemaining FROM cardrentalinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        rentaltime.Add(float.Parse(table["TimeRemaining"].ToString()));
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

        public void SearchMyDeckInfo(string nickName, List<int> mydeck)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT CardIndex FROM mycardinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        mydeck.Add(int.Parse(table["CardIndex"].ToString()));
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

        public void SearchAllCard(string nickNameArr, List<int> allCard)
        {
            string[] nickName = nickNameArr.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            string searchQuery = string.Empty;
            string inputStr = string.Empty;

            for (int n = 0; n < nickName.Length; n++)
            {
                inputStr += string.Format("'{0}'", nickName[n]);

                if (n + 1 < nickName.Length)
                    inputStr += ",";
            }

            searchQuery = string.Format("SELECT DISTINCT CardIndex FROM cardreleaseinfo WHERE NickName IN ({0});", inputStr);

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        allCard.Add(int.Parse(table["CardIndex"].ToString()));
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

        public void SearchShopInfo(string nickName, Dictionary<int, int> itemDic)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT ItemIndex,ItemCount FROM iteminfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        itemDic.Add(int.Parse(table["ItemIndex"].ToString()), int.Parse(table["ItemCount"].ToString()));
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

        public void SearchCoin(string nickName, List<int> coinList)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT MasterCoin,AccountCoin,PhysicsCoin,ChemistryCoin,BiologyCoin,AstronomyCoin FROM characterinfo WHERE NickName = '{0}';", nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        coinList.Add(int.Parse(table["MasterCoin"].ToString()));
                        coinList.Add(int.Parse(table["AccountCoin"].ToString()));
                        coinList.Add(int.Parse(table["PhysicsCoin"].ToString()));
                        coinList.Add(int.Parse(table["ChemistryCoin"].ToString()));
                        coinList.Add(int.Parse(table["BiologyCoin"].ToString()));
                        coinList.Add(int.Parse(table["AstronomyCoin"].ToString()));
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

        public int SearchSavedCoin(string nickName, string coinKind)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT {0} FROM characterinfo WHERE NickName = '{1}';", coinKind, nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        return int.Parse(table[coinKind].ToString());
                    }

                    table.Close();
                    return 0;
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

                return 0;
            }
        }

        public bool SearchItemCount(string nickName, int itemIndex, out int itemCnt)
        {
            itemCnt = 0;

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT ItemCount FROM iteminfo WHERE NickName = '{0}' AND ItemIndex = {1};", nickName, itemIndex);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        itemCnt = int.Parse(table["ItemCount"].ToString());
                        return true;
                    }

                    table.Close();
                    return false;
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

                return false;
            }
        }

        public void SearchMyFriend(string nickName, Dictionary<string, int> temp)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT fi.FriendNickName,ci.AccountLevel " +
                                                "FROM (SELECT FriendNickName FROM friendinfo WHERE NickName = '{0}') fi," +
                                                "(SELECT AccountLevel FROM characterinfo WHERE NickName = (SELECT FriendNickName FROM friendinfo WHERE NickName = '{1}')) ci;", nickName, nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        temp.Add(table["FriendNickName"].ToString(), int.Parse(table["AccountLevel"].ToString()));

                        Console.WriteLine("============================MyFriend List============================");
                        Console.Write(table["FriendNickName"].ToString() + "\t");
                        Console.WriteLine(table["AccountLevel"].ToString());
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

        public void SearchReceiveFriend(string nickName, Dictionary<string, int> temp)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT fi.FriendNickName,ci.AccountLevel " +
                                                "FROM (SELECT FriendNickName FROM receivefriendinfo WHERE NickName = '{0}') fi," +
                                                "(SELECT AccountLevel FROM characterinfo WHERE NickName = (SELECT FriendNickName FROM receivefriendinfo WHERE NickName = '{1}')) ci;", nickName, nickName);

                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        temp.Add(table["FriendNickName"].ToString(), int.Parse(table["AccountLevel"].ToString()));

                        Console.WriteLine("============================Receive Friend List============================");
                        Console.Write(table["FriendNickName"].ToString() + "\t");
                        Console.WriteLine(table["AccountLevel"].ToString());
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

        public void SearchWithFriend(string nickName, Dictionary<string, int> temp, List<string> remainTime)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string searchQuery = string.Format("SELECT fi.OpponentNickName,fi.PlayDate,ci.AccountLevel " +
                                                "FROM (SELECT OpponentNickName,PlayDate FROM playwithinfo WHERE NickName = '{0}') fi," +
                                                "(SELECT AccountLevel FROM characterinfo WHERE NickName = (SELECT OpponentNickName FROM playwithinfo WHERE NickName = '{1}')) ci;", nickName, nickName);
                
                try
                {
                    MySqlCommand command = new MySqlCommand(searchQuery, connection);

                    MySqlDataReader table = command.ExecuteReader();

                    while (table.Read())
                    {
                        temp.Add(table["OpponentNickName"].ToString(), int.Parse(table["AccountLevel"].ToString()));
                        remainTime.Add(table["PlayDate"].ToString());

                        Console.WriteLine("============================Play With Friend List============================");
                        Console.Write(table["OpponentNickName"].ToString() + "\t");
                        Console.Write(table["PlayDate"].ToString() + "\t");
                        Console.WriteLine(table["AccountLevel"].ToString());
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

        public void InsertItem(string nickName, int itemIndex)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string insertQuery = string.Format("INSERT INTO iteminfo(NickName, ItemIndex, ItemCount) VALUES ('{0}',{1},{2});", nickName, itemIndex, 1);

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

        public void UpdateCoinValue(string nickName, string coinKind, int coin)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string updateQuery = string.Format("UPDATE characterinfo SET {0}={1} WHERE NickName='{2}';", coinKind, coin, nickName);

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

        public void UpdateItemCount(string nickName, int itemIndex, int itemCnt)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string updateQuery = string.Format("UPDATE iteminfo SET ItemCount = {0} WHERE NickName='{1}' AND ItemIndex={2};", itemCnt, nickName, itemIndex);

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
