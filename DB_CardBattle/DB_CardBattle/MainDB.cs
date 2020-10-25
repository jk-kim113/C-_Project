using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DB_CardBattle
{
    public struct FromServerData
    {
        public byte[] _data;
        public int _length;

        public FromServerData(byte[] data, int length)
        {
            _data = data;
            _length = length;
        }
    }

    class MainDB
    {
        const short _port = 81;
        Socket _waitServer;

        Socket _conncetServer;

        Queue<FromServerData> _fromServerQueue = new Queue<FromServerData>();
        Queue<byte[]> _toServerQueue = new Queue<byte[]>();

        Thread _tAccept;
        Thread _tFromServer;
        Thread _tToServer;

        public MainDB()
        {
            CreateServer();
        }

        void CreateServer()
        {
            try
            {
                _waitServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _waitServer.Bind(new IPEndPoint(IPAddress.Any, _port));
                _waitServer.Listen(1);

                Console.WriteLine("소켓이 만들어 졌습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        void Accept()
        {
            try
            {
                while (true)
                {
                    if (_waitServer != null && _waitServer.Poll(0, SelectMode.SelectRead))
                    {
                        _conncetServer = _waitServer.Accept();
                        Console.WriteLine("Server Accept");
                    }

                    if (_conncetServer != null)
                    {
                        if (_conncetServer.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buffer = new byte[1032];

                            try
                            {
                                int recvLen = _conncetServer.Receive(buffer);
                                if (recvLen > 0)
                                {
                                    _fromServerQueue.Enqueue(new FromServerData(buffer, recvLen));
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                    }

                    Thread.Sleep(10);
                }
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        void FromServerQueue()
        {
            try
            {
                while(true)
                {
                    if(_fromServerQueue.Count != 0)
                    {
                        FromServerData fData = _fromServerQueue.Dequeue();

                        DefinedStructure.PacketInfo packet = new DefinedStructure.PacketInfo();
                        packet = (DefinedStructure.PacketInfo)ConvertPacket.ByteArrayToStructure(fData._data, packet.GetType(), fData._length);

                        switch((DefinedProtocol.eFromServer)packet._id)
                        {
                            case DefinedProtocol.eFromServer.OverlapCheck_ID:

                                DefinedStructure.Packet_OverlapCheckID pOverlapCheck = new DefinedStructure.Packet_OverlapCheckID();
                                pOverlapCheck = (DefinedStructure.Packet_OverlapCheckID)ConvertPacket.ByteArrayToStructure(packet._data, pOverlapCheck.GetType(), packet._totalSize);

                                OverlapCheck_ID(pOverlapCheck._id, pOverlapCheck._index);

                                break;

                            case DefinedProtocol.eFromServer.JoinGame:

                                DefinedStructure.Packet_JoinGame pJoinGame = new DefinedStructure.Packet_JoinGame();
                                pJoinGame = (DefinedStructure.Packet_JoinGame)ConvertPacket.ByteArrayToStructure(packet._data, pJoinGame.GetType(), packet._totalSize);

                                JoinGame(pJoinGame._id, pJoinGame._pw, pJoinGame._index);

                                break;

                            case DefinedProtocol.eFromServer.LogIn:

                                DefinedStructure.Packet_LogIn pLogIn = new DefinedStructure.Packet_LogIn();
                                pLogIn = (DefinedStructure.Packet_LogIn)ConvertPacket.ByteArrayToStructure(packet._data, pLogIn.GetType(), packet._totalSize);

                                LogIn(pLogIn._id, pLogIn._pw, pLogIn._index);

                                break;

                            case DefinedProtocol.eFromServer.EnrollUserInfo:

                                DefinedStructure.Packet_MyInfo pMyInfo = new DefinedStructure.Packet_MyInfo();
                                pMyInfo = (DefinedStructure.Packet_MyInfo)ConvertPacket.ByteArrayToStructure(packet._data, pMyInfo.GetType(), packet._totalSize);

                                EnrollUserInfo(pMyInfo._UUID, pMyInfo._name, pMyInfo._avatarIndex);

                                break;
                        }
                    }
                }
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        void ToServerQueue()
        {
            try
            {
                while (true)
                {
                    if (_toServerQueue.Count != 0)
                    {
                        byte[] buffer = _toServerQueue.Dequeue();

                        _conncetServer.Send(buffer);
                    }

                    Thread.Sleep(10);
                }
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        void OverlapCheck_ID(string id, int index)
        {
            // Server = IP; Port = number; Database = model name; Uid = account ID; Pwd = password
            using (MySqlConnection connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=cardbattle;Uid=root;Pwd=1234;"))
            {
                connection.Open();

                DefinedStructure.Packet_OverlapCheckResultID pOverlapResult;
                pOverlapResult._index = index;

                if (SearchValue(connection, id))
                    pOverlapResult._result = 0;
                else
                    pOverlapResult._result = 1;

                ToPacket(DefinedProtocol.eToServer.OverlapCheckResult_ID, pOverlapResult);

                connection.Close();
            }
        }

        void JoinGame(string id, string pw, int index)
        {
            using (MySqlConnection connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=cardbattle;Uid=root;Pwd=1234;"))
            {
                connection.Open();

                if(InsertValue(connection, id, pw))
                {
                    DefinedStructure.Packet_CompleteJoin pCompleteJoin;
                    pCompleteJoin._UUID = SearchUUID(connection, id);
                    pCompleteJoin._index = index;

                    InsertGameInfo(connection, pCompleteJoin._UUID);

                    ToPacket(DefinedProtocol.eToServer.CompleteJoin, pCompleteJoin);
                }

                connection.Close();
            }
        }

        void LogIn(string id, string pw, int index)
        {
            using (MySqlConnection connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=cardbattle;Uid=root;Pwd=1234;"))
            {
                connection.Open();

                DefinedStructure.Packet_LogInResult pLogInResult;
                pLogInResult._index = index;

                if(SearchLogIn(connection, id, pw))
                {
                    pLogInResult._UUID = SearchUUID(connection, id);
                    pLogInResult._name = SearchNickName(connection, pLogInResult._UUID);
                    pLogInResult._avatarIndex = SearchAvatarIndex(connection, pLogInResult._UUID);
                    pLogInResult._isSuccess = 0;

                    if(SearchFirst(connection, id))
                        pLogInResult._isFirst = 0;
                    else
                        pLogInResult._isFirst = 1;
                }
                else
                {
                    pLogInResult._UUID = 0;
                    pLogInResult._name = string.Empty;
                    pLogInResult._avatarIndex = -999;
                    pLogInResult._isSuccess = 1;
                    pLogInResult._isFirst = 0;
                }

                ToPacket(DefinedProtocol.eToServer.LogInResult, pLogInResult);

                connection.Close();
            }
        }

        void EnrollUserInfo(long uuid, string nickname, int avatar)
        {
            using (MySqlConnection connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=cardbattle;Uid=root;Pwd=1234;"))
            {
                connection.Open();

                UpdateUserInfo(connection, uuid, nickname, avatar);

                connection.Close();
            }
        }

        bool InsertValue(MySqlConnection connection, string id, string pw)
        {
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
        }

        void InsertGameInfo(MySqlConnection connection, long uuid)
        {
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
        }

        bool SearchValue(MySqlConnection connection, string id)
        {
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
        }

        long SearchUUID(MySqlConnection connection, string id)
        {
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
        }

        bool SearchLogIn(MySqlConnection connection, string id, string pw)
        {
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
        }

        bool SearchFirst(MySqlConnection connection, string id)
        {
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
        }

        void UpdateUserInfo(MySqlConnection connection, long uuid, string nickname, int avatar)
        {
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
        }

        string SearchNickName(MySqlConnection connection, long uuid)
        {
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
        }

        int SearchAvatarIndex(MySqlConnection connection, long uuid)
        {
            string searchQuery = string.Format("SELECT AvatarIndex FROM userinfo WHERE UUID = '{0}';", uuid);

            try
            {
                MySqlCommand command = new MySqlCommand(searchQuery, connection);

                MySqlDataReader table = command.ExecuteReader();

                while (table.Read())
                {
                    int avatarIndex;
                    if(int.TryParse(table["AvatarIndex"].ToString(), out avatarIndex))
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
        }

        void ToPacket(DefinedProtocol.eToServer toServer, object str)
        {
            DefinedStructure.PacketInfo packet;
            packet._id = (int)toServer;
            packet._data = new byte[1024];

            byte[] temp = ConvertPacket.StructureToByteArray(str);
            for (int n = 0; n < temp.Length; n++)
                packet._data[n] = temp[n];
            packet._totalSize = temp.Length;

            _toServerQueue.Enqueue(ConvertPacket.StructureToByteArray(packet));
        }

        public void MainLoop()
        {
            _tAccept = new Thread(new ThreadStart(Accept));
            _tFromServer = new Thread(new ThreadStart(FromServerQueue));
            _tToServer = new Thread(new ThreadStart(ToServerQueue));

            if (!_tAccept.IsAlive)
                _tAccept.Start();
            if (!_tFromServer.IsAlive)
                _tFromServer.Start();
            if (!_tToServer.IsAlive)
                _tToServer.Start();
        }

        public void ExitProgram()
        {
            if (_tAccept.IsAlive)
            {
                _tAccept.Interrupt();
                _tAccept.Join();
            }
            if (_tFromServer.IsAlive)
            {
                _tFromServer.Interrupt();
                _tFromServer.Join();
            }
            if (_tToServer.IsAlive)
            {
                _tToServer.Interrupt();
                _tToServer.Join();
            }
        }
    }
}
