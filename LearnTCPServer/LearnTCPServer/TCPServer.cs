using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

namespace LearnTCPServer
{
    public struct rPacketData
    {
        public byte[] _buffer;
        public int _bufferLength;
        public int _index;

        public rPacketData(byte[] buffer, int size, int idx)
        {
            _buffer = buffer;
            _bufferLength = size;
            _index = idx;
        }
    }

    public struct sPacketData
    {
        public byte[] _buffer;
        public int _index;

        public sPacketData(byte[] buffer, int idx)
        {
            _buffer = buffer;
            _index = idx;
        }
    }

    class TCPServer
    {
        const short _port = 80;
        Socket _waitServer;
        List<Socket> _clients = new List<Socket>();

        Queue<rPacketData> _receiveQueue = new Queue<rPacketData>();
        Queue<sPacketData> _sendQueue = new Queue<sPacketData>();

        Dictionary<long, int> _currentUsers = new Dictionary<long, int>();
        Dictionary<long, string> _currentUserName = new Dictionary<long, string>();

        List<string> _totalLog = new List<string>();

        Thread t_acceptClnt;
        Thread t_addOrder;
        Thread t_doOrder;
        Thread t_sendOrder;

        public TCPServer()
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
            }
        }

        void AcceptClient()
        {
            try
            {
                while (true)
                {
                    if (_waitServer != null && _waitServer.Poll(0, SelectMode.SelectRead))
                    {
                        Socket add = _waitServer.Accept();
                        _clients.Add(add);
                        Console.WriteLine("Client Accept");
                    }

                    Thread.Sleep(1);
                }
            }
            catch(ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void AddOrder()
        {
            try
            {
                while (true)
                {
                    if (_clients.Count != 0)
                    {
                        for (int n = 0; n < _clients.Count; n++)
                        {
                            if (_clients[n].Poll(0, SelectMode.SelectRead))
                            {
                                byte[] sendBuffer = new byte[1032];

                                try
                                {
                                    int recvLen = _clients[n].Receive(sendBuffer);
                                    if (recvLen > 0)
                                    {
                                        _receiveQueue.Enqueue(new rPacketData(sendBuffer, recvLen, n));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch(ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void DoOrder()
        {
            try
            {
                while (true)
                {
                    if (_receiveQueue.Count != 0)
                    {
                        rPacketData rPacket = _receiveQueue.Dequeue();

                        try
                        {
                            DefinedStructure.PacketInfo packetSend = new DefinedStructure.PacketInfo();
                            packetSend = (DefinedStructure.PacketInfo)ConvertPacket.ByteArrayToStructure(rPacket._buffer, packetSend.GetType(), rPacket._bufferLength);

                            string logMessage = string.Empty;
                            switch ((DefinedProtocol.eSendMessage)packetSend._id)
                            {
                                case DefinedProtocol.eSendMessage.Connect_Message:

                                    DefinedStructure.Packet_LoginAfter clientInfo = new DefinedStructure.Packet_LoginAfter();
                                    clientInfo = (DefinedStructure.Packet_LoginAfter)ConvertPacket.ByteArrayToStructure(packetSend._data, clientInfo.GetType(), packetSend._totalSize);

                                    DefinedStructure.Packet_Login packet_login;
                                    packet_login._UUID = GetUUIDFromDB(clientInfo._name);

                                    ToPacket(DefinedProtocol.eReceiveMessage.Connect_User, packet_login, rPacket._index);

                                    if (_currentUsers.Count != 0)
                                    {
                                        foreach (long key in _currentUsers.Keys)
                                        {
                                            DefinedStructure.Packet_CurrentUser packetCurrent;
                                            packetCurrent._UUID = key;
                                            packetCurrent._avatarIndex = _currentUsers[key];

                                            ToPacket(DefinedProtocol.eReceiveMessage.Current_User, packetCurrent, rPacket._index);
                                        }
                                    }

                                    break;

                                case DefinedProtocol.eSendMessage.Connect_After:

                                    DefinedStructure.Packet_LoginAfter packet_loginAfter = new DefinedStructure.Packet_LoginAfter();
                                    packet_loginAfter = (DefinedStructure.Packet_LoginAfter)ConvertPacket.ByteArrayToStructure(packetSend._data, packet_loginAfter.GetType(), packetSend._totalSize);

                                    _currentUsers.Add(packet_loginAfter._UUID, packet_loginAfter._avatarIndex);
                                    _currentUserName.Add(packet_loginAfter._UUID, packet_loginAfter._name);

                                    logMessage = string.Format("{0} 유저 접속\t\t{1}", packet_loginAfter._name, DateTime.Now);

                                    ToPacket(DefinedProtocol.eReceiveMessage.Connect_After, packet_loginAfter, -999);

                                    break;

                                case DefinedProtocol.eSendMessage.Chatting_Message:

                                    DefinedStructure.Packet_Chat packet_chat = new DefinedStructure.Packet_Chat();
                                    packet_chat = (DefinedStructure.Packet_Chat)ConvertPacket.ByteArrayToStructure(packetSend._data, packet_chat.GetType(), packetSend._totalSize);
                                    packet_chat._chat = string.Format("{0} : {1}", _currentUserName[packet_chat._UUID], packet_chat._chat);

                                    ToPacket(DefinedProtocol.eReceiveMessage.Chatting_Message, packet_chat, -999);

                                    logMessage = string.Format("{0}\t\t{1}", packet_chat._chat, DateTime.Now);

                                    break;

                                case DefinedProtocol.eSendMessage.Disconnect_Message:

                                    DefinedStructure.Packet_Login packet_logOut = new DefinedStructure.Packet_Login();
                                    packet_logOut = (DefinedStructure.Packet_Login)ConvertPacket.ByteArrayToStructure(packetSend._data, packet_logOut.GetType(), packetSend._totalSize);

                                    DefinedStructure.Packet_LoginAfter packet_out;
                                    packet_out._UUID = packet_logOut._UUID;
                                    packet_out._name = _currentUserName[packet_logOut._UUID];
                                    packet_out._avatarIndex = _currentUsers[packet_logOut._UUID];

                                    logMessage = string.Format("{0} 유저 퇴장\t\t{1}", packet_out._name, DateTime.Now);

                                    ToPacket(DefinedProtocol.eReceiveMessage.Disconnect_User, packet_out, -999);

                                    _currentUserName.Remove(packet_logOut._UUID);
                                    _currentUsers.Remove(packet_logOut._UUID);

                                    _clients[rPacket._index].Shutdown(SocketShutdown.Both);
                                    _clients[rPacket._index].Close();
                                    _clients[rPacket._index] = null;
                                    _clients.RemoveAt(rPacket._index);

                                    break;
                            }

                            if (!string.IsNullOrEmpty(logMessage))
                            {
                                _totalLog.Add(logMessage);
                                Console.WriteLine(logMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void SendOrder()
        {
            try
            {
                while (true)
                {
                    if (_sendQueue.Count != 0)
                    {
                        sPacketData sPacket = _sendQueue.Dequeue();

                        if (sPacket._index < 0)
                        {
                            SendToClient(sPacket._buffer);
                        }
                        else
                        {
                            _clients[sPacket._index].Send(sPacket._buffer);
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void SendToClient(byte[] buffer)
        {
            for (int n = 0; n < _clients.Count; n++)
            {
                if (_clients[n] != null)
                    _clients[n].Send(buffer);
            }
        }

        void ToPacket(DefinedProtocol.eReceiveMessage receiveID, object str, int idx)
        {
            DefinedStructure.PacketInfo packetRecieve1;
            packetRecieve1._id = (int)receiveID;
            packetRecieve1._data = new byte[1024];
            byte[] temp = ConvertPacket.StructureToByteArray(str);
            for (int n = 0; n < temp.Length; n++)
                packetRecieve1._data[n] = temp[n];
            packetRecieve1._totalSize = temp.Length;

            _sendQueue.Enqueue(new sPacketData(ConvertPacket.StructureToByteArray(packetRecieve1), idx));
        }

        long GetUUIDFromDB(string name)
        {
            long uuid = 0;

            using (MySqlConnection connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=chattingdata;Uid=root;Pwd=1234;"))
            {
                connection.Open();

                uuid = SearchName(connection, name);

                if (uuid < 0)
                {
                    InsertNewUser(connection, name);
                    uuid = SearchName(connection, name);
                }

                connection.Close();
            }

            return uuid;
        }

        long SearchName(MySqlConnection connection, string name)
        {
            string searchQuery = string.Format("SELECT * FROM stdTable");

            try
            {
                MySqlCommand command = new MySqlCommand(searchQuery, connection);

                MySqlDataReader table = command.ExecuteReader();
                while (table.Read())
                {
                    if (table["UserName"].ToString().Equals(name))
                    {
                        long uuid = long.Parse(table["UUID"].ToString());
                        table.Close();
                        return uuid;
                    }
                }

                table.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("연결 실패!!");
                Console.WriteLine(ex.ToString());
            }

            return -999;
        }

        void InsertNewUser(MySqlConnection connection, string name)
        {
            string insertQuery = string.Format("INSERT INTO stdTable(UserName,JoinDate) VALUES('{0}','{1}')", name, DateTime.Now);

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

        void SaveLog()
        {
            //TODO 나중에 시간나면 로그 파일 저장 날짜별로 (형태는 마음대로)
        }

        public void MainLoop()
        {
            t_acceptClnt = new Thread(new ThreadStart(AcceptClient));
            t_addOrder = new Thread(new ThreadStart(AddOrder));
            t_doOrder = new Thread(new ThreadStart(DoOrder));
            t_sendOrder = new Thread(new ThreadStart(SendOrder));

            if (!t_acceptClnt.IsAlive)
                t_acceptClnt.Start();

            if (!t_addOrder.IsAlive)
                t_addOrder.Start();

            if (!t_doOrder.IsAlive)
                t_doOrder.Start();

            if (!t_sendOrder.IsAlive)
                t_sendOrder.Start();
        }

        public void ExitProgram()
        {
            if (t_acceptClnt.IsAlive)
                t_acceptClnt.Interrupt();

            if (t_addOrder.IsAlive)
                t_addOrder.Interrupt();

            if (t_doOrder.IsAlive)
                t_doOrder.Interrupt();

            if (t_sendOrder.IsAlive)
                t_sendOrder.Interrupt();

            t_acceptClnt.Join();
            t_addOrder.Join();
            t_doOrder.Join();
            t_sendOrder.Join();
        }
    }
}
