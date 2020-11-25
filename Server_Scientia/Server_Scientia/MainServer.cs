using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server_Scientia
{
    public enum eCardKind
    {
        Abnormal,
        Normal
    }

    public enum eCardField
    {
        Physics,
        Chemistry,
        Biology,
        Astronomy,

        max
    }

    class MainServer
    {
        const short _port = 80;
        Socket _waitServer;

        const string _dbIP = "127.0.0.1";
        const int _dbPort = 81;
        Socket _dbServer;

        int _roomNumber = 1;
        RoomSort _roomInfoSort = new RoomSort();

        SocketManagerClass _socketManager = new SocketManagerClass();

        Queue<PacketClass> _fromClientQueue = new Queue<PacketClass>();
        Queue<PacketClass> _toClientQueue = new Queue<PacketClass>();

        Queue<PacketClass> _fromServerQueue = new Queue<PacketClass>();
        Queue<PacketClass> _toServerQueue = new Queue<PacketClass>();

        Thread _tAccept;
        Thread _tFromClient;
        Thread _tToClient;
        Thread _tFromServer;
        Thread _tToServer;

        TableManager _tbMgr = new TableManager();

        int[] _startCardArr = new int[] { 2, 3, 5, 13, 16, 17, 25, 27, 29, 37, 39, 41 };

        public MainServer()
        {
            _tbMgr.LoadAll();
            CreateServer();
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

        public void ConnectDB()
        {
            if (Connect(_dbIP, _dbPort))
            {
                _tFromServer = new Thread(new ThreadStart(FromServerQueue));
                _tToServer = new Thread(new ThreadStart(ToServerQueue));

                if (!_tFromServer.IsAlive)
                    _tFromServer.Start();

                if (!_tToServer.IsAlive)
                    _tToServer.Start();
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
                            switch ((DefinedProtocol.eFromClient)packet._ProtocolID)
                            {
                                #region LogIn / CreateCharacter
                                case DefinedProtocol.eFromClient.LogInTry:

                                    DefinedStructure.P_Send_ID_Pw pLogInTry = new DefinedStructure.P_Send_ID_Pw();
                                    pLogInTry = (DefinedStructure.P_Send_ID_Pw)packet.Convert(pLogInTry.GetType());

                                    DefinedStructure.P_Check_ID_Pw pCheckLogIn;
                                    pCheckLogIn._id = pLogInTry._id;
                                    pCheckLogIn._pw = pLogInTry._pw;
                                    pCheckLogIn._index = packet._CastIdendifier;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.CheckLogIn, pCheckLogIn));

                                    Console.WriteLine("ID가 {0}인 유저가 접속을 시도 하고 있습니다.", pLogInTry._id);

                                    break;

                                case DefinedProtocol.eFromClient.OverlapCheck_ID:

                                    DefinedStructure.P_OverlapCheck pOverlapCheck_ID = new DefinedStructure.P_OverlapCheck();
                                    pOverlapCheck_ID = (DefinedStructure.P_OverlapCheck)packet.Convert(pOverlapCheck_ID.GetType());

                                    DefinedStructure.P_CheckOverlap pCheckOverlap_ID;
                                    pCheckOverlap_ID._target = pOverlapCheck_ID._target;
                                    pCheckOverlap_ID._index = packet._CastIdendifier;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.OverlapCheck_ID, pCheckOverlap_ID));

                                    break;

                                case DefinedProtocol.eFromClient.OverlapCheck_NickName:

                                    DefinedStructure.P_OverlapCheck pOverlapCheck_NickName = new DefinedStructure.P_OverlapCheck();
                                    pOverlapCheck_NickName = (DefinedStructure.P_OverlapCheck)packet.Convert(pOverlapCheck_NickName.GetType());

                                    DefinedStructure.P_CheckOverlap pCheckOverlap_NickName;
                                    pCheckOverlap_NickName._target = pOverlapCheck_NickName._target;
                                    pCheckOverlap_NickName._index = packet._CastIdendifier;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.OverlapCheck_NickName, pCheckOverlap_NickName));

                                    break;

                                case DefinedProtocol.eFromClient.EnrollTry:

                                    DefinedStructure.P_Send_ID_Pw pEnrollTry = new DefinedStructure.P_Send_ID_Pw();
                                    pEnrollTry = (DefinedStructure.P_Send_ID_Pw)packet.Convert(pEnrollTry.GetType());

                                    DefinedStructure.P_Check_ID_Pw pCheckEnroll;
                                    pCheckEnroll._id = pEnrollTry._id;
                                    pCheckEnroll._pw = pEnrollTry._pw;
                                    pCheckEnroll._index = packet._CastIdendifier;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.CheckEnroll, pCheckEnroll));

                                    break;

                                case DefinedProtocol.eFromClient.GetMyCharacterInfo:

                                    DefinedStructure.P_CheckRequest pGetMyCharacInfo;
                                    pGetMyCharacInfo._UUID = packet._UUID;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.CheckCharacterInfo, pGetMyCharacInfo));

                                    break;

                                case DefinedProtocol.eFromClient.CreateCharacter:

                                    DefinedStructure.P_CreateCharacter pCreateCharacter = new DefinedStructure.P_CreateCharacter();
                                    pCreateCharacter = (DefinedStructure.P_CreateCharacter)packet.Convert(pCreateCharacter.GetType());

                                    Console.WriteLine(packet._UUID);

                                    DefinedStructure.P_CreateCharacterInfo pCreateCharacterInfo;
                                    pCreateCharacterInfo._UUID = packet._UUID;
                                    pCreateCharacterInfo._nickName = pCreateCharacter._nickName;
                                    pCreateCharacterInfo._characterIndex = pCreateCharacter._characterIndex;
                                    pCreateCharacterInfo._slot = pCreateCharacter._slot;
                                    pCreateCharacterInfo._startCardList = new int[48];
                                    for (int n = 0; n < _startCardArr.Length; n++)
                                        pCreateCharacterInfo._startCardList[n] = _startCardArr[n];

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.CreateCharacter, pCreateCharacterInfo));

                                    break;
                                #endregion

                                #region Card
                                case DefinedProtocol.eFromClient.GetMyInfoData:

                                    DefinedStructure.P_GetMyInfoData pGetMyInfoData = new DefinedStructure.P_GetMyInfoData();
                                    pGetMyInfoData = (DefinedStructure.P_GetMyInfoData)packet.Convert(pGetMyInfoData.GetType());

                                    DefinedStructure.P_UserMyInfoData pUserInfoData;
                                    pUserInfoData._UUID = packet._UUID;
                                    pUserInfoData._nickName = pGetMyInfoData._nickName;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.UserMyInfoData, pUserInfoData));

                                    break;

                                case DefinedProtocol.eFromClient.AddReleaseCard:

                                    DefinedStructure.P_ReleaseCard pReleaseCard = new DefinedStructure.P_ReleaseCard();
                                    pReleaseCard = (DefinedStructure.P_ReleaseCard)packet.Convert(pReleaseCard.GetType());

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.AddReleaseCard, pReleaseCard));

                                    break;
                                #endregion

                                #region Room
                                case DefinedProtocol.eFromClient.CreateRoom:

                                    DefinedStructure.P_CreateRoom pCreateRoom = new DefinedStructure.P_CreateRoom();
                                    pCreateRoom = (DefinedStructure.P_CreateRoom)packet.Convert(pCreateRoom.GetType());

                                    RoomSort.RoomInfo roomInfo = new RoomSort.RoomInfo();
                                    roomInfo._roomNumber = _roomNumber++;
                                    roomInfo._name = pCreateRoom._name;
                                    roomInfo._isLock = pCreateRoom._isLock == 0;
                                    roomInfo._pw = pCreateRoom._pw;
                                    roomInfo._mode = pCreateRoom._mode;
                                    roomInfo._rule = pCreateRoom._rule;
                                    roomInfo._master = 0;
                                    roomInfo._userList = new List<RoomSort.UserInfo>();
                                    roomInfo._currentMemberCnt = 0;

                                    for (int n = 0; n < 4; n++)
                                    {
                                        RoomSort.UserInfo userInfo = new RoomSort.UserInfo();
                                        userInfo._isEmpty = true;
                                        roomInfo._userList.Add(userInfo);
                                    }

                                    _roomInfoSort.CreateRoom(pCreateRoom._mode, roomInfo);

                                    GetBattleInfo(roomInfo._roomNumber, packet._UUID, pCreateRoom._nickName);

                                    break;

                                case DefinedProtocol.eFromClient.GetRoomList:

                                    foreach(string key in _roomInfoSort._RoomList.Keys)
                                    {
                                        for(int n = 0; n < _roomInfoSort._RoomList[key].Count; n++)
                                        {
                                            DefinedStructure.P_RoomInfo pRoomInfo;
                                            pRoomInfo._roomNumber = _roomInfoSort._RoomList[key][n]._roomNumber;
                                            pRoomInfo._name = _roomInfoSort._RoomList[key][n]._name;
                                            pRoomInfo._isLock = _roomInfoSort._RoomList[key][n]._isLock ? 0 : 1;
                                            pRoomInfo._currentMemberCnt = _roomInfoSort._RoomList[key][n]._currentMemberCnt;
                                            pRoomInfo._maxMemberCnt = 4;
                                            pRoomInfo._mode = _roomInfoSort._RoomList[key][n]._mode;
                                            pRoomInfo._rule = _roomInfoSort._RoomList[key][n]._rule;
                                            pRoomInfo._isPlay = 0;

                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowRoomList, pRoomInfo, packet._UUID));
                                        }
                                    }

                                    DefinedStructure.P_Request pFinishShowRoom;
                                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.FinishShowRoom, pFinishShowRoom, packet._UUID));

                                    break;

                                case DefinedProtocol.eFromClient.TryEnterRoom:

                                    DefinedStructure.P_TryEnterRoom pTryEnterRoom = new DefinedStructure.P_TryEnterRoom();
                                    pTryEnterRoom = (DefinedStructure.P_TryEnterRoom)packet.Convert(pTryEnterRoom.GetType());

                                    GetBattleInfo(pTryEnterRoom._roomNumber, packet._UUID, pTryEnterRoom._nickName);

                                    break;
                                #endregion

                                case DefinedProtocol.eFromClient.InformReady:

                                    DefinedStructure.P_InformRoomInfo pInformReady = new DefinedStructure.P_InformRoomInfo();
                                    pInformReady = (DefinedStructure.P_InformRoomInfo)packet.Convert(pInformReady.GetType());

                                    RoomSort.RoomInfo roomReady = _roomInfoSort.GetRoom(pInformReady._roomNumber);
                                    RoomSort.UserInfo user = new RoomSort.UserInfo();
                                    for (int n = 0; n < roomReady._userList.Count; n++)
                                    {
                                        if(roomReady._userList[n]._UUID == packet._UUID)
                                        {
                                            user = roomReady._userList[n];
                                            break;
                                        }
                                    }

                                    user._isReady = !user._isReady;

                                    DefinedStructure.P_ShowReady pShowReady;
                                    pShowReady._index = user._index;
                                    pShowReady._isReady = user._isReady ? 0 : 1;

                                    for(int n = 0; n < roomReady._userList.Count; n++)
                                    {
                                        if(!roomReady._userList[n]._isEmpty)
                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowReady, pShowReady, roomReady._userList[n]._UUID));
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.InformGameStart:

                                    DefinedStructure.P_InformRoomInfo pInformGameStart = new DefinedStructure.P_InformRoomInfo();
                                    pInformGameStart = (DefinedStructure.P_InformRoomInfo)packet.Convert(pInformGameStart.GetType());

                                    RoomSort.RoomInfo roomStart = _roomInfoSort.GetRoom(pInformGameStart._roomNumber);

                                    if(CheckAllReady(roomStart))
                                    {
                                        //TODO Game Start Change Bool Value to Playing true

                                        DefinedStructure.P_Request pGameStart;
                                        for(int n = 0; n < roomStart._userList.Count; n++)
                                        {
                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.GameStart, pGameStart, roomStart._userList[n]._UUID));
                                        }

                                        RoomSort.CardInfo cardInfo = new RoomSort.CardInfo();
                                        for(int n = 0; n < (int)eCardField.max; n++)
                                            cardInfo._cardGroup.Add((eCardField)n, new List<int>());

                                        roomStart._cardInfo = cardInfo;
                                        switch (roomStart._mode)
                                        {
                                            case "MyDeck":

                                                break;

                                            case "RandomDeck":
                                            case "AllDeck":
                                                
                                                DefinedStructure.P_GetAllCard pGetAllCard;
                                                pGetAllCard._roomNumber = roomStart._roomNumber;
                                                pGetAllCard._nickNameArr = string.Empty;

                                                for (int n = 0; n < roomStart._userList.Count; n++)
                                                    pGetAllCard._nickNameArr += roomStart._userList[n]._nickName + ",";

                                                _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.GetAllCard, pGetAllCard));

                                                break;

                                            case "AI":

                                                break;
                                        }
                                    }
                                    else
                                    {
                                        DefinedStructure.P_Request pCannotPlay;
                                        _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.CannotPlay, pCannotPlay, packet._UUID));
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.FinishReadCard:

                                    DefinedStructure.P_InformRoomInfo pFinishReadCard = new DefinedStructure.P_InformRoomInfo();
                                    pFinishReadCard = (DefinedStructure.P_InformRoomInfo)packet.Convert(pFinishReadCard.GetType());

                                    RoomSort.RoomInfo roomFinishReadCard = _roomInfoSort.GetRoom(pFinishReadCard._roomNumber);

                                    for (int n = 0; n < roomFinishReadCard._userList.Count; n++)
                                    {
                                        if (roomFinishReadCard._userList[n]._UUID == packet._UUID)
                                        {
                                            roomFinishReadCard._userList[n]._isFinishReadCard = true;
                                            break;
                                        }
                                    }

                                    if(CheckAllReadCard(roomFinishReadCard))
                                    {
                                        roomFinishReadCard._thisTurn = roomFinishReadCard._master - 1;
                                        if (roomFinishReadCard._thisTurn < 0)
                                            roomFinishReadCard._thisTurn = roomFinishReadCard._userList.Count - 1;

                                        while(roomFinishReadCard._userList[roomFinishReadCard._thisTurn]._isEmpty)
                                        {
                                            roomFinishReadCard._thisTurn--;

                                            if (roomFinishReadCard._thisTurn < 0)
                                                roomFinishReadCard._thisTurn = roomFinishReadCard._userList.Count - 1;
                                        }

                                        DefinedStructure.P_ThisTurn pPickCardState;
                                        pPickCardState._index = roomFinishReadCard._thisTurn;

                                        for (int m = 0; m < roomFinishReadCard._userList.Count; m++)
                                        {
                                            if (!roomFinishReadCard._userList[m]._isEmpty)
                                            {
                                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.PickCard, pPickCardState, roomFinishReadCard._userList[m]._UUID));
                                            }
                                        }
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.PickCard:

                                    DefinedStructure.P_PickCard pPickCard = new DefinedStructure.P_PickCard();
                                    pPickCard = (DefinedStructure.P_PickCard)packet.Convert(pPickCard.GetType());

                                    RoomSort.RoomInfo roomPickCard = _roomInfoSort.GetRoom(pPickCard._roomNumber);

                                    for(int n = 0; n < roomPickCard._userList.Count; n++)
                                    {
                                        if(roomPickCard._userList[n]._UUID == packet._UUID)
                                        {
                                            if (roomPickCard._userList[n].IsEmptyCardSlot())
                                            {
                                                DefinedStructure.P_ShowPickCard pShowPickCard;
                                                pShowPickCard._index = roomPickCard._userList[n]._index;
                                                pShowPickCard._cardIndex = pPickCard._cardIndex;
                                                pShowPickCard._slotIndex = roomPickCard._userList[n].AddCard(pPickCard._cardIndex);

                                                for (int m = 0; m < roomPickCard._userList.Count; m++)
                                                {
                                                    if (!roomPickCard._userList[m]._isEmpty)
                                                    {
                                                        _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowPickCard, pShowPickCard, roomPickCard._userList[m]._UUID));
                                                    }
                                                }

                                                if(roomPickCard._userList[roomPickCard._master]._UUID == packet._UUID)
                                                {
                                                    DefinedStructure.P_ThisTurn pGameTurn;
                                                    pGameTurn._index = roomPickCard._master;

                                                    for (int m = 0; m < roomPickCard._userList.Count; m++)
                                                    {
                                                        if (!roomPickCard._userList[m]._isEmpty)
                                                        {
                                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ChooseAction, pGameTurn, roomPickCard._userList[m]._UUID));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (--roomPickCard._thisTurn < 0)
                                                        roomPickCard._thisTurn = roomPickCard._userList.Count - 1;

                                                    while (roomPickCard._userList[roomPickCard._thisTurn]._isEmpty)
                                                    {
                                                        roomPickCard._thisTurn--;

                                                        if (roomPickCard._thisTurn < 0)
                                                            roomPickCard._thisTurn = roomPickCard._userList.Count - 1;
                                                    }

                                                    DefinedStructure.P_ThisTurn pPickCardState;
                                                    pPickCardState._index = roomPickCard._thisTurn;

                                                    for (int m = 0; m < roomPickCard._userList.Count; m++)
                                                    {
                                                        if (!roomPickCard._userList[m]._isEmpty)
                                                        {
                                                            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.PickCard, pPickCardState, roomPickCard._userList[m]._UUID));
                                                        }
                                                    }
                                                }
                                            }   

                                            break;
                                        }
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.ConnectionTerminate:

                                    _socketManager.CloseSocket(packet._CastIdendifier);

                                    break;
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

                        if (packet._CastIdendifier >= 0)
                            _socketManager.Send(packet._Data, packet._CastIdendifier);
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
                            #region LogIn / Character
                            case DefinedProtocol.eToServer.LogInResult:

                                DefinedStructure.P_LogInResult pLogInResult = new DefinedStructure.P_LogInResult();
                                pLogInResult = (DefinedStructure.P_LogInResult)tData.Convert(pLogInResult.GetType());

                                DefinedStructure.P_ResultLogIn pResultLogIn;
                                pResultLogIn._isSuccess = pLogInResult._isSuccess;
                                pResultLogIn._UUID = pLogInResult._UUID;

                                _socketManager.ConnectSocket(pLogInResult._index, pLogInResult._UUID);
                                
                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.LogInResult, pResultLogIn, pLogInResult._index));

                                break;

                            case DefinedProtocol.eToServer.OverlapResult_ID:

                                DefinedStructure.P_CheckResult pOverlapResult_ID = new DefinedStructure.P_CheckResult();
                                pOverlapResult_ID = (DefinedStructure.P_CheckResult)tData.Convert(pOverlapResult_ID.GetType());

                                DefinedStructure.P_ResultCheck pResultOverlap_ID;
                                pResultOverlap_ID._result = pOverlapResult_ID._result;

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ResultOverlap_ID, pResultOverlap_ID, pOverlapResult_ID._index));

                                break;

                            case DefinedProtocol.eToServer.OverlapResult_NickName:

                                DefinedStructure.P_CheckResult pOverlapResult_NickName = new DefinedStructure.P_CheckResult();
                                pOverlapResult_NickName = (DefinedStructure.P_CheckResult)tData.Convert(pOverlapResult_NickName.GetType());

                                DefinedStructure.P_ResultCheck pResultOverlap_NickName;
                                pResultOverlap_NickName._result = pOverlapResult_NickName._result;

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ResultOverlap_NickName, pResultOverlap_NickName, pOverlapResult_NickName._index));

                                break;

                            case DefinedProtocol.eToServer.EnrollResult:

                                DefinedStructure.P_CheckResult pEnrollResult = new DefinedStructure.P_CheckResult();
                                pEnrollResult = (DefinedStructure.P_CheckResult)tData.Convert(pEnrollResult.GetType());

                                DefinedStructure.P_ResultCheck pResultEnroll;
                                pResultEnroll._result = pEnrollResult._result;

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.EnrollResult, pResultEnroll, pEnrollResult._index));

                                break;

                            case DefinedProtocol.eToServer.ShowCharacterInfo:

                                DefinedStructure.P_ShowCharacterInfo pShowCharacInfo = new DefinedStructure.P_ShowCharacterInfo();
                                pShowCharacInfo = (DefinedStructure.P_ShowCharacterInfo)tData.Convert(pShowCharacInfo.GetType());

                                DefinedStructure.P_CharacterInfo pCharacInfo;
                                pCharacInfo._nickName = pShowCharacInfo._nickName;
                                pCharacInfo._chracIndex = pShowCharacInfo._chracIndex;
                                pCharacInfo._accountLevel = pShowCharacInfo._accountLevel;
                                pCharacInfo._slotIndex = pShowCharacInfo._slotIndex;

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.CharacterInfo, pCharacInfo, pShowCharacInfo._UUID));

                                break;

                            case DefinedProtocol.eToServer.CompleteCharacterInfo:

                                DefinedStructure.P_CheckRequest pCompleteCharacterInfo = new DefinedStructure.P_CheckRequest();
                                pCompleteCharacterInfo = (DefinedStructure.P_CheckRequest)tData.Convert(pCompleteCharacterInfo.GetType());

                                DefinedStructure.P_Request pEndCharacterInfo;

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.EndCharacterInfo, pEndCharacterInfo, pCompleteCharacterInfo._UUID));

                                break;

                            case DefinedProtocol.eToServer.CreateCharacterResult:

                                DefinedStructure.P_Result pResult = new DefinedStructure.P_Result();
                                pResult = (DefinedStructure.P_Result)tData.Convert(pResult.GetType());

                                DefinedStructure.P_ResultCheck pResultCheck;
                                pResultCheck._result = pResult._result;

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.EndCreateCharacter, pResultCheck, pResult._UUID));

                                break;
                            #endregion

                            #region Card
                            case DefinedProtocol.eToServer.ShowMyInfoData:

                                DefinedStructure.P_CheckMyInfoData pCheckMyInfoData = new DefinedStructure.P_CheckMyInfoData();
                                pCheckMyInfoData = (DefinedStructure.P_CheckMyInfoData)tData.Convert(pCheckMyInfoData.GetType());

                                DefinedStructure.P_MyInfoData pMyInfoData;
                                pMyInfoData._characIndex = pCheckMyInfoData._characIndex;
                                pMyInfoData._levelArr = new int[5];
                                Array.Copy(pCheckMyInfoData._levelArr, pMyInfoData._levelArr, pMyInfoData._levelArr.Length);
                                pMyInfoData._expArr = new int[5];
                                Array.Copy(pCheckMyInfoData._expArr, pMyInfoData._expArr, pMyInfoData._expArr.Length);
                                pMyInfoData._cardReleaseArr = new int[48];
                                Array.Copy(pCheckMyInfoData._cardReleaseArr, pMyInfoData._cardReleaseArr, pMyInfoData._cardReleaseArr.Length);
                                pMyInfoData._cardRentalArr = new int[48];
                                Array.Copy(pCheckMyInfoData._cardRentalArr, pMyInfoData._cardRentalArr, pMyInfoData._cardRentalArr.Length);
                                pMyInfoData._rentalTimeArr = new float[48];
                                Array.Copy(pCheckMyInfoData._rentalTimeArr, pMyInfoData._rentalTimeArr, pMyInfoData._rentalTimeArr.Length);
                                pMyInfoData._myDeckArr = new int[12];
                                Array.Copy(pCheckMyInfoData._myDeckArr, pMyInfoData._myDeckArr, pMyInfoData._myDeckArr.Length);

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowMyInfo, pMyInfoData, pCheckMyInfoData._UUID));

                                break;

                            case DefinedProtocol.eToServer.CompleteAddReleaseCard:

                                DefinedStructure.P_CheckRequest pCompleteAddReleaseCard = new DefinedStructure.P_CheckRequest();
                                pCompleteAddReleaseCard = (DefinedStructure.P_CheckRequest)tData.Convert(pCompleteAddReleaseCard.GetType());

                                DefinedStructure.P_Request pShowCompleteAddReleaseCard;

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.CompleteAddReleaseCard, pShowCompleteAddReleaseCard, pCompleteAddReleaseCard._UUID));

                                break;
                            #endregion

                            case DefinedProtocol.eToServer.ShowBattleInfo:

                                DefinedStructure.P_ShowBattleInfo pShowBattleInfo = new DefinedStructure.P_ShowBattleInfo();
                                pShowBattleInfo = (DefinedStructure.P_ShowBattleInfo)tData.Convert(pShowBattleInfo.GetType());

                                RoomSort.RoomInfo room = _roomInfoSort.GetRoom(pShowBattleInfo._roomNumber);

                                RoomSort.UserInfo userInfo = new RoomSort.UserInfo();
                                userInfo._index = room.EmptyIndex();
                                userInfo._UUID = pShowBattleInfo._UUID;
                                userInfo._nickName = pShowBattleInfo._nickName;
                                userInfo._level = pShowBattleInfo._accountlevel;
                                userInfo._isEmpty = false;
                                userInfo._isReady = room._master == userInfo._index;
                                userInfo._pickedCardArr = new int[4];

                                for (int n = 0; n < room._userList.Count; n++)
                                {
                                    if(room._userList[n]._isEmpty)
                                    {
                                        room._userList[n] = userInfo;
                                        room._currentMemberCnt++;
                                        break;
                                    }
                                }
                                
                                for(int n = 0; n < room._userList.Count; n++)
                                {
                                    if(!room._userList[n]._isEmpty)
                                    {
                                        DefinedStructure.P_UserInfo pUserInfo;
                                        pUserInfo._roomNumber = pShowBattleInfo._roomNumber;
                                        pUserInfo._index = room._userList[n]._index;
                                        pUserInfo._nickName = room._userList[n]._nickName;
                                        pUserInfo._accountLevel = room._userList[n]._level;
                                        pUserInfo._isReady = room._userList[n]._isReady ? 0 : 1;

                                        for (int m = 0; m < room._userList.Count; m++)
                                        {
                                            if (!room._userList[m]._isEmpty)
                                            {
                                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.EnterRoom, pUserInfo, room._userList[m]._UUID));
                                            }
                                        }
                                    }
                                }

                                DefinedStructure.P_MasterInfo pMasterInfo;
                                pMasterInfo._masterIndex = room._master;

                                for (int n = 0; n < room._userList.Count; n++)
                                {
                                    if (!room._userList[n]._isEmpty)
                                    {
                                        _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowMaster, pMasterInfo, room._userList[n]._UUID));
                                    }
                                }

                                break;

                            case DefinedProtocol.eToServer.ShowAllCard:

                                DefinedStructure.P_ShowAllCard pShowAllCard = new DefinedStructure.P_ShowAllCard();
                                pShowAllCard = (DefinedStructure.P_ShowAllCard)tData.Convert(pShowAllCard.GetType());

                                RoomSort.RoomInfo roomToBattle = _roomInfoSort.GetRoom(pShowAllCard._roomNum);

                                switch(roomToBattle._mode)
                                {
                                    case "RandomDeck":

                                        PickCardRandomly(roomToBattle, pShowAllCard._cardArr, pShowAllCard._cardCount);

                                        break;

                                    case "AllDeck":
                                        break;
                                }

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

        void GetBattleInfo(int roomNum, long uuid, string nickName)
        {
            DefinedStructure.P_GetBattleInfo pGetBattleInfo;
            pGetBattleInfo._roomNumber = roomNum;
            pGetBattleInfo._UUID = uuid;
            pGetBattleInfo._nickName = nickName;

            _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.GetBattleInfo, pGetBattleInfo));
        }

        bool CheckAllReady(RoomSort.RoomInfo room)
        {
            int cnt = 0;
            for (int n = 0; n < room._userList.Count; n++)
            {
                if (room._userList[n]._isReady)
                {
                    cnt++;
                }
            }

            if (cnt == 1)
                return false;

            if (room._currentMemberCnt == cnt)
                return true;
            else
                return false;
        }

        bool CheckAllReadCard(RoomSort.RoomInfo room)
        {
            int cnt = 0;
            for (int n = 0; n < room._userList.Count; n++)
            {
                if (room._userList[n]._isFinishReadCard)
                    cnt++;
            }

            if (room._currentMemberCnt == cnt)
                return true;
            else
                return false;
        }

        void PickCardRandomly(RoomSort.RoomInfo room, int[] cardArr, int cardCnt)
        {
            Random rd = new Random();

            List<int> normalCard = new List<int>();
            List<int> nonNormalCard = new List<int>();

            for(int n = 0; n < cardCnt; n++)
            {
                if (_tbMgr.Get(eTableType.CardData).ToI(cardArr[n], CardData.Index.Kind.ToString()) == (int)eCardKind.Normal)
                    normalCard.Add(cardArr[n]);
                else
                    nonNormalCard.Add(cardArr[n]);
            }

            int selectIndex = 0;

            for(int n = 0; n < 4; n++)
            {
                do
                {
                    selectIndex = rd.Next(0, normalCard.Count);
                }
                while (room._cardInfo._cardGroup[(eCardField)_tbMgr.Get(eTableType.CardData).ToI(normalCard[selectIndex], CardData.Index.Field.ToString())].Contains(normalCard[selectIndex]) 
                        || cardArr[selectIndex] == 0);

                room._cardInfo._cardGroup[(eCardField)_tbMgr.Get(eTableType.CardData).ToI(normalCard[selectIndex], CardData.Index.Field.ToString())].Add(normalCard[selectIndex]);

                RemoveLinkCard(normalCard[selectIndex], normalCard);

                if (normalCard.Contains(normalCard[selectIndex]))
                    normalCard.Remove(normalCard[selectIndex]);
            }

            while(!room._cardInfo.IsOver())
            {
                do
                {
                    selectIndex = rd.Next(0, nonNormalCard.Count);
                }
                while (room._cardInfo._cardGroup[(eCardField)_tbMgr.Get(eTableType.CardData).ToI(nonNormalCard[selectIndex], CardData.Index.Field.ToString())].Contains(nonNormalCard[selectIndex]) 
                        || room._cardInfo._cardGroup[(eCardField)_tbMgr.Get(eTableType.CardData).ToI(nonNormalCard[selectIndex], CardData.Index.Field.ToString())].Count >= 3 
                        || nonNormalCard[selectIndex] == 0);

                room._cardInfo._cardGroup[(eCardField)_tbMgr.Get(eTableType.CardData).ToI(nonNormalCard[selectIndex], CardData.Index.Field.ToString())].Add(nonNormalCard[selectIndex]);

                RemoveLinkCard(nonNormalCard[selectIndex], nonNormalCard);

                if (nonNormalCard.Contains(nonNormalCard[selectIndex]))
                    nonNormalCard.Remove(nonNormalCard[selectIndex]);
            }

            DefinedStructure.P_PickedCard pPickedCard;
            pPickedCard._pickedCardArr = new int[12];
            int packIdx = 0;
            foreach(eCardField key in room._cardInfo._cardGroup.Keys)
            {
                for(int n = 0; n < room._cardInfo._cardGroup[key].Count; n++)
                {
                    pPickedCard._pickedCardArr[packIdx++] = room._cardInfo._cardGroup[key][n];
                }
            }

            for (int n = 0; n < room._userList.Count; n++)
            {
                if (!room._userList[n]._isEmpty)
                {
                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowPickedCard, pPickedCard, room._userList[n]._UUID));
                }
            }
        }

        void RemoveLinkCard(int selectedIndex, List<int> card)
        {
            int temp = selectedIndex % 2;

            if(temp == 0)
            {
                if (card.Contains(selectedIndex - 1))
                    card.Remove(selectedIndex - 1);
            }
            else
            {
                if(card.Contains(selectedIndex + 1))
                    card.Remove(selectedIndex + 1);
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
