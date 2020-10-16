using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server_CardBattle
{
    public struct ClientInfo
    {
        public string _name;
        public int _avatarIdx;

        public ClientInfo(string name, int avatarIdx)
        {
            _name = name;
            _avatarIdx = avatarIdx;
        }
    }

    class UpgradeServer
    {
        const short _port = 80;
        Socket _waitServer;

        long _startUUID = 1000000000;

        SocketManagerClass _socketManager = new SocketManagerClass();

        Queue<PacketClass> _fromClientQueue = new Queue<PacketClass>();
        Queue<PacketClass> _toClientQueue = new Queue<PacketClass>();

        Dictionary<long, ClientInfo> _clientsDic = new Dictionary<long, ClientInfo>();
        Dictionary<long, int> _userScoreDic = new Dictionary<long, int>();
        Dictionary<int, int> _selectedCardNumDic = new Dictionary<int, int>();
        Dictionary<int, ServerInfo.RoomInfo> _roomInfoDic = new Dictionary<int, ServerInfo.RoomInfo>();
        Dictionary<int, int[]> _iconIndexesDic = new Dictionary<int, int[]>();
        Dictionary<int, bool[]> _isClickableDic = new Dictionary<int, bool[]>();

        const int _cardCount = 24;
        int _currentRoomNumber = 1;

        Thread _tAccept;
        Thread _tFromClient;
        Thread _tToClient;

        public UpgradeServer()
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
                        Socket add = _waitServer.Accept();

                        SocketClass socket = new SocketClass(add, _startUUID += 1);
                        _socketManager.AddSocket(socket);

                        Console.WriteLine("Client Accept");
                    }

                    _socketManager.AddFromQueue(_fromClientQueue);

                    Thread.Sleep(10);
                }
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        void FromClientQueue()
        {
            try
            {
                while (true)
                {
                    if (_fromClientQueue.Count != 0)
                    {
                        PacketClass packet = _fromClientQueue.Dequeue();

                        try
                        {
                            string logMessage = string.Empty;
                            switch ((DefinedProtocol.eFromClient)packet._ProtocolID)
                            {
                                #region 유저 입장 부분
                                case DefinedProtocol.eFromClient.Connect:

                                    DefinedStructure.Packet_Connect pConnect = new DefinedStructure.Packet_Connect();
                                    pConnect = (DefinedStructure.Packet_Connect)packet.Convert(pConnect.GetType());

                                    DefinedStructure.Packet_CheckConnect pCheckConnect;
                                    pCheckConnect._UUID = packet._UUID;

                                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.CheckConnect, pCheckConnect, packet._UUID));

                                    ClientInfo cInfo = new ClientInfo(pConnect._name, pConnect._avatarIndex);
                                    _clientsDic.Add(packet._UUID, cInfo);

                                    _userScoreDic.Add(pCheckConnect._UUID, 0);

                                    if (_roomInfoDic.Count != 0)
                                    {
                                        foreach (int key in _roomInfoDic.Keys)
                                        {
                                            DefinedStructure.Packet_ShowRoomInfo pShowRoomInfo;
                                            pShowRoomInfo._roomNumber = _roomInfoDic[key]._roomNumber;
                                            pShowRoomInfo._roomName = _roomInfoDic[key]._name;
                                            pShowRoomInfo._isLock = _roomInfoDic[key]._isLock ? 0 : 1;
                                            pShowRoomInfo._currentMemberNum = _roomInfoDic[key]._currentMember;

                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowRoomInfo, pShowRoomInfo, packet._UUID));
                                        }
                                    }

                                    logMessage = string.Format("{0} 유저가 입장\t\t{1}", pConnect._name, DateTime.Now);
                                    break;
                                #endregion

                                #region 로비 / 방 입장 부분
                                case DefinedProtocol.eFromClient.CreateRoom:

                                    DefinedStructure.Packet_CreateRoom pCreateRoom = new DefinedStructure.Packet_CreateRoom();
                                    pCreateRoom = (DefinedStructure.Packet_CreateRoom)packet.Convert(pCreateRoom.GetType());

                                    ServerInfo.RoomInfo roomInfo = new ServerInfo.RoomInfo();
                                    roomInfo._roomNumber = _currentRoomNumber;
                                    roomInfo._name = pCreateRoom._roomName;
                                    roomInfo._isLock = pCreateRoom._isLock.Equals(0);
                                    roomInfo._pw = pCreateRoom._pw;
                                    roomInfo._currentMember = 1;
                                    roomInfo._memberIdx = new List<long>();
                                    roomInfo._memberIdx.Add(packet._UUID);
                                    roomInfo._currentOrderIdx = 0;
                                    roomInfo._AI = new List<CardBattleAI>();
                                    roomInfo._AI.Add(new CardBattleAI(CardBattleAI.eAIDifficulty.Hard));
                                    roomInfo._currentAIOrder = 0;
                                    roomInfo._isAITurn = false;

                                    logMessage = string.Format("{0} 유저가 {1}번 방을 만들었습니다.", _clientsDic[packet._UUID]._name, _currentRoomNumber);

                                    _roomInfoDic.Add(_currentRoomNumber, roomInfo);
                                    _selectedCardNumDic.Add(_currentRoomNumber, 0);
                                    _currentRoomNumber++;

                                    DefinedStructure.Packet_AfterCreateRoom pAfterCreateRoom;
                                    pAfterCreateRoom._roomNumber = roomInfo._roomNumber;

                                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.AfterCreateRoom, pAfterCreateRoom, packet._UUID));

                                    break;

                                case DefinedProtocol.eFromClient.EnterRoom:

                                    DefinedStructure.Packet_EnterRoom pEnterRoom = new DefinedStructure.Packet_EnterRoom();
                                    pEnterRoom = (DefinedStructure.Packet_EnterRoom)packet.Convert(pEnterRoom.GetType());

                                    if (_roomInfoDic[pEnterRoom._roomNumber]._isLock)
                                    {
                                        if (_roomInfoDic[pEnterRoom._roomNumber]._pw.Equals(pEnterRoom._pw))
                                        {
                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.SuccessEnterRoom, null, packet._UUID));
                                            EnterRoom(pEnterRoom._roomNumber, packet._UUID);

                                            logMessage = string.Format("{0} 유저가 {1}번 방에 입장하였습니다.", _clientsDic[packet._UUID]._name, pEnterRoom._roomNumber);
                                        }
                                        else
                                        {
                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.FailEnterRoom, null, packet._UUID));
                                            logMessage = string.Format("{0} 유저가 {1}번 방 입장에 실패하였습니다.", _clientsDic[packet._UUID]._name, pEnterRoom._roomNumber);
                                        }
                                    }
                                    else
                                    {
                                        _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.SuccessEnterRoom, null, packet._UUID));
                                        EnterRoom(pEnterRoom._roomNumber, packet._UUID);

                                        logMessage = string.Format("{0} 유저가 {1}번 방에 입장하였습니다.", _clientsDic[packet._UUID]._name, pEnterRoom._roomNumber);
                                    }

                                    if (_roomInfoDic[pEnterRoom._roomNumber]._memberIdx.Count >= 3)
                                    {
                                        Console.WriteLine("{0}방에서 게임이 시작되었습니다.", pEnterRoom._roomNumber);

                                        _isClickableDic.Add(pEnterRoom._roomNumber, new bool[_cardCount]);

                                        DefinedStructure.Packet_NextTurn pNextTurn;
                                        pNextTurn._name = _clientsDic[_roomInfoDic[pEnterRoom._roomNumber]._memberIdx[_roomInfoDic[pEnterRoom._roomNumber]._currentOrderIdx]]._name;

                                        Console.WriteLine("이번 턴은 {0} 유저입니다.", pNextTurn._name);

                                        SendBufferInRoom(DefinedProtocol.eToClient.NextTurn, _roomInfoDic[pEnterRoom._roomNumber]._memberIdx, pNextTurn);

                                        MixCard(pEnterRoom._roomNumber);

                                        SendBufferInRoom(DefinedProtocol.eToClient.GameStart, _roomInfoDic[pEnterRoom._roomNumber]._memberIdx, null);
                                    }

                                    break;
                                #endregion

                                #region 게임 실행 부분
                                case DefinedProtocol.eFromClient.ChooseCard:

                                    DefinedStructure.Packet_ChooseCard pChooseCard = new DefinedStructure.Packet_ChooseCard();
                                    pChooseCard = (DefinedStructure.Packet_ChooseCard)packet.Convert(pChooseCard.GetType());

                                    DefinedStructure.Packet_ChooseInfo pChooseInfo;
                                    pChooseInfo._cardIdx1 = pChooseCard._cardIdx1;
                                    pChooseInfo._cardIdx2 = pChooseCard._cardIdx2;
                                    pChooseInfo._cardImgIdx1 = _iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx1];
                                    pChooseInfo._cardImgIdx2 = _iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx2];

                                    if(!_roomInfoDic[pChooseCard._roomNumber]._isAITurn)
                                        Console.WriteLine(string.Format("{0} 유저가 {1}번 카드와 {2}번 카드를 선택 하였습니다.", _clientsDic[packet._UUID]._name, pChooseCard._cardIdx1, pChooseCard._cardIdx2));

                                    SendBufferInRoom(DefinedProtocol.eToClient.ChooseInfo, _roomInfoDic[pChooseCard._roomNumber]._memberIdx, pChooseInfo);

                                    DefinedStructure.Packet_ChooseResult pChooseResult;
                                    pChooseResult._name = _roomInfoDic[pChooseCard._roomNumber]._isAITurn?"AI":_clientsDic[pChooseCard._UUID]._name;
                                    pChooseResult._cardIdx1 = pChooseCard._cardIdx1;
                                    pChooseResult._cardIdx2 = pChooseCard._cardIdx2;

                                    if (_iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx1] == _iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx2])
                                    {
                                        pChooseResult._isSuccess = 0;

                                        if (!_roomInfoDic[pChooseCard._roomNumber]._isAITurn)
                                            _userScoreDic[pChooseCard._UUID]++;

                                        _selectedCardNumDic[pChooseCard._roomNumber]++;

                                        if (!_roomInfoDic[pChooseCard._roomNumber]._isAITurn)
                                            Console.WriteLine(string.Format("{0} 유저가 카드를 성공적으로 골랐습니다.", _clientsDic[packet._UUID]._name));

                                        _isClickableDic[pChooseCard._roomNumber][pChooseCard._cardIdx1] = true;
                                        _isClickableDic[pChooseCard._roomNumber][pChooseCard._cardIdx2] = true;

                                        for (int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._AI.Count; n++)
                                            _roomInfoDic[pChooseCard._roomNumber]._AI[n].RemoveMemory(pChooseCard._cardIdx1, pChooseCard._cardIdx2);
                                    }
                                    else
                                    {
                                        pChooseResult._isSuccess = 1;
                                        if (!_roomInfoDic[pChooseCard._roomNumber]._isAITurn)
                                            Console.WriteLine(string.Format("{0} 유저가 카드를 고르는데 실패했습니다.", _clientsDic[packet._UUID]._name));

                                        for (int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._AI.Count; n++)
                                            _roomInfoDic[pChooseCard._roomNumber]._AI[n].SaveMemory(pChooseCard._cardIdx1, pChooseCard._cardIdx2);
                                    }

                                    SendBufferInRoom(DefinedProtocol.eToClient.ChooseResult, _roomInfoDic[pChooseCard._roomNumber]._memberIdx, pChooseResult);

                                    if (_selectedCardNumDic[pChooseCard._roomNumber] >= _cardCount / 2)
                                    {
                                        Console.WriteLine(string.Format("{0}번 방에서 게임이 끝났습니다.", pChooseCard._roomNumber));
                                        int highScore = int.MinValue;
                                        long winPlayerUUID = 0;

                                        foreach (long key in _userScoreDic.Keys)
                                        {
                                            if (highScore < _userScoreDic[key])
                                            {
                                                highScore = _userScoreDic[key];
                                                winPlayerUUID = key;
                                            }
                                        }

                                        Console.WriteLine(string.Format("{0}번 방의 게임 승자는 {1}입니다.", pChooseCard._roomNumber, _clientsDic[winPlayerUUID]._name));

                                        DefinedStructure.Packet_GameResult pGameResult;
                                        pGameResult._name = _clientsDic[winPlayerUUID]._name;

                                        SendBufferInRoom(DefinedProtocol.eToClient.GameResult, _roomInfoDic[pChooseCard._roomNumber]._memberIdx, pGameResult);
                                    }
                                    else
                                    {
                                        DefinedStructure.Packet_NextTurn pNextTurn2;

                                        if (!_roomInfoDic[pChooseCard._roomNumber]._isAITurn)
                                        {
                                            if (++_roomInfoDic[pChooseCard._roomNumber]._currentOrderIdx >= _roomInfoDic[pChooseCard._roomNumber]._memberIdx.Count)
                                            {
                                                _roomInfoDic[pChooseCard._roomNumber]._isAITurn = true;
                                                _roomInfoDic[pChooseCard._roomNumber]._currentOrderIdx = 0;
                                                pNextTurn2._name = "AI";
                                                TurnAI(pChooseCard._roomNumber, _roomInfoDic[pChooseCard._roomNumber]._AI[_roomInfoDic[pChooseCard._roomNumber]._currentAIOrder]);
                                            }
                                            else
                                            {
                                                pNextTurn2._name = _clientsDic[_roomInfoDic[pChooseCard._roomNumber]._memberIdx[_roomInfoDic[pChooseCard._roomNumber]._currentOrderIdx]]._name;
                                            }
                                        }
                                        else
                                        {
                                            if(++_roomInfoDic[pChooseCard._roomNumber]._currentAIOrder >= _roomInfoDic[pChooseCard._roomNumber]._AI.Count)
                                            {
                                                _roomInfoDic[pChooseCard._roomNumber]._isAITurn = false;
                                                _roomInfoDic[pChooseCard._roomNumber]._currentAIOrder = 0;
                                                pNextTurn2._name = _clientsDic[_roomInfoDic[pChooseCard._roomNumber]._memberIdx[_roomInfoDic[pChooseCard._roomNumber]._currentOrderIdx]]._name;
                                            }
                                            else
                                            {
                                                pNextTurn2._name = "AI";
                                                TurnAI(pChooseCard._roomNumber, _roomInfoDic[pChooseCard._roomNumber]._AI[_roomInfoDic[pChooseCard._roomNumber]._currentAIOrder]);
                                            }
                                        }

                                        Console.WriteLine("이번 턴은 {0} 유저입니다.", pNextTurn2._name);

                                        SendBufferInRoom(DefinedProtocol.eToClient.NextTurn, _roomInfoDic[pChooseCard._roomNumber]._memberIdx, pNextTurn2);
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
            catch(ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        void ToClientQueue()
        {
            try
            {
                while (true)
                {
                    if (_toClientQueue.Count != 0)
                    {
                        PacketClass packet = _toClientQueue.Dequeue();

                        if (packet._UUID < 0)
                            _socketManager.SendAll(packet._Data);
                        else
                            _socketManager.Send(packet._Data, packet._UUID);
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

        void MixCard(int roomNum)
        {
            Random rd = new Random();

            if (!_iconIndexesDic.ContainsKey(roomNum))
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
                pShowUser._avatarIndex = _clientsDic[_roomInfoDic[roomNum]._memberIdx[n]]._avatarIdx;

                for (int m = 0; m < _roomInfoDic[roomNum]._memberIdx.Count; m++)
                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowUserInfo, pShowUser, _roomInfoDic[roomNum]._memberIdx[m]));
            }
        }

        void SendBufferInRoom(DefinedProtocol.eToClient type, List<long> userUUIDLists, object str)
        {
            for (int n = 0; n < userUUIDLists.Count; n++)
                _toClientQueue.Enqueue(_socketManager.AddToQueue(type, str, userUUIDLists[n]));
        }

        void TurnAI(int roomNum, CardBattleAI ai)
        {
            int[] select;

            if(!ai.Check(_iconIndexesDic[roomNum], out select))
            {
                Random rd = new Random();
                select = new int[2];

                do
                {
                    select[0] = rd.Next(0, _cardCount);
                }
                while (_isClickableDic[roomNum][select[0]]);

                do
                {
                    select[1] = rd.Next(0, _cardCount);
                }
                while (select[0] == select[1] && _isClickableDic[roomNum][select[0]]);
            }

            DefinedStructure.Packet_ChooseCard pChooseCard;
            pChooseCard._UUID = 0;
            pChooseCard._roomNumber = roomNum;
            pChooseCard._cardIdx1 = select[0];
            pChooseCard._cardIdx2 = select[1];

            DefinedStructure.PacketInfo packetInfo;
            packetInfo._id = (int)DefinedProtocol.eFromClient.ChooseCard;
            packetInfo._data = ConvertPacket.StructureToByteArray(pChooseCard);
            packetInfo._totalSize = packetInfo._data.Length;

            PacketClass packet = new PacketClass(packetInfo._id, packetInfo._data, packetInfo._totalSize);
            _fromClientQueue.Enqueue(packet);
        }

        public void MainLoop()
        {
            _tAccept = new Thread(new ThreadStart(Accept));
            _tFromClient = new Thread(new ThreadStart(FromClientQueue));
            _tToClient = new Thread(new ThreadStart(ToClientQueue));

            if (!_tAccept.IsAlive)
                _tAccept.Start();

            if (!_tFromClient.IsAlive)
                _tFromClient.Start();

            if (!_tToClient.IsAlive)
                _tToClient.Start();
        }

        public void ExitProgram()
        {
            if (_tAccept.IsAlive)
            {
                _tAccept.Interrupt();
                _tAccept.Join();
            }
            if (_tFromClient.IsAlive)
            {
                _tFromClient.Interrupt();
                _tFromClient.Join();
            }
            if (_tToClient.IsAlive)
            {
                _tToClient.Interrupt();
                _tToClient.Join();
            }
        }
    }
}
