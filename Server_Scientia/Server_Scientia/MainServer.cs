using System;
using System.Collections.Generic;
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

    public enum eCoinType
    {
        MasterCoin,
        AccountCoin,
        PhysicsCoin,
        ChemistryCoin,
        BiologyCoin,
        AstronomyCoin
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

        public void Test()
        {
            //RoomInfo room = new RoomInfo();
            //UserInfo user = new UserInfo();
            //user.InitUserInfo(1, "Test", 1, room);
            //user.InitSkillCube();
            //user.MoveSkillCube(eCardField.Physics);

            //int[] userskill = user.SkillCubePos(eCardField.Physics);
            
            //Console.WriteLine("스킬 큐브의 위치 : {0} // {1} // {2} // {3} // {4}", userskill[0], userskill[1], userskill[2], userskill[3], userskill[4]);
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
                            RoomInfo roomInfo;
                            UserInfo userInfo;

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

                                    roomInfo = new RoomInfo();
                                    roomInfo.InitRoomInfo(_roomNumber++, pCreateRoom._name, pCreateRoom._isLock == 0, pCreateRoom._pw, pCreateRoom._mode, pCreateRoom._rule);
                                    _roomInfoSort.CreateRoom(pCreateRoom._mode, roomInfo);

                                    GetBattleInfo(roomInfo._RoomNumber, packet._UUID, pCreateRoom._nickName);

                                    Console.WriteLine("{0} 유저가 {1}번 방을 만들었습니다.", packet._UUID, roomInfo._RoomNumber);

                                    break;

                                case DefinedProtocol.eFromClient.GetRoomList:

                                    foreach(string key in _roomInfoSort._RoomList.Keys)
                                    {
                                        for(int n = 0; n < _roomInfoSort._RoomList[key].Count; n++)
                                        {
                                            DefinedStructure.P_RoomInfo pRoomInfo;
                                            pRoomInfo._roomNumber = _roomInfoSort._RoomList[key][n]._RoomNumber;
                                            pRoomInfo._name = _roomInfoSort._RoomList[key][n]._Name;
                                            pRoomInfo._isLock = _roomInfoSort._RoomList[key][n]._IsLock ? 0 : 1;
                                            pRoomInfo._currentMemberCnt = _roomInfoSort._RoomList[key][n]._NowMemeberCnt;
                                            pRoomInfo._maxMemberCnt = 4;
                                            pRoomInfo._mode = _roomInfoSort._RoomList[key][n]._Mode;
                                            pRoomInfo._rule = _roomInfoSort._RoomList[key][n]._Rule;
                                            pRoomInfo._isPlay = _roomInfoSort._RoomList[key][n]._IsPlay ? 0 : 1;

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

                                case DefinedProtocol.eFromClient.QuickEnter:

                                    DefinedStructure.P_QuickEnter pQuickEnter = new DefinedStructure.P_QuickEnter();
                                    pQuickEnter = (DefinedStructure.P_QuickEnter)packet.Convert(pQuickEnter.GetType());

                                    RoomInfo roomQuickEnter = _roomInfoSort.QuickEnter(pQuickEnter._quickMode);
                                    if(roomQuickEnter != null)
                                        GetBattleInfo(roomQuickEnter._RoomNumber, packet._UUID, pQuickEnter._nickName);
                                    else
                                        ShowSystemMessage(packet._UUID, "No_Room");

                                    break;
                                #endregion

                                #region Game Ready
                                case DefinedProtocol.eFromClient.InformReady:

                                    DefinedStructure.P_InformRoomInfo pInformReady = new DefinedStructure.P_InformRoomInfo();
                                    pInformReady = (DefinedStructure.P_InformRoomInfo)packet.Convert(pInformReady.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pInformReady._roomNumber);
                                    roomInfo.UserReady(pInformReady._index);

                                    DefinedStructure.P_ShowReady pShowReady;
                                    pShowReady._index = pInformReady._index;
                                    pShowReady._isReady = roomInfo._UserArr[pInformReady._index]._IsReady ? 0 : 1;

                                    SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.ShowReady, pShowReady);

                                    break;

                                case DefinedProtocol.eFromClient.InformGameStart:

                                    DefinedStructure.P_InformRoomInfo pInformGameStart = new DefinedStructure.P_InformRoomInfo();
                                    pInformGameStart = (DefinedStructure.P_InformRoomInfo)packet.Convert(pInformGameStart.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pInformGameStart._roomNumber);

                                    if(CheckAllReady(roomInfo))
                                    {
                                        roomInfo.GameStart();
                                        Console.WriteLine("{0}번 방에서 게임이 시작되었습니다.", roomInfo._RoomNumber);

                                        DefinedStructure.P_GameStart pGameStart;
                                        pGameStart._skilcubeCnt = 8;
                                        pGameStart._flaskcubeCnt = 30;
                                        SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.GameStart, pGameStart);

                                        switch (roomInfo._Mode)
                                        {
                                            case "MyDeck":

                                                break;

                                            case "RandomDeck":
                                            case "AllDeck":
                                                
                                                DefinedStructure.P_GetAllCard pGetAllCard;
                                                pGetAllCard._roomNumber = roomInfo._RoomNumber;
                                                pGetAllCard._nickNameArr = string.Empty;

                                                for (int n = 0; n < roomInfo._UserArr.Length; n++)
                                                {
                                                    if (roomInfo._UserArr[n] != null && roomInfo._UserArr[n]._IsReady)
                                                        pGetAllCard._nickNameArr += roomInfo._UserArr[n]._NickName + ",";
                                                }

                                                _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.GetAllCard, pGetAllCard));

                                                break;

                                            case "AI":

                                                break;
                                        }
                                    }
                                    else
                                        ShowSystemMessage(packet._UUID, "CannotPlay");

                                    break;

                                case DefinedProtocol.eFromClient.FinishReadCard:

                                    DefinedStructure.P_InformRoomInfo pFinishReadCard = new DefinedStructure.P_InformRoomInfo();
                                    pFinishReadCard = (DefinedStructure.P_InformRoomInfo)packet.Convert(pFinishReadCard.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pFinishReadCard._roomNumber);
                                    roomInfo.UserFinishReadCard(pFinishReadCard._index);

                                    if(CheckAllReadCard(roomInfo))
                                    {
                                        Console.WriteLine("{0}번 방에서 카드 읽기 페이즈를 종료합니다.", roomInfo._RoomNumber);
                                        roomInfo._ThisTurn = roomInfo._Master - 1;
                                        if (roomInfo._ThisTurn < 0)
                                            roomInfo._ThisTurn = roomInfo._UserArr.Length - 1;

                                        while(roomInfo._UserArr[roomInfo._ThisTurn]._IsEmpty)
                                        {
                                            roomInfo._ThisTurn--;

                                            if (roomInfo._ThisTurn < 0)
                                                roomInfo._ThisTurn = roomInfo._UserArr.Length - 1;
                                        }

                                        DefinedStructure.P_ThisTurn pPickCardState;
                                        pPickCardState._index = roomInfo._ThisTurn;

                                        SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.PickCard, pPickCardState);
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.PickCard:

                                    DefinedStructure.P_PickCard pPickCard = new DefinedStructure.P_PickCard();
                                    pPickCard = (DefinedStructure.P_PickCard)packet.Convert(pPickCard.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pPickCard._roomNumber);
                                    userInfo = roomInfo.SearchUser(packet._UUID);

                                    Console.WriteLine("{0} 유저가 {1}번 카드를 선택하였습니다.", userInfo._NickName, pPickCard._cardIndex);

                                    if (userInfo.IsEmptyCardSlot())
                                    {
                                        if (roomInfo._CardDeck.PickCard((eCardField)_tbMgr.Get(eTableType.CardData).ToI(pPickCard._cardIndex, CardData.Index.Field.ToString()), pPickCard._cardIndex))
                                            RenewProjectBoard(roomInfo, pPickCard._cardIndex);
                                        else
                                            ShowSystemMessage(packet._UUID, "FailPickCard");

                                        int slotIndex = userInfo.AddCard(pPickCard._cardIndex);
                                        RenewCardSlot(roomInfo, userInfo, pPickCard._cardIndex, slotIndex);

                                        Console.WriteLine("방장의 UUID : {0} / 현재 턴 유저의 UUID : {1}", roomInfo._UserArr[roomInfo._Master]._UUID, packet._UUID);

                                        if (roomInfo._UserArr[roomInfo._Master]._UUID == packet._UUID)
                                        {   
                                            roomInfo._ThisTurn = roomInfo._Master;

                                            DefinedStructure.P_ThisTurn pGameTurn;
                                            pGameTurn._index = roomInfo._Master;

                                            SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.ChooseAction, pGameTurn);
                                        }
                                        else
                                        {
                                            if (--roomInfo._ThisTurn < 0)
                                                roomInfo._ThisTurn = roomInfo._UserArr.Length - 1;

                                            while (roomInfo._UserArr[roomInfo._ThisTurn] == null ||
                                                    roomInfo._UserArr[roomInfo._ThisTurn]._IsEmpty)
                                            {
                                                roomInfo._ThisTurn--;

                                                if (roomInfo._ThisTurn < 0)
                                                    roomInfo._ThisTurn = roomInfo._UserArr.Length - 1;
                                            }

                                            DefinedStructure.P_ThisTurn pPickCardState;
                                            pPickCardState._index = roomInfo._ThisTurn;
                                            SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.PickCard, pPickCardState);
                                        }
                                    }
                                    else
                                        ShowSystemMessage(packet._UUID, "NoSlot");

                                    break;
                                #endregion

                                #region Battle
                                case DefinedProtocol.eFromClient.SelectAction:

                                    DefinedStructure.P_SelectAction pSelectAction = new DefinedStructure.P_SelectAction();
                                    pSelectAction = (DefinedStructure.P_SelectAction)packet.Convert(pSelectAction.GetType());

                                    Console.WriteLine("{0} 유저가 {1} 행동을 선택하였습니다.", packet._UUID, pSelectAction._selectType == 0 ? "카드 가져오기" : "카드 회전하기");

                                    roomInfo = _roomInfoSort.GetRoom(pSelectAction._roomNumber);

                                    switch (pSelectAction._selectType)
                                    {
                                        case 0:

                                            if(!roomInfo._CardDeck._IsEmpty)
                                            {
                                                userInfo = roomInfo.SearchUser(packet._UUID);

                                                if(userInfo.IsEmptyCardSlot())
                                                {
                                                    DefinedStructure.P_GetCard pGetCard;
                                                    pGetCard._index = userInfo._Index;

                                                    Console.WriteLine("카드 가져오기를 선택하였습니다.");
                                                    SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.GetCard, pGetCard);
                                                }
                                                else
                                                    ShowSystemMessage(packet._UUID, "No_CardSlot");
                                            }
                                            else
                                                ShowSystemMessage(packet._UUID, "No_Card");

                                            break;

                                        case 1:

                                            userInfo = roomInfo.SearchUser(packet._UUID);

                                            if(userInfo._NowCardCnt > 0)
                                            {
                                                DefinedStructure.P_InformRotateCard pInformRotateCard;
                                                pInformRotateCard._index = userInfo._Index;
                                                pInformRotateCard._cardArr = new int[4];
                                                pInformRotateCard._cardRoteteInfo = new int[4];
                                                pInformRotateCard._turnCount = 2;

                                                for (int n = 0; n < userInfo._CardSlotCnt; n++)
                                                {
                                                    if(n < userInfo._UnLockSlotCnt)
                                                    {
                                                        if(userInfo._MyPickCard[n]._IsEmpty)
                                                            pInformRotateCard._cardArr[n] = 0;
                                                        else
                                                            pInformRotateCard._cardArr[n] = userInfo._MyPickCard[n]._CardIndex;
                                                    }
                                                    else
                                                        pInformRotateCard._cardArr[n] = -1;

                                                    pInformRotateCard._cardRoteteInfo[n] = userInfo._MyPickCard[n]._RotateInfo;
                                                }

                                                Console.WriteLine("카드 회전을 선택하였습니다.");
                                                Console.WriteLine("카드 정보 (1) : {0} // (2) : {1} // (3) : {2} // (4) : {3}", 
                                                    pInformRotateCard._cardArr[0], pInformRotateCard._cardArr[1], pInformRotateCard._cardArr[2], pInformRotateCard._cardArr[3]);
                                                SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.RotateCard, pInformRotateCard);
                                            }
                                            else
                                                ShowSystemMessage(packet._UUID, "No_RotatableCard");

                                            break;
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.PickCardInProgress:

                                    DefinedStructure.P_PickCard pPickCardInProgress = new DefinedStructure.P_PickCard();
                                    pPickCardInProgress = (DefinedStructure.P_PickCard)packet.Convert(pPickCardInProgress.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pPickCardInProgress._roomNumber);
                                    userInfo = roomInfo.SearchUser(packet._UUID);

                                    if (userInfo._IsApplyingEffect)
                                    {
                                        switch (userInfo._ApplyCard)
                                        {
                                            case 16:

                                                ReturnCard(roomInfo, userInfo, userInfo._ApplyCard);

                                                break;

                                            case 25:

                                                if(--userInfo._RepetitionCnt > 0)
                                                    GetCardByEffect(roomInfo, userInfo);
                                                else
                                                    ReturnCard(roomInfo, userInfo, userInfo._ApplyCard);

                                                break;

                                            case 27:

                                                SelectField(roomInfo, userInfo);

                                                break;
                                        }
                                    }
                                    else
                                    {
                                        RenewProjectBoard(roomInfo, pPickCardInProgress._cardIndex);

                                        int slotIndex2 = userInfo.AddCard(pPickCardInProgress._cardIndex);
                                        RenewCardSlot(roomInfo, userInfo, pPickCardInProgress._cardIndex, slotIndex2);
                                    }

                                    InformNowTurn(roomInfo, DefinedProtocol.eToClient.ChooseAction);

                                    break;

                                case DefinedProtocol.eFromClient.RotateInfo:

                                    DefinedStructure.P_RotateInfo pRotateInfo = new DefinedStructure.P_RotateInfo();
                                    pRotateInfo = (DefinedStructure.P_RotateInfo)packet.Convert(pRotateInfo.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pRotateInfo._roomNumber);

                                    DefinedStructure.P_ShowRotateInfo pShowRotateInfo;
                                    pShowRotateInfo._index = pRotateInfo._index;
                                    pShowRotateInfo._rotateValue = pRotateInfo._rotateValue;
                                    pShowRotateInfo._restCount = pRotateInfo._restCount;

                                    SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.ShowRotateInfo, pShowRotateInfo);

                                    break;

                                case DefinedProtocol.eFromClient.FinishRotate:

                                    DefinedStructure.P_FinishRotate pFinishRotate = new DefinedStructure.P_FinishRotate();
                                    pFinishRotate = (DefinedStructure.P_FinishRotate)packet.Convert(pFinishRotate.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pFinishRotate._roomNumber);
                                    userInfo = roomInfo.SearchUser(packet._UUID);

                                    if (userInfo._IsApplyingEffect)
                                    {
                                        switch (userInfo._ApplyCard)
                                        {
                                            case 13:

                                                userInfo.AddFlaskCube(1);
                                                roomInfo._MaxFlaskCube -= 1;
                                                ShowUserFlaskCube(roomInfo, userInfo);
                                                ShowTotalFlaskCube(roomInfo);
                                                ReturnCard(roomInfo, userInfo, userInfo._ApplyCard);

                                                break;

                                            case 39:

                                                SelectMyCard(roomInfo, userInfo);

                                                break;
                                        }
                                    }
                                    else
                                    {
                                        for (int n = 0; n < userInfo._CardSlotCnt; n++)
                                            userInfo._MyPickCard[n]._RotateInfo += pFinishRotate._rotateCardInfoArr[n];

                                        CheckCompleteCard(roomInfo, userInfo);
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.ChooseCompleteCard:

                                    DefinedStructure.P_ChooseCompleteCard pChooseCompleteCard = new DefinedStructure.P_ChooseCompleteCard();
                                    pChooseCompleteCard = (DefinedStructure.P_ChooseCompleteCard)packet.Convert(pChooseCompleteCard.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pChooseCompleteCard._roomNumber);
                                    userInfo= roomInfo.SearchUser(packet._UUID);

                                    MoveCube(roomInfo, userInfo);
                                    ApplyEffect(roomInfo, userInfo._MyPickCard[pChooseCompleteCard._index]._CardIndex, userInfo);

                                    CheckCompleteCard(roomInfo, userInfo);

                                    break;

                                case DefinedProtocol.eFromClient.SelectFieldResult:

                                    DefinedStructure.P_SelectFieldResult pSelectFieldResult = new DefinedStructure.P_SelectFieldResult();
                                    pSelectFieldResult = (DefinedStructure.P_SelectFieldResult)packet.Convert(pSelectFieldResult.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pSelectFieldResult._roomNumber);
                                    userInfo = roomInfo.SearchUser(packet._UUID);

                                    if(userInfo._IsApplyingEffect)
                                    {
                                        switch(userInfo._ApplyCard)
                                        {
                                            case 2:

                                                if(userInfo.CheckNotMostField(pSelectFieldResult._field))
                                                {
                                                    RenewSkillCube(roomInfo, userInfo, pSelectFieldResult._field);
                                                    ReturnCard(roomInfo, userInfo, userInfo._ApplyCard);
                                                    InformNowTurn(roomInfo, DefinedProtocol.eToClient.ChooseAction);
                                                }   
                                                else
                                                    ShowSystemMessage(packet._UUID, "Not_Correct_Select");

                                                break;

                                            case 27:

                                                for(int n = 0; n < userInfo._UnLockSlotCnt; n++)
                                                {
                                                    if(_tbMgr.Get(eTableType.CardData).ToI(userInfo._MyPickCard[n]._CardIndex, CardData.Index.Field.ToString()) == pSelectFieldResult._field)
                                                        userInfo._MyPickCard[n]._RotateInfo++;
                                                }

                                                ReturnCard(roomInfo, userInfo, userInfo._ApplyCard);
                                                CheckCompleteCard(roomInfo, userInfo);

                                                break;
                                        }
                                    }
                                    else if(!userInfo._IsFinishGameOver)
                                        userInfo.SelectPhysicsEffectField(pSelectFieldResult._field);

                                    break;

                                case DefinedProtocol.eFromClient.SelectCardResult:

                                    DefinedStructure.P_PickCard pSelectCardResult = new DefinedStructure.P_PickCard();
                                    pSelectCardResult = (DefinedStructure.P_PickCard)packet.Convert(pSelectCardResult.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pSelectCardResult._roomNumber);
                                    userInfo = roomInfo.SearchUser(packet._UUID);
                                    
                                    if (userInfo._IsApplyingEffect)
                                    {
                                        switch (userInfo._ApplyCard)
                                        {
                                            case 3:

                                                roomInfo._CardDeck.AddFlaskOnCard(pSelectCardResult._cardIndex, 1);
                                                roomInfo._MaxFlaskCube -= 1;
                                                RenewSkillCube(roomInfo, userInfo,
                                                    _tbMgr.Get(eTableType.CardData).ToI(pSelectCardResult._cardIndex, CardData.Index.Kind.ToString()));
                                                ShowTotalFlaskCube(roomInfo);

                                                ReturnCard(roomInfo, userInfo, userInfo._ApplyCard);

                                                break;

                                            case 37:

                                                if(!userInfo._IsSecondEffect)
                                                {
                                                    userInfo._IsSecondEffect = true;

                                                    for(int n = 0; n < roomInfo._UserArr.Length; n++)
                                                    {
                                                        if(roomInfo._UserArr[n] != null && !roomInfo._UserArr[n]._IsEmpty)
                                                        {
                                                            if(roomInfo._UserArr[n].IsHaveCard(pSelectCardResult._cardIndex))
                                                            {
                                                                roomInfo._UserArr[n].AddFlaskOnCard(pSelectCardResult._cardIndex, 1);
                                                                roomInfo._MaxFlaskCube -= 1;
                                                                ShowTotalFlaskCube(roomInfo);
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    SelectMyCard(roomInfo, userInfo);
                                                }
                                                else
                                                {
                                                    userInfo.AddFlaskOnCard(pSelectCardResult._cardIndex, 3);
                                                    roomInfo._MaxFlaskCube -= 3;
                                                    ShowTotalFlaskCube(roomInfo);

                                                    InformNowTurn(roomInfo, DefinedProtocol.eToClient.ChooseAction);
                                                }
                                                
                                                break;

                                            case 39:

                                                for(int n = 0; n < userInfo._UnLockSlotCnt; n++)
                                                {
                                                    if(userInfo._MyPickCard[n]._CardIndex == pSelectCardResult._cardIndex)
                                                    {
                                                        userInfo._FlaskOnCard[n] = 2;
                                                        roomInfo._MaxFlaskCube -= 2;
                                                        ShowTotalFlaskCube(roomInfo);
                                                        break;
                                                    }   
                                                }

                                                ReturnCard(roomInfo, userInfo, userInfo._ApplyCard);

                                                break;
                                        }

                                        InformNowTurn(roomInfo, DefinedProtocol.eToClient.ChooseAction);
                                    }

                                    break;

                                case DefinedProtocol.eFromClient.FinishGameOver:

                                    DefinedStructure.P_InformRoomInfo pFinishGameOver = new DefinedStructure.P_InformRoomInfo();
                                    pFinishGameOver = (DefinedStructure.P_InformRoomInfo)packet.Convert(pFinishGameOver.GetType());

                                    roomInfo = _roomInfoSort.GetRoom(pFinishGameOver._roomNumber);
                                    userInfo = roomInfo.SearchUser(packet._UUID);

                                    userInfo._IsFinishGameOver = true;

                                    if(roomInfo.CheckAllFinishGameOver())
                                    {
                                        int[] score = new int[roomInfo._UserArr.Length];
                                        int[] userIndex = new int[roomInfo._UserArr.Length];
                                        for (int n = 0; n < roomInfo._UserArr.Length; n++)
                                        {
                                            if (roomInfo._UserArr[n] != null && !roomInfo._UserArr[n]._IsEmpty)
                                            {
                                                roomInfo._UserArr[n].CaculateScore();
                                                score[n] = roomInfo._UserArr[n]._GameScore;
                                                userIndex[n] = n;
                                            }
                                        }

                                        Array.Sort(score, userIndex);

                                        DefinedStructure.P_ShowGameResult pShowGameResult;
                                        pShowGameResult._firstCharacterIndex = 0;
                                        pShowGameResult._rankNickName = string.Empty;
                                        pShowGameResult._rankScore = new int[4];
                                        Array.Copy(score, pShowGameResult._rankScore, pShowGameResult._rankScore.Length);
                                        pShowGameResult._accountExp = 0;
                                        pShowGameResult._physicsExp = 0;
                                        pShowGameResult._chemistryExp = 0;
                                        pShowGameResult._biologyExp = 0;
                                        pShowGameResult._astonomyExp = 0;

                                        TimeSpan timeSpan = DateTime.Now - roomInfo._GameStartTime;
                                        for (int n = score.Length - 1; n >= 0; n--)
                                        {
                                            if(roomInfo._UserArr[userIndex[n]] != null && !roomInfo._UserArr[userIndex[n]]._IsEmpty)
                                            {
                                                if (n == score.Length - 1)
                                                    pShowGameResult._firstCharacterIndex = roomInfo._UserArr[userIndex[n]]._CharacterIndex;
                                                
                                                pShowGameResult._rankNickName += roomInfo._UserArr[userIndex[n]]._NickName + ",";

                                                pShowGameResult._accountExp = (int)timeSpan.TotalSeconds;
                                                pShowGameResult._physicsExp = roomInfo._UserArr[n]._CompleteCountDic[eCardField.Physics] * 10;
                                                pShowGameResult._chemistryExp = roomInfo._UserArr[n]._CompleteCountDic[eCardField.Chemistry] * 10;
                                                pShowGameResult._biologyExp = roomInfo._UserArr[n]._CompleteCountDic[eCardField.Biology] * 10;
                                                pShowGameResult._astonomyExp = roomInfo._UserArr[n]._CompleteCountDic[eCardField.Astronomy] * 10;
                                            }
                                        }

                                        SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.ShowGameResult, pShowGameResult);
                                    }

                                    break;
                                #endregion

                                #region Shop
                                case DefinedProtocol.eFromClient.RequsetShopInfo:

                                    DefinedStructure.P_RequestShopInfo pRequsetShopInfo = new DefinedStructure.P_RequestShopInfo();
                                    pRequsetShopInfo = (DefinedStructure.P_RequestShopInfo)packet.Convert(pRequsetShopInfo.GetType());

                                    DefinedStructure.P_GetShopInfo pGetShopInfo;
                                    pGetShopInfo._UUID = packet._UUID;
                                    pGetShopInfo._nickName = pRequsetShopInfo._nickName;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.GetShopInfo, pGetShopInfo));
                                    Console.WriteLine("{0} 유저가 상점 정보를 요청합니다.", packet._UUID);

                                    break;

                                case DefinedProtocol.eFromClient.BuyItem:

                                    DefinedStructure.P_BuyItem pBuyItem = new DefinedStructure.P_BuyItem();
                                    pBuyItem = (DefinedStructure.P_BuyItem)packet.Convert(pBuyItem.GetType());

                                    DefinedStructure.P_TryBuyItem pTryBuyItem;
                                    pTryBuyItem._UUID = packet._UUID;
                                    pTryBuyItem._nickName = pBuyItem._nickName;
                                    pTryBuyItem._itemIndex = pBuyItem._itemIndex;
                                    pTryBuyItem._exchangeType = _tbMgr.Get(eTableType.ItemData).ToS(pBuyItem._itemIndex, ItemData.Index.ExchangeType.ToString());
                                    pTryBuyItem._coin = _tbMgr.Get(eTableType.ItemData).ToI(pBuyItem._itemIndex, ItemData.Index.Coin.ToString());
                                    pTryBuyItem._coinKind = _tbMgr.Get(eTableType.ItemData).ToS(pBuyItem._itemIndex, ItemData.Index.CoinKind.ToString());
                                    pTryBuyItem._exchangeResult = _tbMgr.Get(eTableType.ItemData).ToI(pBuyItem._itemIndex, ItemData.Index.ExchangeResult.ToString());

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.TryBuyItem, pTryBuyItem));
                                    Console.WriteLine("{0} 유저가 {1}번 아이템 구매를 시도합니다.", pTryBuyItem._nickName, pTryBuyItem._itemIndex);

                                    break;
                                #endregion

                                case DefinedProtocol.eFromClient.RequestFriendList:

                                    DefinedStructure.P_RequestFriendList pRequestFriendList = new DefinedStructure.P_RequestFriendList();
                                    pRequestFriendList = (DefinedStructure.P_RequestFriendList)packet.Convert(pRequestFriendList.GetType());

                                    DefinedStructure.P_GetFriendList pGetFriendList;
                                    pGetFriendList._UUID = packet._UUID;
                                    pGetFriendList._nickName = pRequestFriendList._nickName;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.GetFriendList, pGetFriendList));
                                    Console.WriteLine("{0} 유저가 친구 목록을 불러오는 중입니다.", pGetFriendList._nickName);

                                    break;

                                case DefinedProtocol.eFromClient.ConnectionTerminate:

                                    Console.WriteLine("{0} 유저가 접속 종료를 시도합니다.", packet._UUID);

                                    DefinedStructure.P_Request pCofirmTerminate;
                                    _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ConfirmTerminate, pCofirmTerminate, packet._UUID));

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


                        RoomInfo roomInfo;
                        UserInfo userInfo;

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

                            #region Game Ready
                            case DefinedProtocol.eToServer.ShowBattleInfo:

                                DefinedStructure.P_ShowBattleInfo pShowBattleInfo = new DefinedStructure.P_ShowBattleInfo();
                                pShowBattleInfo = (DefinedStructure.P_ShowBattleInfo)tData.Convert(pShowBattleInfo.GetType());

                                roomInfo = _roomInfoSort.GetRoom(pShowBattleInfo._roomNumber);
                                roomInfo.AddUser(pShowBattleInfo._UUID, pShowBattleInfo._nickName, pShowBattleInfo._characIndex, pShowBattleInfo._accountlevel);
                                
                                for(int n = 0; n < roomInfo._UserArr.Length; n++)
                                {
                                    if(roomInfo._UserArr[n] != null && !roomInfo._UserArr[n]._IsEmpty)
                                    {
                                        DefinedStructure.P_UserInfo pUserInfo;
                                        pUserInfo._roomNumber = pShowBattleInfo._roomNumber;
                                        pUserInfo._index = n;
                                        pUserInfo._nickName = roomInfo._UserArr[n]._NickName;
                                        pUserInfo._accountLevel = roomInfo._UserArr[n]._Level;
                                        pUserInfo._isReady = roomInfo._UserArr[n]._IsReady ? 0 : 1;

                                        SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.EnterRoom, pUserInfo);
                                    }
                                }

                                DefinedStructure.P_MasterInfo pMasterInfo;
                                pMasterInfo._masterIndex = roomInfo._Master;

                                SendBufferInRoom(roomInfo, DefinedProtocol.eToClient.ShowMaster, pMasterInfo);

                                break;

                            case DefinedProtocol.eToServer.ShowAllCard:

                                DefinedStructure.P_ShowAllCard pShowAllCard = new DefinedStructure.P_ShowAllCard();
                                pShowAllCard = (DefinedStructure.P_ShowAllCard)tData.Convert(pShowAllCard.GetType());

                                RoomInfo roomToBattle = _roomInfoSort.GetRoom(pShowAllCard._roomNum);

                                switch(roomToBattle._Mode)
                                {
                                    case "RandomDeck":

                                        PickCardRandomly(roomToBattle, pShowAllCard._cardArr, pShowAllCard._cardCount);

                                        break;

                                    case "AllDeck":
                                        break;
                                }

                                break;
                            #endregion

                            #region Shop
                            case DefinedProtocol.eToServer.UserShopInfo:

                                DefinedStructure.P_UserShopInfo pUserShopInfo = new DefinedStructure.P_UserShopInfo();
                                pUserShopInfo = (DefinedStructure.P_UserShopInfo)tData.Convert(pUserShopInfo.GetType());

                                ShowUserItem(pUserShopInfo._UUID, pUserShopInfo._itemIndex, pUserShopInfo._itemCount);

                                break;

                            case DefinedProtocol.eToServer.FinishUserShopInfo:

                                DefinedStructure.P_FinishUserShopInfo pFinishUserShopInfo = new DefinedStructure.P_FinishUserShopInfo();
                                pFinishUserShopInfo = (DefinedStructure.P_FinishUserShopInfo)tData.Convert(pFinishUserShopInfo.GetType());

                                DefinedStructure.P_EndUserShopInfo pEndUserShopInfo;
                                pEndUserShopInfo._coinArr = new int[6];
                                Array.Copy(pFinishUserShopInfo._coinArr, pEndUserShopInfo._coinArr, pEndUserShopInfo._coinArr.Length);

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.EndUserShopInfo, pEndUserShopInfo, pFinishUserShopInfo._UUID));

                                break;

                            case DefinedProtocol.eToServer.ResultBuyItem:

                                DefinedStructure.P_ResultBuyItem pResultBuyItem = new DefinedStructure.P_ResultBuyItem();
                                pResultBuyItem = (DefinedStructure.P_ResultBuyItem)tData.Convert(pResultBuyItem.GetType());
                                
                                if (pResultBuyItem._isSuccess == 0)
                                {
                                    if(pResultBuyItem._exchangeType == "Item")
                                    {
                                        ShowUserItem(pResultBuyItem._UUID, pResultBuyItem._itemIndex, pResultBuyItem._itemCount);
                                        ShowUserCoin(pResultBuyItem._UUID, pResultBuyItem._coinKind, pResultBuyItem._remainCoin);
                                        Console.WriteLine("{0} 유저가 {1}번 아이템을 구매했습니다.", pResultBuyItem._UUID, pResultBuyItem._itemIndex);
                                    }   
                                    else
                                    {   
                                        ShowUserCoin(pResultBuyItem._UUID, pResultBuyItem._exchangeType, pResultBuyItem._itemCount);
                                        ShowUserCoin(pResultBuyItem._UUID, pResultBuyItem._coinKind, pResultBuyItem._remainCoin);
                                        Console.WriteLine("{0} 유저가 {1} 코인을 구매했습니다.", pResultBuyItem._UUID, pResultBuyItem._exchangeType);
                                    }
                                }
                                else
                                    ShowSystemMessage(pResultBuyItem._UUID, "Fail_Buy_Item");

                                break;
                            #endregion

                            #region community
                            case DefinedProtocol.eToServer.ResultFriendList:

                                DefinedStructure.P_UserFriendList pUserFriendList = new DefinedStructure.P_UserFriendList();
                                pUserFriendList = (DefinedStructure.P_UserFriendList)tData.Convert(pUserFriendList.GetType());

                                DefinedStructure.P_ShowFriendList pShowFriendList;
                                pShowFriendList._friendNickName = pUserFriendList._friendNickName;
                                pShowFriendList._friendLevel = new int[10];
                                Array.Copy(pUserFriendList._friendLevel, pShowFriendList._friendLevel, pShowFriendList._friendLevel.Length);
                                pShowFriendList._receiveNickName = pUserFriendList._receiveNickName;
                                pShowFriendList._receiveLevel = new int[10];
                                Array.Copy(pUserFriendList._receiveLevel, pShowFriendList._receiveLevel, pShowFriendList._receiveLevel.Length);
                                pShowFriendList._withNickName = pUserFriendList._withNickName;
                                pShowFriendList._withLevel = new int[10];
                                Array.Copy(pUserFriendList._withLevel, pShowFriendList._withLevel, pShowFriendList._withLevel.Length);
                                pShowFriendList._withDate = pUserFriendList._withDate;

                                _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowFriendList, pShowFriendList, pUserFriendList._UUID));

                                break;
                                #endregion
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

        bool CheckAllReady(RoomInfo room)
        {
            int readyCount = 0;
            for (int n = 0; n < room._UserArr.Length; n++)
            {
                if(room._UserArr[n] != null && !room._UserArr[n]._IsEmpty && n != room._Master && room._UserArr[n]._IsReady)
                    readyCount++;
            }

            if (readyCount != 0 && readyCount == room._NowMemeberCnt - 1)
                return true;
            else
                return false;

        }

        bool CheckAllReadCard(RoomInfo room)
        {
            for (int n = 0; n < room._UserArr.Length; n++)
            {
                if(room._UserArr[n] != null && !room._UserArr[n]._IsEmpty)
                {
                    if (!room._UserArr[n]._IsFinishReadCard)
                        return false;
                }
            }
            
            return true;
        }

        void PickCardRandomly(RoomInfo room, int[] cardArr, int cardCnt)
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
                while (room._CardDeck.IsContain((eCardField)_tbMgr.Get(eTableType.CardData).ToI(normalCard[selectIndex], CardData.Index.Field.ToString()), normalCard[selectIndex])
                        || normalCard[selectIndex] == 0);

                room._CardDeck.AddCard((eCardField)_tbMgr.Get(eTableType.CardData).ToI(normalCard[selectIndex], CardData.Index.Field.ToString()), normalCard[selectIndex]);

                RemoveLinkCard(normalCard[selectIndex], normalCard);

                if (normalCard.Contains(normalCard[selectIndex]))
                    normalCard.Remove(normalCard[selectIndex]);
            }

            while(!room._CardDeck.IsOver())
            {
                do
                {
                    selectIndex = rd.Next(0, nonNormalCard.Count);
                }
                while (room._CardDeck.IsContain((eCardField)_tbMgr.Get(eTableType.CardData).ToI(nonNormalCard[selectIndex], CardData.Index.Field.ToString()), nonNormalCard[selectIndex])
                        || room._CardDeck.FieldCount((eCardField)_tbMgr.Get(eTableType.CardData).ToI(nonNormalCard[selectIndex], CardData.Index.Field.ToString())) >= 3 
                        || nonNormalCard[selectIndex] == 0);

                room._CardDeck.AddCard((eCardField)_tbMgr.Get(eTableType.CardData).ToI(nonNormalCard[selectIndex], CardData.Index.Field.ToString()), nonNormalCard[selectIndex]);

                RemoveLinkCard(nonNormalCard[selectIndex], nonNormalCard);

                if (nonNormalCard.Contains(nonNormalCard[selectIndex]))
                    nonNormalCard.Remove(nonNormalCard[selectIndex]);
            }

            DefinedStructure.P_PickedCard pPickedCard;
            pPickedCard._pickedCardArr = new int[12];
            int packIdx = 0;

            for(int n = 0; n < (int)eCardField.max; n++)
            {
                int[] fieldCard = room._CardDeck.GetFieldCard((eCardField)n);

                for (int m = 0; m < fieldCard.Length; m++)
                {
                    pPickedCard._pickedCardArr[packIdx++] = fieldCard[m];
                }
            }

            SendBufferInRoom(room, DefinedProtocol.eToClient.ShowPickedCard, pPickedCard);
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

        void RenewProjectBoard(RoomInfo room, int cardIndex)
        {
            DefinedStructure.P_ShowProjectBoard pShowProjectBoard;
            pShowProjectBoard._cardIndex = cardIndex;
            pShowProjectBoard._cardCount = room._CardDeck.CardCount(cardIndex);
            SendBufferInRoom(room, DefinedProtocol.eToClient.ShowProjectBoard, pShowProjectBoard);
        }

        void InformNowTurn(RoomInfo room, DefinedProtocol.eToClient protocolType)
        {
            if (++room._ThisTurn >= room._UserArr.Length)
                room._ThisTurn = 0;

            while (room._UserArr[room._ThisTurn] == null || room._UserArr[room._ThisTurn]._IsEmpty)
            {
                room._ThisTurn++;

                if (room._ThisTurn >= room._UserArr.Length)
                    room._ThisTurn = 0;
            }

            if(room._IsFinalTurn && room._ThisTurn == room._Master)
            {
                for(int n = 0; n < room._UserArr.Length; n++)
                {
                    if(room._UserArr[n] != null)
                    {
                        room._UserArr[n]._IsReady = false;

                        DefinedStructure.P_GameOver pGameOver;

                        int count = 0;
                        if(room._UserArr[n].IsPhysicsSpecificScore(out count))
                            pGameOver._specificScore = count;
                        else
                            pGameOver._specificScore = 0;

                        room._UserArr[n]._IsFinishGameOver = pGameOver._specificScore == 0;

                        _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.GameOver, pGameOver, room._UserArr[n]._UUID));
                    }
                }
            }
            else
            {
                DefinedStructure.P_ThisTurn pInformNowTurn;
                pInformNowTurn._index = room._ThisTurn;

                SendBufferInRoom(room, protocolType, pInformNowTurn);
            }
        }

        void SendBufferInRoom(RoomInfo room, DefinedProtocol.eToClient protocolType, object str)
        {
            for (int n = 0; n < room._UserArr.Length; n++)
            {
                if (room._UserArr[n] != null && !room._UserArr[n]._IsEmpty)
                {
                    _toClientQueue.Enqueue(_socketManager.AddToQueue(protocolType, str, room._UserArr[n]._UUID));
                }
            }
        }

        void ApplyEffect(RoomInfo room, int cardIndex, UserInfo user)
        {
            Console.WriteLine("{0} 유저가 {1}번 카드의 완료 효과를 얻었습니다.", user._NickName, cardIndex);

            int flask = 0;
            switch (cardIndex)
            {
                case 2:

                    user._IsApplyingEffect = true;
                    user._ApplyCard = cardIndex;
                    SelectField(room, user);

                    break;

                case 3:

                    user._IsApplyingEffect = true;
                    user._ApplyCard = cardIndex;
                    if (!room._CardDeck._IsEmpty)
                    {
                        DefinedStructure.P_GetCard pGetCard;
                        pGetCard._index = user._Index;

                        SendBufferInRoom(room, DefinedProtocol.eToClient.SelectCard, pGetCard);
                    }
                    else
                        ReturnCard(room, user, cardIndex);

                    break;

                case 5:

                    flask = user.MaxSkillPower() / 2;
                    user.AddFlaskCube(flask);
                    room._MaxFlaskCube -= flask;
                    
                    ShowUserFlaskCube(room, user);
                    ShowTotalFlaskCube(room);

                    break;

                case 13:

                    user._IsApplyingEffect = true;
                    user._ApplyCard = cardIndex;
                    if (user._NowCardCnt > 1)
                    {
                        DefinedStructure.P_InformRotateCard pInformRotateCard;
                        pInformRotateCard._index = user._Index;
                        pInformRotateCard._cardArr = new int[4];
                        pInformRotateCard._cardRoteteInfo = new int[4];
                        pInformRotateCard._turnCount = 3;

                        for (int n = 0; n < user._CardSlotCnt; n++)
                        {
                            if (n < user._UnLockSlotCnt)
                            {
                                if (user._MyPickCard[n]._CardIndex == 0)
                                    pInformRotateCard._cardArr[n] = 0;
                                else
                                    pInformRotateCard._cardArr[n] = user._MyPickCard[n]._CardIndex;
                            }
                            else
                                pInformRotateCard._cardArr[n] = -1;

                            pInformRotateCard._cardRoteteInfo[n] = user._MyPickCard[n]._RotateInfo;
                        }

                        SendBufferInRoom(room, DefinedProtocol.eToClient.RotateCard, pInformRotateCard);
                    }
                    else
                    {
                        user.AddFlaskCube(1);
                        room._MaxFlaskCube -= 1;
                        ShowUserFlaskCube(room, user);
                        ShowTotalFlaskCube(room);
                        ReturnCard(room, user, cardIndex);
                    }

                    break;

                case 16:

                    user._IsApplyingEffect = true;
                    user._ApplyCard = cardIndex;
                    if (user._NowCardCnt > 0)
                    {
                        DefinedStructure.P_InformRotateCard pInformRotateCard;
                        pInformRotateCard._index = user._Index;
                        pInformRotateCard._cardArr = new int[4];
                        pInformRotateCard._cardRoteteInfo = new int[4];
                        pInformRotateCard._turnCount = 2;

                        for (int n = 0; n < user._CardSlotCnt; n++)
                        {
                            if (n < user._UnLockSlotCnt)
                            {
                                if (user._MyPickCard[n]._IsEmpty)
                                    pInformRotateCard._cardArr[n] = 0;
                                else
                                    pInformRotateCard._cardArr[n] = user._MyPickCard[n]._CardIndex;
                            }
                            else
                                pInformRotateCard._cardArr[n] = -1;

                            pInformRotateCard._cardRoteteInfo[n] = user._MyPickCard[n]._RotateInfo;
                        }

                        SendBufferInRoom(room, DefinedProtocol.eToClient.RotateCard, pInformRotateCard);
                    }
                    else
                        GetCardByEffect(room, user);

                    break;

                case 17:

                    flask = user.AllSkillCubeCount() / 2;
                    user.AddFlaskCube(flask);
                    room._MaxFlaskCube -= flask;

                    ShowUserFlaskCube(room, user);
                    ShowTotalFlaskCube(room);

                    break;

                case 25:

                    user._IsApplyingEffect = true;
                    user._ApplyCard = cardIndex;
                    user._RepetitionCnt = 2;

                    GetCardByEffect(room, user);

                    break;

                case 27:

                    user._IsApplyingEffect = true;
                    user._ApplyCard = cardIndex;

                    GetCardByEffect(room, user);

                    break;

                case 29:

                    user._IsApplyingEffect = true;
                    user._ApplyCard = cardIndex;

                    flask = user.MyCardCount();
                    user.AddFlaskCube(flask);
                    room._MaxFlaskCube -= flask;

                    ShowUserFlaskCube(room, user);
                    ShowTotalFlaskCube(room);

                    GetCardByEffect(room, user);

                    break;

                case 37:

                    user._IsSecondEffect = false;
                    SelectOtherCard(room, user);

                    break;

                case 39:

                    user._IsApplyingEffect = true;
                    user._ApplyCard = cardIndex;
                    if (user._NowCardCnt > 0)
                    {
                        DefinedStructure.P_InformRotateCard pInformRotateCard;
                        pInformRotateCard._index = user._Index;
                        pInformRotateCard._cardArr = new int[4];
                        pInformRotateCard._cardRoteteInfo = new int[4];
                        pInformRotateCard._turnCount = 2;

                        for (int n = 0; n < user._CardSlotCnt; n++)
                        {
                            if (n < user._UnLockSlotCnt)
                            {
                                if (user._MyPickCard[n]._IsEmpty)
                                    pInformRotateCard._cardArr[n] = 0;
                                else
                                    pInformRotateCard._cardArr[n] = user._MyPickCard[n]._CardIndex;
                            }
                            else
                                pInformRotateCard._cardArr[n] = -1;

                            pInformRotateCard._cardRoteteInfo[n] = user._MyPickCard[n]._RotateInfo;
                        }

                        SendBufferInRoom(room, DefinedProtocol.eToClient.RotateCard, pInformRotateCard);
                    }
                    else
                        SelectMyCard(room, user);

                    break;
                    
                case 41:

                    flask = user._FlaskCubeCnt / 3;
                    user.AddFlaskCube(flask);
                    room._MaxFlaskCube -= flask;

                    ShowUserFlaskCube(room, user);
                    ShowTotalFlaskCube(room);

                    break;
            }
        }

        void CheckCompleteCard(RoomInfo room, UserInfo user)
        {
            int completeCnt;
            if (user.IsComplete(out completeCnt))
            {
                if (completeCnt >= 2)
                {
                    DefinedStructure.P_SelectCompleteCard pSelectCompleteCard;
                    pSelectCompleteCard._index = user._Index;
                    pSelectCompleteCard._cardArr = new int[user._CardSlotCnt];
                    for (int n = 0; n < user._CardSlotCnt; n++)
                    {
                        if (user._MyPickCard[n]._RotateInfo >= 4)
                            pSelectCompleteCard._cardArr[n] = user._MyPickCard[n]._RotateInfo;
                        else
                            pSelectCompleteCard._cardArr[n] = 0;
                    }

                    SendBufferInRoom(room, DefinedProtocol.eToClient.SelectCompleteCard, pSelectCompleteCard);
                }
                else
                {
                    MoveCube(room, user);

                    int completeCard = user.GetCompleteCard();
                    string applyText = _tbMgr.Get(eTableType.CardData).ToS(completeCard, CardData.Index.Apply.ToString());

                    switch(applyText)
                    {
                        case "Now":

                            ApplyEffect(room, completeCard, user);

                            break;

                        case "Later":

                            ApplyEffect(room, completeCard, user);

                            return;
                    }

                    ReturnCard(room, user, completeCard);
                }
            }

            InformNowTurn(room, DefinedProtocol.eToClient.ChooseAction);
        }

        void MoveCube(RoomInfo room, UserInfo user)
        {
            int completeCardIndex = user.GetCompleteCard();
            int flaskCnt = 0;
            if (room._CardDeck.GetFlaskOnCard((eCardField)_tbMgr.Get(eTableType.CardData).ToI(completeCardIndex, CardData.Index.Field.ToString()), completeCardIndex, out flaskCnt))
            {
                user._FlaskCubeCnt += flaskCnt;
                Console.WriteLine("{0} 유저가 {1} 카드 위에 있는 플라스크 큐브 {2}개를 자신의 슬롯으로 옮겼습니다.", user._NickName, completeCardIndex, flaskCnt);

                ShowTotalFlaskCube(room);
                ShowUserFlaskCube(room, user);

                if (!user._IsFlaskEffect && user._FlaskCubeCnt >= 5)
                {
                    user.AddCardSlot();
                    ShowSlot(room, user);
                }   
            }

            RenewSkillCube(room, user, _tbMgr.Get(eTableType.CardData).ToI(completeCardIndex, CardData.Index.Field.ToString()));
        }

        void ShowSlot(RoomInfo room, UserInfo user)
        {
            DefinedStructure.P_ShowUserSlot pShowUserSlot;
            pShowUserSlot._index = user._Index;
            pShowUserSlot._unLockSlot = user._UnLockSlotCnt;

            SendBufferInRoom(room, DefinedProtocol.eToClient.ShowUserSlot, pShowUserSlot);
        }

        void ShowUserFlaskCube(RoomInfo room, UserInfo user)
        {
            DefinedStructure.P_ShowUserFlask pShowUserFlask;
            pShowUserFlask._index = user._Index;
            pShowUserFlask._userFlask = user._FlaskCubeCnt;
            SendBufferInRoom(room, DefinedProtocol.eToClient.ShowUserFlask, pShowUserFlask);
        }

        void ShowTotalFlaskCube(RoomInfo room)
        {
            DefinedStructure.P_ShowTotalFlask pShowTotalFlask;
            pShowTotalFlask._totalFlask = room._MaxFlaskCube;
            SendBufferInRoom(room, DefinedProtocol.eToClient.ShowTotalFlask, pShowTotalFlask);
        }

        void RenewCardSlot(RoomInfo room, UserInfo user, int cardIndex, int slotIndex)
        {
            DefinedStructure.P_ShowPickCard pShowPickCard;
            pShowPickCard._index = user._Index;
            pShowPickCard._cardIndex = cardIndex;
            pShowPickCard._slotIndex = slotIndex;

            SendBufferInRoom(room, DefinedProtocol.eToClient.ShowPickCard, pShowPickCard);
        }

        void DeleteCardSlot(RoomInfo room, UserInfo user, int slotIndex)
        {
            DefinedStructure.P_DeletePickCard pDeletePickCard;
            pDeletePickCard._index = user._Index;
            pDeletePickCard._slotIndex = slotIndex;

            SendBufferInRoom(room, DefinedProtocol.eToClient.DeletePickCard, pDeletePickCard);
        }

        void SelectField(RoomInfo room, UserInfo user)
        {
            DefinedStructure.P_SelectField pSelectField;
            pSelectField._userIndex = user._Index;

            SendBufferInRoom(room, DefinedProtocol.eToClient.SelectField, pSelectField);
        }

        void RenewSkillCube(RoomInfo room, UserInfo user, int field)
        {
            if (user.MoveSkillCube((eCardField)field, room))
            {
                DefinedStructure.P_ShowTotalSkill pShowTotalSkill;
                pShowTotalSkill._totalSkill = room._MaxSkillCube;
                SendBufferInRoom(room, DefinedProtocol.eToClient.ShowTotalSkill, pShowTotalSkill);
            }

            DefinedStructure.P_ShowUserSkill pShowUserSkill;
            pShowUserSkill._index = user._Index;
            pShowUserSkill._field = field;
            pShowUserSkill._userSkill = user.AllSkillCubeCount();
            pShowUserSkill._userSkillPos = new int[5];
            int[] userskill = user.SkillCubePos((eCardField)pShowUserSkill._field);
            for (int n = 0; n < userskill.Length; n++)
                pShowUserSkill._userSkillPos[n] = userskill[n];
            SendBufferInRoom(room, DefinedProtocol.eToClient.ShowUserSkill, pShowUserSkill);

            Console.WriteLine("{0} 유저가 스킬 큐브를 옮겼습니다.", user._NickName);
            Console.WriteLine("스킬 큐브의 위치 : {0} // {1} // {2} // {3} // {4}", userskill[0], userskill[1], userskill[2], userskill[3], userskill[4]);

            if (!user._IsSkillEffect && user.AllSkillCubeCount() >= 5)
            {
                user.AddCardSlot();
                ShowSlot(room, user);
            }
        }

        void ShowUserItem(long uuid, int itemIndex, int itemCnt)
        {
            DefinedStructure.P_ShowShopInfo pShowShopInfo;
            pShowShopInfo._itemIndex = itemIndex;
            pShowShopInfo._itemCount = itemCnt;

            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowUserShopInfo, pShowShopInfo, uuid));
        }

        void ShowUserCoin(long uuid, string coinType, int coinValue)
        {
            DefinedStructure.P_ShowCoinInfo pShowCoinInfo;
            pShowCoinInfo._coinIndex = (int)(eCoinType)Enum.Parse(typeof(eCoinType), coinType);
            pShowCoinInfo._coinValue = coinValue;

            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.ShowCoinInfo, pShowCoinInfo, uuid));
        }

        void ShowSystemMessage(long uuid, string msgType)
        {
            DefinedStructure.P_SystemMessage pSystemMessage;
            pSystemMessage._systemMsgType = msgType;

            _toClientQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eToClient.SystemMessage, pSystemMessage, uuid));
        }

        void ReturnCard(RoomInfo room, UserInfo user, int completeCard)
        {
            switch (room._Rule)
            {
                case "Normal":

                    room._CardDeck.ReturnCard((eCardField)_tbMgr.Get(eTableType.CardData).ToI(completeCard, CardData.Index.Field.ToString()), completeCard);
                    RenewProjectBoard(room, completeCard);

                    break;
            }

            int slotIndex = user.DeleteCard(completeCard);
            DeleteCardSlot(room, user, slotIndex);

            user._IsApplyingEffect = false;
            user._ApplyCard = -1;
            user.CompleteCard((eCardField)_tbMgr.Get(eTableType.CardData).ToI(completeCard, CardData.Index.Field.ToString()));
        }

        void GetCardByEffect(RoomInfo room, UserInfo user)
        {
            if (!room._CardDeck._IsEmpty)
            {
                if (user.IsEmptyCardSlot())
                {
                    DefinedStructure.P_GetCard pGetCard;
                    pGetCard._index = user._Index;

                    SendBufferInRoom(room, DefinedProtocol.eToClient.GetCard, pGetCard);
                }
                else
                    ReturnCard(room, user, user._ApplyCard);
            }
            else
                ReturnCard(room, user, user._ApplyCard);
        }

        void SelectMyCard(RoomInfo room, UserInfo user)
        {
            DefinedStructure.P_SelectMyCard pSelectMyCard;
            pSelectMyCard._index = user._Index;
            pSelectMyCard._cardArr = new int[4];
            for (int n = 0; n < user._UnLockSlotCnt; n++)
                pSelectMyCard._cardArr[n] = user._MyPickCard[n]._CardIndex;

            SendBufferInRoom(room, DefinedProtocol.eToClient.SelectMyCard, pSelectMyCard);
        }

        void SelectOtherCard(RoomInfo room, UserInfo user)
        {
            DefinedStructure.P_SelectOtherCard pSelectOtherCard;
            pSelectOtherCard._index = user._Index;
            pSelectOtherCard._cardArr = new int[12];

            int temp = 0;
            for (int n = 0; n < room._UserArr.Length; n++)
            {
                if (room._UserArr[n] != null && !room._UserArr[n]._IsEmpty)
                {
                    if (room._UserArr[n]._Index != user._Index)
                    {
                        for (int m = 0; m < room._UserArr[n]._UnLockSlotCnt; m++)
                            pSelectOtherCard._cardArr[temp++] = room._UserArr[n]._MyPickCard[n]._CardIndex;
                    }
                }
            }

            SendBufferInRoom(room, DefinedProtocol.eToClient.SelectOtherCard, pSelectOtherCard);
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
