using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Server_CardBattle
{
    public struct FromClientPacket
    {
        public byte[] _buffer;
        public int _bufferLength;
        public long _UUID;

        public FromClientPacket(byte[] buffer, int size, long uuid)
        {
            _buffer = buffer;
            _bufferLength = size;
            _UUID = uuid;
        }
    }

    public struct ToClientPacket
    {
        public byte[] _buffer;
        public long _UUID;

        public ToClientPacket(byte[] buffer, long uuid)
        {
            _buffer = buffer;
            _UUID = uuid;
        }
    }

    public struct ClientSocket
    {
        public long _UUID;
        public string _name;
        public int _avartarIdx;
        public Socket _client;

        public ClientSocket(long uuid, Socket client)
        {
            _UUID = uuid;
            _name = string.Empty;
            _avartarIdx = 0;
            _client = client;
        }
    }

    class MainServer
    {
        const short _port = 80;
        Socket _waitServer;

        long _startUUID = 1000000000;
        Dictionary<long, ClientSocket> _clientsDic = new Dictionary<long, ClientSocket>();
        
        Queue<FromClientPacket> _fromClientQueue = new Queue<FromClientPacket>();
        Queue<ToClientPacket> _toClientQueue = new Queue<ToClientPacket>();

        int _currentRoomNumber = 1;

        Dictionary<int, ServerInfo.RoomInfo> _roomInfoDic = new Dictionary<int, ServerInfo.RoomInfo>();

        Dictionary<long, int> _userScoreDic = new Dictionary<long, int>();

        Dictionary<int, int[]> _iconIndexesDic = new Dictionary<int, int[]>();
        const int _cardCount = 24;

        int _selectedCardNum = 0;

        Thread _tAddOrder;
        Thread _tDoOrder;
        Thread _tSendOrder;

        public MainServer()
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

        void AddOrder()
        {
            try
            {
                while (true)
                {
                    if (_waitServer != null && _waitServer.Poll(0, SelectMode.SelectRead))
                    {
                        Socket add = _waitServer.Accept();

                        ClientSocket cSocket = new ClientSocket(_startUUID += 1, add);
                        _clientsDic.Add(_startUUID, cSocket);

                        Console.WriteLine("Client Accept");
                    }

                    if (_clientsDic.Count != 0)
                    {
                        lock (_clientsDic)
                        {
                            foreach (long key in _clientsDic.Keys)
                            {
                                if (_clientsDic[key]._client.Poll(0, SelectMode.SelectRead))
                                {
                                    byte[] sendBuffer = new byte[1032];

                                    try
                                    {
                                        int recvLen = _clientsDic[key]._client.Receive(sendBuffer);
                                        if (recvLen > 0)
                                        {
                                            _fromClientQueue.Enqueue(new FromClientPacket(sendBuffer, recvLen, key));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(10);
                }
            }
            catch (ThreadInterruptedException e)
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
                    if (_fromClientQueue.Count != 0)
                    {
                        FromClientPacket fPacket = _fromClientQueue.Dequeue();

                        try
                        {
                            DefinedStructure.PacketInfo packet_fromClient = new DefinedStructure.PacketInfo();
                            packet_fromClient = (DefinedStructure.PacketInfo)ConvertPacket.ByteArrayToStructure(fPacket._buffer, packet_fromClient.GetType(), fPacket._bufferLength);

                            string logMessage = string.Empty;
                            switch ((DefinedProtocol.eFromClient)packet_fromClient._id)
                            {
                                #region 유저 입장 부분
                                case DefinedProtocol.eFromClient.Connect:

                                    DefinedStructure.Packet_Connect pConnect = new DefinedStructure.Packet_Connect();
                                    pConnect = (DefinedStructure.Packet_Connect)ConvertPacket.ByteArrayToStructure(packet_fromClient._data, pConnect.GetType(), packet_fromClient._totalSize);

                                    DefinedStructure.Packet_CheckConnect pCheckConnect;
                                    //pCheckConnect._UUID = GetUUIDFromDB(pConnect._name);
                                    pCheckConnect._UUID = fPacket._UUID;

                                    ToPacket(DefinedProtocol.eToClient.CheckConnect, pCheckConnect, fPacket._UUID);

                                    ClientSocket cSocket = _clientsDic[fPacket._UUID];
                                    cSocket._name = pConnect._name;
                                    cSocket._avartarIdx = pConnect._avatarIndex;

                                    lock(_clientsDic)
                                    {
                                        _clientsDic[fPacket._UUID] = cSocket;
                                    }

                                    _userScoreDic.Add(pCheckConnect._UUID, 0);

                                    if(_roomInfoDic.Count != 0)
                                    {
                                        foreach(int key in _roomInfoDic.Keys)
                                        {
                                            DefinedStructure.Packet_ShowRoomInfo pShowRoomInfo;
                                            pShowRoomInfo._roomNumber = _roomInfoDic[key]._roomNumber;
                                            pShowRoomInfo._roomName = _roomInfoDic[key]._name;
                                            pShowRoomInfo._isLock = _roomInfoDic[key]._isLock ? 0 : 1;
                                            pShowRoomInfo._currentMemberNum = _roomInfoDic[key]._currentMember;

                                            ToPacket(DefinedProtocol.eToClient.ShowRoomInfo, pShowRoomInfo, fPacket._UUID);
                                        }
                                    }

                                    logMessage = string.Format("{0} 유저가 입장\t\t{1}", pConnect._name, DateTime.Now);

                                    break;
                                #endregion

                                #region 로비 / 방 입장 부분
                                case DefinedProtocol.eFromClient.CreateRoom:

                                    DefinedStructure.Packet_CreateRoom pCreateRoom = new DefinedStructure.Packet_CreateRoom();
                                    pCreateRoom = (DefinedStructure.Packet_CreateRoom)ConvertPacket.ByteArrayToStructure(packet_fromClient._data, pCreateRoom.GetType(), packet_fromClient._totalSize);

                                    ServerInfo.RoomInfo roomInfo = new ServerInfo.RoomInfo();
                                    roomInfo._roomNumber = _currentRoomNumber;
                                    roomInfo._name = pCreateRoom._roomName;
                                    roomInfo._isLock = pCreateRoom._isLock.Equals(0);
                                    roomInfo._pw = pCreateRoom._pw;
                                    roomInfo._currentMember = 1;
                                    roomInfo._memberIdx = new List<long>();
                                    roomInfo._memberIdx.Add(fPacket._UUID);
                                    roomInfo._currentOrderIdx = 0;

                                    logMessage = string.Format("{0} 유저가 {1}번 방을 만들었습니다.", _clientsDic[fPacket._UUID]._name, _currentRoomNumber);

                                    _roomInfoDic.Add(_currentRoomNumber, roomInfo);
                                    _currentRoomNumber++;

                                    DefinedStructure.Packet_AfterCreateRoom pAfterCreateRoom;
                                    pAfterCreateRoom._roomNumber = roomInfo._roomNumber;

                                    ToPacket(DefinedProtocol.eToClient.AfterCreateRoom, pAfterCreateRoom, fPacket._UUID);

                                    break;

                                case DefinedProtocol.eFromClient.EnterRoom:

                                    DefinedStructure.Packet_EnterRoom pEnterRoom = new DefinedStructure.Packet_EnterRoom();
                                    pEnterRoom = (DefinedStructure.Packet_EnterRoom)ConvertPacket.ByteArrayToStructure(packet_fromClient._data, pEnterRoom.GetType(), packet_fromClient._totalSize);

                                    if(_roomInfoDic[pEnterRoom._roomNumber]._isLock)
                                    {
                                        if(_roomInfoDic[pEnterRoom._roomNumber]._pw.Equals(pEnterRoom._pw))
                                        {
                                            ToPacket(DefinedProtocol.eToClient.SuccessEnterRoom, null, fPacket._UUID);
                                            EnterRoom(pEnterRoom._roomNumber, fPacket._UUID);

                                            logMessage = string.Format("{0} 유저가 {1}번 방에 입장하였습니다.", _clientsDic[fPacket._UUID]._name, pEnterRoom._roomNumber);
                                        }
                                        else
                                        {
                                            ToPacket(DefinedProtocol.eToClient.FailEnterRoom, null, fPacket._UUID);
                                            logMessage = string.Format("{0} 유저가 {1}번 방 입장에 실패하였습니다.", _clientsDic[fPacket._UUID]._name, pEnterRoom._roomNumber);
                                        }
                                    }
                                    else
                                    {
                                        ToPacket(DefinedProtocol.eToClient.SuccessEnterRoom, null, fPacket._UUID);
                                        EnterRoom(pEnterRoom._roomNumber, fPacket._UUID);

                                        logMessage = string.Format("{0} 유저가 {1}번 방에 입장하였습니다.", _clientsDic[fPacket._UUID]._name, pEnterRoom._roomNumber);
                                    }

                                    if(_roomInfoDic[pEnterRoom._roomNumber]._memberIdx.Count >= 3)
                                    {
                                        Console.WriteLine("{0}방에서 게임이 시작되었습니다.", pEnterRoom._roomNumber);

                                        DefinedStructure.Packet_NextTurn pNextTurn;
                                        pNextTurn._name = _clientsDic[_roomInfoDic[pEnterRoom._roomNumber]._memberIdx[_roomInfoDic[pEnterRoom._roomNumber]._currentOrderIdx]]._name;

                                        Console.WriteLine("이번 턴은 {0} 유저입니다.", pNextTurn._name);

                                        for (int n = 0; n < _roomInfoDic[pEnterRoom._roomNumber]._memberIdx.Count; n++)
                                        {
                                            ToPacket(DefinedProtocol.eToClient.NextTurn, pNextTurn, _roomInfoDic[pEnterRoom._roomNumber]._memberIdx[n]);
                                        }

                                        MixCard(pEnterRoom._roomNumber);

                                        for (int n = 0; n < _roomInfoDic[pEnterRoom._roomNumber]._memberIdx.Count; n++)
                                        {
                                            ToPacket(DefinedProtocol.eToClient.GameStart, null, _roomInfoDic[pEnterRoom._roomNumber]._memberIdx[n]);
                                        }
                                    }

                                    break;
                                #endregion

                                #region 게임 실행 부분
                                case DefinedProtocol.eFromClient.ChooseCard:

                                    DefinedStructure.Packet_ChooseCard pChooseCard = new DefinedStructure.Packet_ChooseCard();
                                    pChooseCard = (DefinedStructure.Packet_ChooseCard)ConvertPacket.ByteArrayToStructure(packet_fromClient._data, pChooseCard.GetType(), packet_fromClient._totalSize);

                                    DefinedStructure.Packet_ChooseInfo pChooseInfo;
                                    pChooseInfo._cardIdx1 = pChooseCard._cardIdx1;
                                    pChooseInfo._cardIdx2 = pChooseCard._cardIdx2;
                                    pChooseInfo._cardImgIdx1 = _iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx1];
                                    pChooseInfo._cardImgIdx2 = _iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx2];

                                    Console.WriteLine(string.Format("{0} 유저가 {1}번 카드와 {2}번 카드를 선택 하였습니다.", _clientsDic[fPacket._UUID]._name, pChooseCard._cardIdx1, pChooseCard._cardIdx2));

                                    for(int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._memberIdx.Count; n++)
                                    {
                                        ToPacket(DefinedProtocol.eToClient.ChooseInfo, pChooseInfo, _roomInfoDic[pChooseCard._roomNumber]._memberIdx[n]);
                                    }

                                    DefinedStructure.Packet_ChooseResult pChooseResult;
                                    pChooseResult._name = _clientsDic[pChooseCard._UUID]._name;
                                    pChooseResult._cardIdx1 = pChooseCard._cardIdx1;
                                    pChooseResult._cardIdx2 = pChooseCard._cardIdx2;

                                    if (_iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx1] == _iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx2])
                                    {
                                        pChooseResult._isSuccess = 0;
                                        _userScoreDic[pChooseCard._UUID]++;
                                        _selectedCardNum++;
                                        Console.WriteLine(string.Format("{0} 유저가 카드를 성공적으로 골랐습니다.", _clientsDic[fPacket._UUID]._name));
                                    }   
                                    else
                                    {
                                        pChooseResult._isSuccess = 1;
                                        Console.WriteLine(string.Format("{0} 유저가 카드를 고르는데 실패했습니다.", _clientsDic[fPacket._UUID]._name));
                                    }   

                                    for (int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._memberIdx.Count; n++)
                                    {
                                        ToPacket(DefinedProtocol.eToClient.ChooseResult, pChooseResult, _roomInfoDic[pChooseCard._roomNumber]._memberIdx[n]);
                                    }

                                    if(_selectedCardNum >= _cardCount / 2)
                                    {
                                        Console.WriteLine(string.Format("{0}번 방에서 게임이 끝났습니다.", pChooseCard._roomNumber));
                                        int highScore = int.MinValue;
                                        long winPlayerUUID = 0;

                                        foreach(long key in _userScoreDic.Keys)
                                        {
                                            if(highScore < _userScoreDic[key])
                                            {
                                                highScore = _userScoreDic[key];
                                                winPlayerUUID = key;
                                            }
                                        }

                                        Console.WriteLine(string.Format("{0}번 방의 게임 승자는 {1}입니다.", pChooseCard._roomNumber, _clientsDic[winPlayerUUID]._name));

                                        DefinedStructure.Packet_GameResult pGameResult;
                                        pGameResult._name = _clientsDic[winPlayerUUID]._name;

                                        for (int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._memberIdx.Count; n++)
                                        {
                                            ToPacket(DefinedProtocol.eToClient.GameResult, pGameResult, _roomInfoDic[pChooseCard._roomNumber]._memberIdx[n]);
                                        }
                                    }
                                    else
                                    {
                                        DefinedStructure.Packet_NextTurn pNextTurn2;

                                        if (++_roomInfoDic[pChooseCard._roomNumber]._currentOrderIdx >= _roomInfoDic[pChooseCard._roomNumber]._memberIdx.Count)
                                            _roomInfoDic[pChooseCard._roomNumber]._currentOrderIdx = 0;

                                        pNextTurn2._name = _clientsDic[_roomInfoDic[pChooseCard._roomNumber]._memberIdx[_roomInfoDic[pChooseCard._roomNumber]._currentOrderIdx]]._name;

                                        Console.WriteLine("이번 턴은 {0} 유저입니다.", pNextTurn2._name);

                                        for (int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._memberIdx.Count; n++)
                                        {
                                            ToPacket(DefinedProtocol.eToClient.NextTurn, pNextTurn2, _roomInfoDic[pChooseCard._roomNumber]._memberIdx[n]);
                                        }
                                    }

                                    break;
                                    #endregion

                            }

                            if (!string.IsNullOrEmpty(logMessage))
                            {   
                                Console.WriteLine(logMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }

                    Thread.Sleep(10);
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
                    if (_toClientQueue.Count != 0)
                    {
                        ToClientPacket tPacket = _toClientQueue.Dequeue();

                        if (tPacket._UUID < 0)
                        {
                            SendToClient(tPacket._buffer);
                        }
                        else
                        {
                            _clientsDic[tPacket._UUID]._client.Send(tPacket._buffer);
                        }
                    }

                    Thread.Sleep(10);
                }
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void SendToClient(byte[] buffer)
        {
            foreach(long key in _clientsDic.Keys)
            {
                if (_clientsDic[key]._client != null)
                    _clientsDic[key]._client.Send(buffer);
            }
        }

        void ToPacket(DefinedProtocol.eToClient toClientID, object str, long uuid)
        {
            DefinedStructure.PacketInfo packetRecieve1;
            packetRecieve1._id = (int)toClientID;
            packetRecieve1._data = new byte[1024];

            if(str != null)
            {
                byte[] temp = ConvertPacket.StructureToByteArray(str);
                for (int n = 0; n < temp.Length; n++)
                    packetRecieve1._data[n] = temp[n];
                packetRecieve1._totalSize = temp.Length;
            }
            else
            {
                packetRecieve1._totalSize = packetRecieve1._data.Length;
            }

            _toClientQueue.Enqueue(new ToClientPacket(ConvertPacket.StructureToByteArray(packetRecieve1), uuid));
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

        void MixCard(int roomNum)
        {
            Random rd = new Random();

            if(!_iconIndexesDic.ContainsKey(roomNum))
                _iconIndexesDic.Add(roomNum, new int[_cardCount]);

            for (int n = 0; n < _iconIndexesDic[roomNum].Length; n++)
            {
                _iconIndexesDic[roomNum][n] = n / 2;
            }

            for (int n = 0; n < _iconIndexesDic[roomNum].Length; n++)
            {
                int rid = rd.Next(0, _iconIndexesDic[roomNum].Length);
                int td = _iconIndexesDic[roomNum][n];
                _iconIndexesDic[roomNum][n] = _iconIndexesDic[roomNum][rid];
                _iconIndexesDic[roomNum][rid] = td;
            }
        }

        void EnterRoom(int roomNum, long uuid)
        {
            _roomInfoDic[roomNum]._memberIdx.Add(uuid);
            _roomInfoDic[roomNum]._currentMember = _roomInfoDic[roomNum]._memberIdx.Count;

            for (int n = 0; n < _roomInfoDic[roomNum]._memberIdx.Count; n++)
            {
                DefinedStructure.Packet_Connect pShowUser;
                pShowUser._name = _clientsDic[_roomInfoDic[roomNum]._memberIdx[n]]._name;
                pShowUser._avatarIndex = _clientsDic[_roomInfoDic[roomNum]._memberIdx[n]]._avartarIdx;

                for (int m = 0; m < _roomInfoDic[roomNum]._memberIdx.Count; m++)
                {
                    ToPacket(DefinedProtocol.eToClient.ShowUserInfo, pShowUser, _roomInfoDic[roomNum]._memberIdx[m]);
                }
            }
        }

        public void MainLoop()
        {
            _tAddOrder = new Thread(new ThreadStart(AddOrder));
            _tDoOrder = new Thread(new ThreadStart(DoOrder));
            _tSendOrder = new Thread(new ThreadStart(SendOrder));

            if (!_tAddOrder.IsAlive)
                _tAddOrder.Start();

            if (!_tDoOrder.IsAlive)
                _tDoOrder.Start();

            if (!_tSendOrder.IsAlive)
                _tSendOrder.Start();
        }

        public void ExitProgram()
        {
            if(_tAddOrder.IsAlive)
            {
                _tAddOrder.Interrupt();
                _tAddOrder.Join();
            }
            if(_tDoOrder.IsAlive)
            {
                _tDoOrder.Interrupt();
                _tDoOrder.Join();
            }
            if(_tSendOrder.IsAlive)
            {
                _tSendOrder.Interrupt();
                _tSendOrder.Join();
            }
        }
    }
}
