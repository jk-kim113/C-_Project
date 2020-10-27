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
    class UpgradeServer
    {
        const short _port = 80;
        Socket _waitServer;

        const string _dbIP = "127.0.0.1";
        const int _dbPort = 81;
        Socket _dbServer;

        SocketManagerClass _socketManager = new SocketManagerClass();

        Queue<PacketClass> _fromClientQueue = new Queue<PacketClass>();
        Queue<PacketClass> _toClientQueue = new Queue<PacketClass>();

        Queue<PacketClass> _fromServerQueue = new Queue<PacketClass>();
        Queue<PacketClass> _toServerQueue = new Queue<PacketClass>();

        Dictionary<int, int> _selectedCardNumDic = new Dictionary<int, int>();
        Dictionary<int, ServerInfo.RoomInfo> _roomInfoDic = new Dictionary<int, ServerInfo.RoomInfo>();
        Dictionary<long, ServerInfo.UserInfo> _connectUserInfo = new Dictionary<long, ServerInfo.UserInfo>();
        Dictionary<int, int[]> _iconIndexesDic = new Dictionary<int, int[]>();
        Dictionary<int, bool[]> _isClickableDic = new Dictionary<int, bool[]>();

        const int _cardCount = 24;
        int _currentRoomNumber = 1;

        Thread _tAccept;
        Thread _tFromClient;
        Thread _tToClient;
        Thread _tFromServer;
        Thread _tToServer;

        public UpgradeServer()
        {
            CreateServer();
        }

        bool Connect(string ipAddress, int port)
        {
            try
            {
                _dbServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _dbServer.Connect(ipAddress, port);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return false;
        }

        void CreateServer()
        {
            try
            {
                _waitServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _waitServer.Bind(new IPEndPoint(IPAddress.Any, _port));
                _waitServer.Listen(1);

                Console.WriteLine("서버가 만들어 졌습니다.");
                Console.WriteLine("DB 서버와 연결하려면 Connect를 입력하세요.");
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
                        _socketManager.AddSocket(new SocketClass(_waitServer.Accept()));

                        Console.WriteLine("Client Accept");
                    }

                    _socketManager.AddFromQueue(_fromClientQueue);

                    if (_dbServer != null && _dbServer.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buffer = new byte[1032];
                        int recvLen = _dbServer.Receive(buffer);
                        if (recvLen > 0)
                        {
                            DefinedStructure.PacketInfo pToClient = new DefinedStructure.PacketInfo();
                            pToClient = (DefinedStructure.PacketInfo)ConvertPacket.ByteArrayToStructure(buffer, pToClient.GetType(), recvLen);

                            PacketClass packet = new PacketClass(pToClient._id, pToClient._data, pToClient._totalSize);
                            _toServerQueue.Enqueue(packet);
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
                                #region 로그인 부분
                                case DefinedProtocol.eFromClient.OverlapCheck_ID:

                                    DefinedStructure.Packet_OverlapCheckID pOverlapCheck = new DefinedStructure.Packet_OverlapCheckID();
                                    pOverlapCheck = (DefinedStructure.Packet_OverlapCheckID)packet.Convert(pOverlapCheck.GetType());
                                    pOverlapCheck._index = packet._CastIdendifier;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.OverlapCheck_ID, pOverlapCheck));

                                    break;

                                case DefinedProtocol.eFromClient.JoinGame:

                                    DefinedStructure.Packet_JoinGame pJoinGame = new DefinedStructure.Packet_JoinGame();
                                    pJoinGame = (DefinedStructure.Packet_JoinGame)packet.Convert(pJoinGame.GetType());
                                    pJoinGame._index = packet._CastIdendifier;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.JoinGame, pJoinGame));

                                    break;

                                case DefinedProtocol.eFromClient.LogIn:

                                    DefinedStructure.Packet_LogIn pLogIn = new DefinedStructure.Packet_LogIn();
                                    pLogIn = (DefinedStructure.Packet_LogIn)packet.Convert(pLogIn.GetType());
                                    pLogIn._index = packet._CastIdendifier;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.LogIn, pLogIn));

                                    break;
                                #endregion

                                #region 유저 입장 부분
                                case DefinedProtocol.eFromClient.MyInfo:

                                    DefinedStructure.Packet_MyInfo pMyInfo = new DefinedStructure.Packet_MyInfo();
                                    pMyInfo = (DefinedStructure.Packet_MyInfo)packet.Convert(pMyInfo.GetType());

                                    _connectUserInfo[pMyInfo._UUID]._nickName = pMyInfo._name;
                                    _connectUserInfo[pMyInfo._UUID]._avatarIndex = pMyInfo._avatarIndex;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.EnrollUserInfo, pMyInfo));

                                    ShowRoomList(pMyInfo._UUID);

                                    logMessage = string.Format("{0} 유저가 입장\t\t{1}", pMyInfo._name, DateTime.Now);
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
                                    roomInfo._masterUUID = packet._UUID;
                                    roomInfo._currentMember = 1;
                                    roomInfo._slot = new long[8];
                                    roomInfo._AI = new List<CardBattleAI>();
                                    roomInfo._readyCount = 0;
                                    roomInfo._currentOrder = 0;
                                    roomInfo._score = new int[8];

                                    roomInfo._slot[0] = packet._UUID;

                                    logMessage = string.Format("{0} 유저가 {1}번 방을 만들었습니다.", packet._UUID, _currentRoomNumber);

                                    _roomInfoDic.Add(_currentRoomNumber, roomInfo);
                                    _selectedCardNumDic.Add(_currentRoomNumber, 0);
                                    _currentRoomNumber++;

                                    DefinedStructure.Packet_AfterCreateRoom pAfterCreateRoom;
                                    pAfterCreateRoom._roomNumber = roomInfo._roomNumber;

                                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.AfterCreateRoom, pAfterCreateRoom, packet._UUID));

                                    ShowRoomList(-999);

                                    break;

                                case DefinedProtocol.eFromClient.EnterRoom:

                                    DefinedStructure.Packet_EnterRoom pEnterRoom = new DefinedStructure.Packet_EnterRoom();
                                    pEnterRoom = (DefinedStructure.Packet_EnterRoom)packet.Convert(pEnterRoom.GetType());

                                    if (_roomInfoDic[pEnterRoom._roomNumber]._isLock)
                                    {
                                        if (_roomInfoDic[pEnterRoom._roomNumber]._pw.Equals(pEnterRoom._pw))
                                        {
                                            EnterRoom(pEnterRoom._roomNumber, packet._UUID);
                                            logMessage = string.Format("{0} 유저가 {1}번 방에 입장하였습니다.", packet._UUID, pEnterRoom._roomNumber);
                                        }
                                        else
                                        {
                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.FailEnterRoom, null, packet._UUID));
                                            logMessage = string.Format("{0} 유저가 {1}번 방 입장에 실패하였습니다.", packet._UUID, pEnterRoom._roomNumber);
                                        }
                                    }
                                    else
                                    {
                                        EnterRoom(pEnterRoom._roomNumber, packet._UUID);
                                        logMessage = string.Format("{0} 유저가 {1}번 방에 입장하였습니다.", packet._UUID, pEnterRoom._roomNumber);
                                    }

                                    ShowRoomList(-999);

                                    break;

                                case DefinedProtocol.eFromClient.ExitRoom:

                                    DefinedStructure.Packet_ExitRoom pExitRoom = new DefinedStructure.Packet_ExitRoom();
                                    pExitRoom = (DefinedStructure.Packet_ExitRoom)packet.Convert(pExitRoom.GetType());

                                    _roomInfoDic[pExitRoom._roomNumber]._slot[pExitRoom._slotIndex] = 0;
                                    _roomInfoDic[pExitRoom._roomNumber]._currentMember--;

                                    if (_roomInfoDic[pExitRoom._roomNumber]._currentMember <= _roomInfoDic[pExitRoom._roomNumber]._AI.Count)
                                    {
                                        _roomInfoDic.Remove(pExitRoom._roomNumber);
                                    }
                                    else
                                    {
                                        if (pExitRoom._UUID == _roomInfoDic[pExitRoom._roomNumber]._masterUUID)
                                        {
                                            for(int n = 0; n < _roomInfoDic[pExitRoom._roomNumber]._slot.Length; n++)
                                            {
                                                if(_roomInfoDic[pExitRoom._roomNumber]._slot[n] > 0)
                                                {
                                                    _roomInfoDic[pExitRoom._roomNumber]._masterUUID = _roomInfoDic[pExitRoom._roomNumber]._slot[n];
                                                    break;
                                                }
                                            }
                                            
                                            DefinedStructure.Packet_ShowMaster pShowMaster;
                                            pShowMaster._name = _connectUserInfo[_roomInfoDic[pExitRoom._roomNumber]._masterUUID]._nickName;

                                            SendBufferInRoom(DefinedProtocol.eToClient.ShowMaster, _roomInfoDic[pExitRoom._roomNumber]._slot, pShowMaster);
                                        }

                                        DefinedStructure.Packet_ShowExit pShowExit;
                                        pShowExit._name = _connectUserInfo[pExitRoom._UUID]._nickName;

                                        SendBufferInRoom(DefinedProtocol.eToClient.ShowExit, _roomInfoDic[pExitRoom._roomNumber]._slot, pShowExit);
                                    }

                                    //ShowRoomList(pExitRoom._UUID);
                                    ShowRoomList(-999);

                                    break;

                                case DefinedProtocol.eFromClient.Ready:

                                    DefinedStructure.Packet_Ready pReady = new DefinedStructure.Packet_Ready();
                                    pReady = (DefinedStructure.Packet_Ready)packet.Convert(pReady.GetType());

                                    _roomInfoDic[pReady._roomNumber]._readyCount++;

                                    DefinedStructure.Packet_ShowReady pShowReady;
                                    pShowReady._slotIndex = pReady._slotIndex;

                                    SendBufferInRoom(DefinedProtocol.eToClient.ShowReady, _roomInfoDic[pReady._roomNumber]._slot, pShowReady);

                                    if(_roomInfoDic[pReady._roomNumber]._readyCount == _roomInfoDic[pReady._roomNumber]._currentMember - 1)
                                    {
                                        _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.CanPlay, null, _roomInfoDic[pReady._roomNumber]._masterUUID));
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.GameStart:

                                    DefinedStructure.Packet_GameStart pGameStart = new DefinedStructure.Packet_GameStart();
                                    pGameStart = (DefinedStructure.Packet_GameStart)packet.Convert(pGameStart.GetType());

                                    _roomInfoDic[pGameStart._roomNumber]._timeStart = DateTime.Now;

                                    Console.WriteLine("{0}방에서 게임이 시작되었습니다.", pGameStart._roomNumber);

                                    _isClickableDic.Add(pGameStart._roomNumber, new bool[_cardCount]);

                                    while(_roomInfoDic[pGameStart._roomNumber]._slot[_roomInfoDic[pGameStart._roomNumber]._currentOrder] == 0)
                                    {
                                        _roomInfoDic[pGameStart._roomNumber]._currentOrder++;

                                        if (_roomInfoDic[pGameStart._roomNumber]._currentOrder >= _roomInfoDic[pGameStart._roomNumber]._slot.Length)
                                            _roomInfoDic[pGameStart._roomNumber]._currentOrder = 0;
                                    }

                                    DefinedStructure.Packet_NextTurn pNextTurn;
                                    pNextTurn._name = string.Empty;
                                    if (_roomInfoDic[pGameStart._roomNumber]._slot[_roomInfoDic[pGameStart._roomNumber]._currentOrder] > 0)
                                        pNextTurn._name = _connectUserInfo[_roomInfoDic[pGameStart._roomNumber]._slot[_roomInfoDic[pGameStart._roomNumber]._currentOrder]]._nickName;
                                    else if (_roomInfoDic[pGameStart._roomNumber]._slot[_roomInfoDic[pGameStart._roomNumber]._currentOrder] < 0)
                                        pNextTurn._name = "AI" + Math.Abs(_roomInfoDic[pGameStart._roomNumber]._slot[_roomInfoDic[pGameStart._roomNumber]._currentOrder]);

                                    Console.WriteLine("이번 턴은 {0} 유저입니다.", pNextTurn._name);

                                    SendBufferInRoom(DefinedProtocol.eToClient.NextTurn, _roomInfoDic[pGameStart._roomNumber]._slot, pNextTurn);

                                    MixCard(pGameStart._roomNumber);

                                    SendBufferInRoom(DefinedProtocol.eToClient.GameStart, _roomInfoDic[pGameStart._roomNumber]._slot, null);

                                    break;

                                case DefinedProtocol.eFromClient.AddAI:

                                    DefinedStructure.Packet_AddAI pAddAI = new DefinedStructure.Packet_AddAI();
                                    pAddAI = (DefinedStructure.Packet_AddAI)packet.Convert(pAddAI.GetType());

                                    _roomInfoDic[pAddAI._roomNumber]._currentMember++;
                                    _roomInfoDic[pAddAI._roomNumber]._readyCount++;
                                    _roomInfoDic[pAddAI._roomNumber]._AI.Add(new CardBattleAI(CardBattleAI.eAIDifficulty.Hard));
                                    _roomInfoDic[pAddAI._roomNumber]._slot[pAddAI._index] = -_roomInfoDic[pAddAI._roomNumber]._AI.Count;

                                    DefinedStructure.Packet_ShowAI pShowAI;
                                    pShowAI._slotIndex = pAddAI._index;
                                    pShowAI._aiName = "AI" + _roomInfoDic[pAddAI._roomNumber]._AI.Count.ToString();

                                    SendBufferInRoom(DefinedProtocol.eToClient.ShowAI, _roomInfoDic[pAddAI._roomNumber]._slot, pShowAI);

                                    Console.WriteLine("{0}가 추가되었습니다.", pShowAI._aiName);

                                    ShowRoomList(-999);

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

                                    Console.WriteLine(string.Format("{0} 유저가 {1}번 카드와 {2}번 카드를 선택 하였습니다.", packet._UUID, pChooseCard._cardIdx1, pChooseCard._cardIdx2));

                                    SendBufferInRoom(DefinedProtocol.eToClient.ChooseInfo, _roomInfoDic[pChooseCard._roomNumber]._slot, pChooseInfo);

                                    DefinedStructure.Packet_ChooseResult pChooseResult;
                                    pChooseResult._name = string.Empty;

                                    Console.WriteLine(_roomInfoDic[pChooseCard._roomNumber]._slot[pChooseCard._slotIndex]);

                                    if (_roomInfoDic[pChooseCard._roomNumber]._slot[pChooseCard._slotIndex] > 0)
                                        pChooseResult._name = _connectUserInfo[pChooseCard._UUID]._nickName;
                                    else if (_roomInfoDic[pChooseCard._roomNumber]._slot[pChooseCard._slotIndex] < 0)
                                        pChooseResult._name = "AI" + Math.Abs(_roomInfoDic[pChooseCard._roomNumber]._slot[pChooseCard._slotIndex]).ToString();

                                    pChooseResult._cardIdx1 = pChooseCard._cardIdx1;
                                    pChooseResult._cardIdx2 = pChooseCard._cardIdx2;

                                    if (_iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx1] == _iconIndexesDic[pChooseCard._roomNumber][pChooseCard._cardIdx2])
                                    {
                                        pChooseResult._isSuccess = 0;

                                        _roomInfoDic[pChooseCard._roomNumber]._score[pChooseCard._slotIndex]++;

                                        _selectedCardNumDic[pChooseCard._roomNumber]++;

                                        _isClickableDic[pChooseCard._roomNumber][pChooseCard._cardIdx1] = true;
                                        _isClickableDic[pChooseCard._roomNumber][pChooseCard._cardIdx2] = true;

                                        for (int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._AI.Count; n++)
                                            _roomInfoDic[pChooseCard._roomNumber]._AI[n].RemoveMemory(pChooseCard._cardIdx1, pChooseCard._cardIdx2);
                                    }
                                    else
                                    {
                                        pChooseResult._isSuccess = 1;
                                        
                                        for (int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._AI.Count; n++)
                                            _roomInfoDic[pChooseCard._roomNumber]._AI[n].SaveMemory(pChooseCard._cardIdx1, pChooseCard._cardIdx2);
                                    }

                                    SendBufferInRoom(DefinedProtocol.eToClient.ChooseResult, _roomInfoDic[pChooseCard._roomNumber]._slot, pChooseResult);

                                    if (_selectedCardNumDic[pChooseCard._roomNumber] >= _cardCount / 2)
                                    {
                                        _roomInfoDic[pChooseCard._roomNumber]._timeEnd = DateTime.Now;

                                        Console.WriteLine(string.Format("{0}번 방에서 게임이 끝났습니다.", pChooseCard._roomNumber));
                                        int highScore = int.MinValue;
                                        long winPlayerIndex = 0;

                                        for(int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._score.Length; n++)
                                        {
                                            if (highScore < _roomInfoDic[pChooseCard._roomNumber]._score[n])
                                            {
                                                highScore = _roomInfoDic[pChooseCard._roomNumber]._score[n];
                                                winPlayerIndex = _roomInfoDic[pChooseCard._roomNumber]._slot[n];
                                            }
                                        }   

                                        Console.WriteLine(string.Format("{0}번 방의 게임 승자는 {1}입니다.", pChooseCard._roomNumber, winPlayerIndex));

                                        DefinedStructure.Packet_GameResult pGameResult;
                                        pGameResult._name = string.Empty;
                                        if (winPlayerIndex > 0)
                                            pGameResult._name = _connectUserInfo[winPlayerIndex]._nickName;
                                        else if (winPlayerIndex < 0)
                                            pGameResult._name = "AI" + Math.Abs(winPlayerIndex).ToString();


                                        SendBufferInRoom(DefinedProtocol.eToClient.GameResult, _roomInfoDic[pChooseCard._roomNumber]._slot, pGameResult);

                                        for(int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._score.Length; n++)
                                            _roomInfoDic[pChooseCard._roomNumber]._score[n] = 0;

                                        _roomInfoDic[pChooseCard._roomNumber]._readyCount = _roomInfoDic[pChooseCard._roomNumber]._AI.Count;
                                        _isClickableDic.Remove(pChooseCard._roomNumber);

                                        TimeSpan timeDiff = _roomInfoDic[pChooseCard._roomNumber]._timeEnd - _roomInfoDic[pChooseCard._roomNumber]._timeStart;
                                        double diffSecond = timeDiff.TotalSeconds;

                                        for(int n = 0; n < _roomInfoDic[pChooseCard._roomNumber]._slot.Length; n++)
                                        {
                                            if(_roomInfoDic[pChooseCard._roomNumber]._slot[n] > 0)
                                            {
                                                DefinedStructure.Packet_SaveResult pSaveResult;
                                                pSaveResult._UUID = _roomInfoDic[pChooseCard._roomNumber]._slot[n];
                                                pSaveResult._clearTime = (int)diffSecond;
                                                pSaveResult._isWin = winPlayerIndex == _roomInfoDic[pChooseCard._roomNumber]._slot[n] ? 0 : 1;

                                                _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.SaveResult, pSaveResult));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        DefinedStructure.Packet_NextTurn pNextTurn2;
                                        pNextTurn2._name = string.Empty;

                                        if (++_roomInfoDic[pChooseCard._roomNumber]._currentOrder >= _roomInfoDic[pChooseCard._roomNumber]._slot.Length)
                                            _roomInfoDic[pChooseCard._roomNumber]._currentOrder = 0;

                                        while (_roomInfoDic[pChooseCard._roomNumber]._slot[_roomInfoDic[pChooseCard._roomNumber]._currentOrder] == 0)
                                        {
                                            _roomInfoDic[pChooseCard._roomNumber]._currentOrder++;

                                            if (_roomInfoDic[pChooseCard._roomNumber]._currentOrder >= _roomInfoDic[pChooseCard._roomNumber]._slot.Length)
                                                _roomInfoDic[pChooseCard._roomNumber]._currentOrder = 0;
                                        }

                                        if (_roomInfoDic[pChooseCard._roomNumber]._slot[_roomInfoDic[pChooseCard._roomNumber]._currentOrder] > 0)
                                        {
                                            pNextTurn2._name = _connectUserInfo[_roomInfoDic[pChooseCard._roomNumber]._slot[_roomInfoDic[pChooseCard._roomNumber]._currentOrder]]._nickName;
                                        }
                                        else if(_roomInfoDic[pChooseCard._roomNumber]._slot[_roomInfoDic[pChooseCard._roomNumber]._currentOrder] < 0)
                                        {
                                            pNextTurn2._name = "AI" + Math.Abs(_roomInfoDic[pChooseCard._roomNumber]._slot[_roomInfoDic[pChooseCard._roomNumber]._currentOrder]).ToString();
                                            
                                            TurnAI(pChooseCard._roomNumber, _roomInfoDic[pChooseCard._roomNumber]._AI[Math.Abs((int)(_roomInfoDic[pChooseCard._roomNumber]._slot[_roomInfoDic[pChooseCard._roomNumber]._currentOrder])) - 1], _roomInfoDic[pChooseCard._roomNumber]._currentOrder);
                                        }

                                        Console.WriteLine("이번 턴은 {0} 유저입니다.", pNextTurn2._name);

                                        SendBufferInRoom(DefinedProtocol.eToClient.NextTurn, _roomInfoDic[pChooseCard._roomNumber]._slot, pNextTurn2);
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
                        {
                            if(packet._CastIdendifier >= 0)
                            {
                                _socketManager.Send(packet._Data, packet._CastIdendifier);
                            }
                            else
                            {
                                _socketManager.Send(packet._Data, packet._UUID);
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
                while (true)
                {
                    if (_fromServerQueue.Count != 0)
                        _dbServer.Send(_fromServerQueue.Dequeue()._Data);

                    Thread.Sleep(10);
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
                        PacketClass tData = _toServerQueue.Dequeue();

                        switch ((DefinedProtocol.eToServer)tData._ProtocolID)
                        {
                            case DefinedProtocol.eToServer.OverlapCheckResult_ID:

                                DefinedStructure.Packet_OverlapCheckResultID pOverlapResult = new DefinedStructure.Packet_OverlapCheckResultID();
                                pOverlapResult = (DefinedStructure.Packet_OverlapCheckResultID)tData.Convert(pOverlapResult.GetType());

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.OverlapCheckResult_ID, pOverlapResult, pOverlapResult._index));

                                break;

                            case DefinedProtocol.eToServer.CompleteJoin:

                                DefinedStructure.Packet_CompleteJoin pCompleteJoin = new DefinedStructure.Packet_CompleteJoin();
                                pCompleteJoin = (DefinedStructure.Packet_CompleteJoin)tData.Convert(pCompleteJoin.GetType());

                                _socketManager.SocketComplete(pCompleteJoin._UUID, pCompleteJoin._index);

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.CompleteJoin, pCompleteJoin, pCompleteJoin._index));

                                break;

                            case DefinedProtocol.eToServer.LogInResult:

                                DefinedStructure.Packet_LogInResult pLogInResult = new DefinedStructure.Packet_LogInResult();
                                pLogInResult = (DefinedStructure.Packet_LogInResult)tData.Convert(pLogInResult.GetType());

                                ServerInfo.UserInfo userInfo = new ServerInfo.UserInfo();

                                if(pLogInResult._isSuccess == 0)
                                {
                                    userInfo._UUID = pLogInResult._UUID;
                                    if(pLogInResult._isFirst != 0)
                                    {
                                        userInfo._nickName = pLogInResult._name;
                                        userInfo._avatarIndex = pLogInResult._avatarIndex;

                                        _socketManager.SocketComplete(pLogInResult._UUID, pLogInResult._index);
                                    }

                                    _connectUserInfo.Add(pLogInResult._UUID, userInfo);
                                }

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.LogInResult, pLogInResult, pLogInResult._index));

                                if (pLogInResult._isSuccess == 0)
                                    if (pLogInResult._isFirst != 0)
                                        ShowRoomList(pLogInResult._UUID);

                                break;

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

        void ShowRoomList(long uuid)
        {
            if (_roomInfoDic.Count != 0)
            {
                foreach (int key in _roomInfoDic.Keys)
                {
                    DefinedStructure.Packet_ShowRoomInfo pShowRoomInfo;
                    pShowRoomInfo._roomNumber = _roomInfoDic[key]._roomNumber;
                    pShowRoomInfo._roomName = _roomInfoDic[key]._name;
                    pShowRoomInfo._isLock = _roomInfoDic[key]._isLock ? 0 : 1;
                    pShowRoomInfo._currentMemberNum = _roomInfoDic[key]._currentMember;

                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowRoomInfo, pShowRoomInfo, uuid));
                }
            }
        }

        void EnterRoom(int roomNum, long uuid)
        {
            for(int n = 0; n < _roomInfoDic[roomNum]._slot.Length; n++)
            {
                if (_roomInfoDic[roomNum]._slot[n] == 0)
                {
                    _roomInfoDic[roomNum]._slot[n] = uuid;
                    _roomInfoDic[roomNum]._currentMember++;

                    DefinedStructure.Packet_SuccessEnterRoom pSuccessEnter;
                    pSuccessEnter._slotIndex = n;
                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.SuccessEnterRoom, pSuccessEnter, uuid));

                    break;
                }   
            }

            for (int n = 0; n < _roomInfoDic[roomNum]._slot.Length; n++)
            {
                if(_roomInfoDic[roomNum]._slot[n] > 0)
                {
                    DefinedStructure.Packet_MyInfo pShowUser;
                    pShowUser._UUID = _roomInfoDic[roomNum]._slot[n];
                    pShowUser._name = _connectUserInfo[_roomInfoDic[roomNum]._slot[n]]._nickName;
                    pShowUser._avatarIndex = _connectUserInfo[_roomInfoDic[roomNum]._slot[n]]._avatarIndex;
                    pShowUser._slotIndex = n;

                    for (int m = 0; m < _roomInfoDic[roomNum]._slot.Length; m++)
                    {
                        if(_roomInfoDic[roomNum]._slot[m] > 0)
                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowUserInfo, pShowUser, _roomInfoDic[roomNum]._slot[m]));
                    }
                }
                else if(_roomInfoDic[roomNum]._slot[n] < 0)
                {
                    DefinedStructure.Packet_ShowAI pShowAI;
                    pShowAI._slotIndex = n;
                    pShowAI._aiName = "AI" + Math.Abs(_roomInfoDic[roomNum]._slot[n]).ToString();

                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowAI, pShowAI, uuid));
                }
            }

            DefinedStructure.Packet_ShowMaster pShowMaster;
            pShowMaster._name = _connectUserInfo[_roomInfoDic[roomNum]._masterUUID]._nickName;

            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowMaster, pShowMaster, uuid));
        }

        void SendBufferInRoom(DefinedProtocol.eToClient type, long[] userUUIDArr, object str)
        {
            for (int n = 0; n < userUUIDArr.Length; n++)
            {
                if(userUUIDArr[n] > 0)
                    _toClientQueue.Enqueue(_socketManager.AddToQueue(type, str, userUUIDArr[n]));
            }   
        }

        void TurnAI(int roomNum, CardBattleAI ai, int index)
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
                while (select[0] == select[1] || _isClickableDic[roomNum][select[1]]);
            }

            DefinedStructure.Packet_ChooseCard pChooseCard;
            pChooseCard._UUID = 0;
            pChooseCard._roomNumber = roomNum;
            pChooseCard._cardIdx1 = select[0];
            pChooseCard._cardIdx2 = select[1];
            pChooseCard._slotIndex = index;

            DefinedStructure.PacketInfo packetInfo;
            packetInfo._id = (int)DefinedProtocol.eFromClient.ChooseCard;
            packetInfo._data = ConvertPacket.StructureToByteArray(pChooseCard);
            packetInfo._totalSize = packetInfo._data.Length;

            PacketClass packet = new PacketClass(packetInfo._id, packetInfo._data, packetInfo._totalSize);
            _fromClientQueue.Enqueue(packet);
        }

        public void ConnectDB()
        {
            if(Connect(_dbIP, _dbPort))
            {
                _tFromServer = new Thread(new ThreadStart(FromServerQueue));
                _tToServer = new Thread(new ThreadStart(ToServerQueue));

                if (!_tFromServer.IsAlive)
                    _tFromServer.Start();

                if (!_tToServer.IsAlive)
                    _tToServer.Start();
            }
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
            _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.ExitServer, null));

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
