using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DB_Scientia
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

        DB_Query _dbQuery;

        public MainDB()
        {
            CreateServer();
            _dbQuery = new DB_Query("127.0.0.1", "3306", "mydb", "root", "1234");
        }

        void CreateServer()
        {
            try
            {
                _waitServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _waitServer.Bind(new IPEndPoint(IPAddress.Any, _port));
                _waitServer.Listen(1);

                Console.WriteLine("DB 서버가 생성되었습니다.");
                Console.WriteLine("서버가 연결을 시도할 때 까지 대기 하십시오.");
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
                while (true)
                {
                    if (_fromServerQueue.Count != 0)
                    {
                        FromServerData fData = _fromServerQueue.Dequeue();

                        DefinedStructure.PacketInfo packet = new DefinedStructure.PacketInfo();
                        packet = (DefinedStructure.PacketInfo)ConvertPacket.ByteArrayToStructure(fData._data, packet.GetType(), fData._length);

                        switch ((DefinedProtocol.eFromServer)packet._id)
                        {
                            #region LogIn / Character
                            case DefinedProtocol.eFromServer.CheckLogIn:

                                DefinedStructure.P_Check_ID_Pw pCheckLogIn = new DefinedStructure.P_Check_ID_Pw();
                                pCheckLogIn = (DefinedStructure.P_Check_ID_Pw)ConvertPacket.ByteArrayToStructure(packet._data, pCheckLogIn.GetType(), packet._totalSize);

                                LogIn(pCheckLogIn._id, pCheckLogIn._pw, pCheckLogIn._index);

                                break;

                            case DefinedProtocol.eFromServer.OverlapCheck_ID:

                                DefinedStructure.P_CheckOverlap pCheckOverlap_ID = new DefinedStructure.P_CheckOverlap();
                                pCheckOverlap_ID = (DefinedStructure.P_CheckOverlap)ConvertPacket.ByteArrayToStructure(packet._data, pCheckOverlap_ID.GetType(), packet._totalSize);

                                OverlapCheck_ID(pCheckOverlap_ID._target, pCheckOverlap_ID._index);

                                break;

                            case DefinedProtocol.eFromServer.OverlapCheck_NickName:

                                DefinedStructure.P_CheckOverlap pCheckOverlap_NickName = new DefinedStructure.P_CheckOverlap();
                                pCheckOverlap_NickName = (DefinedStructure.P_CheckOverlap)ConvertPacket.ByteArrayToStructure(packet._data, pCheckOverlap_NickName.GetType(), packet._totalSize);

                                OverlapCheck_NickName(pCheckOverlap_NickName._target, pCheckOverlap_NickName._index);

                                break;

                            case DefinedProtocol.eFromServer.CheckEnroll:

                                DefinedStructure.P_Check_ID_Pw pCheckEnroll = new DefinedStructure.P_Check_ID_Pw();
                                pCheckEnroll = (DefinedStructure.P_Check_ID_Pw)ConvertPacket.ByteArrayToStructure(packet._data, pCheckEnroll.GetType(), packet._totalSize);

                                Enroll(pCheckEnroll._id, pCheckEnroll._pw, pCheckEnroll._index);

                                break;

                            case DefinedProtocol.eFromServer.CheckCharacterInfo:

                                DefinedStructure.P_CheckRequest pCheckCharacInfo = new DefinedStructure.P_CheckRequest();
                                pCheckCharacInfo = (DefinedStructure.P_CheckRequest)ConvertPacket.ByteArrayToStructure(packet._data, pCheckCharacInfo.GetType(), packet._totalSize);

                                CheckCharacInfo(pCheckCharacInfo._UUID);

                                break;

                            case DefinedProtocol.eFromServer.CreateCharacter:

                                DefinedStructure.P_CreateCharacterInfo pCreateCharac = new DefinedStructure.P_CreateCharacterInfo();
                                pCreateCharac = (DefinedStructure.P_CreateCharacterInfo)ConvertPacket.ByteArrayToStructure(packet._data, pCreateCharac.GetType(), packet._totalSize);

                                CreateCharacter(pCreateCharac._UUID, pCreateCharac._nickName, pCreateCharac._characterIndex, pCreateCharac._slot, pCreateCharac._startCardList);

                                break;
                            #endregion

                            #region Card
                            case DefinedProtocol.eFromServer.UserMyInfoData:

                                DefinedStructure.P_UserMyInfoData pUserMyInfoData = new DefinedStructure.P_UserMyInfoData();
                                pUserMyInfoData = (DefinedStructure.P_UserMyInfoData)ConvertPacket.ByteArrayToStructure(packet._data, pUserMyInfoData.GetType(), packet._totalSize);

                                UserMyInfoData(pUserMyInfoData._UUID, pUserMyInfoData._nickName);

                                break;

                            case DefinedProtocol.eFromServer.AddReleaseCard:

                                DefinedStructure.P_ReleaseCard pReleaseCard = new DefinedStructure.P_ReleaseCard();
                                pReleaseCard = (DefinedStructure.P_ReleaseCard)ConvertPacket.ByteArrayToStructure(packet._data, pReleaseCard.GetType(), packet._totalSize);

                                AddCardRelease(pReleaseCard._nickName, pReleaseCard._cardIndex);

                                break;
                            #endregion

                            case DefinedProtocol.eFromServer.GetBattleInfo:

                                DefinedStructure.P_GetBattleInfo pGetBattleInfo = new DefinedStructure.P_GetBattleInfo();
                                pGetBattleInfo = (DefinedStructure.P_GetBattleInfo)ConvertPacket.ByteArrayToStructure(packet._data, pGetBattleInfo.GetType(), packet._totalSize);

                                GetBattleInfo(pGetBattleInfo._roomNumber, pGetBattleInfo._UUID, pGetBattleInfo._nickName);

                                break;

                            case DefinedProtocol.eFromServer.GetAllCard:

                                DefinedStructure.P_GetAllCard pGetAllCard = new DefinedStructure.P_GetAllCard();
                                pGetAllCard = (DefinedStructure.P_GetAllCard)ConvertPacket.ByteArrayToStructure(packet._data, pGetAllCard.GetType(), packet._totalSize);

                                GetAllCard(pGetAllCard._roomNumber, pGetAllCard._nickNameArr);

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

        void ToServerQueue()
        {
            try
            {
                while (true)
                {
                    if (_toServerQueue.Count != 0)
                    {
                        byte[] buffer = _toServerQueue.Dequeue();

                        _conncetServer.Send(buffer);;
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

        void LogIn(string id, string pw, int index)
        {
            DefinedStructure.P_LogInResult pLogInResult;
            pLogInResult._index = index;

            if (_dbQuery.CheckLogIn(id, pw))
            {
                pLogInResult._UUID = _dbQuery.SearchUUID(id);
                pLogInResult._isSuccess = 0;
            }
            else
            {
                pLogInResult._UUID = 0;
                pLogInResult._isSuccess = 1;
            }

            ToPacket(DefinedProtocol.eToServer.LogInResult, pLogInResult);

            Console.WriteLine("{0} 유저가 로그인에 {1}했습니다.", pLogInResult._UUID, pLogInResult._isSuccess == 0 ? "성공" : "실패");
        }

        void OverlapCheck_ID(string id, int index)
        {
            DefinedStructure.P_CheckResult pOverlapResult;
            pOverlapResult._index = index;
            pOverlapResult._result = _dbQuery.SearchID(id) ? 0 : 1;

            ToPacket(DefinedProtocol.eToServer.OverlapResult_ID, pOverlapResult);

            Console.WriteLine("{0} <- 해당 아이디는 중복이 {1}니다.", id, pOverlapResult._result == 0 ? "맞습" : "아닙");
        }

        void OverlapCheck_NickName(string nickname, int index)
        {
            DefinedStructure.P_CheckResult pOverlapResult;
            pOverlapResult._index = index;
            pOverlapResult._result = _dbQuery.SearchNickName(nickname) ? 0 : 1;

            ToPacket(DefinedProtocol.eToServer.OverlapResult_NickName, pOverlapResult);

            Console.WriteLine("{0} <- 해당 닉네임은 중복이 {1}니다.", nickname, pOverlapResult._result == 0 ? "맞습" : "아닙");
        }

        void Enroll(string id, string pw, int index)
        {
            DefinedStructure.P_CheckResult pEnrollResult;
            pEnrollResult._index = index;

            if (_dbQuery.InsertUserInfo(id, pw))
            {   
                pEnrollResult._result = 0;
                Console.WriteLine("회원 등록이 완료되었습니다.");
            }
            else
            {
                pEnrollResult._result = 1;
                Console.WriteLine("회원 등록 과정에서 문제가 발견되었습니다.");
            }

            ToPacket(DefinedProtocol.eToServer.EnrollResult, pEnrollResult);
        }

        void CheckCharacInfo(long uuid)
        {
            List<CharacterInfo> characInfoList = new List<CharacterInfo>();
            _dbQuery.SearchCharacterInfo(uuid, characInfoList);

            for(int n = 0; n < characInfoList.Count; n++)
            {
                DefinedStructure.P_ShowCharacterInfo pCharacInfo;
                pCharacInfo._UUID = uuid;
                pCharacInfo._nickName = characInfoList[n]._nickName;
                pCharacInfo._chracIndex = characInfoList[n]._chracterIndex;
                pCharacInfo._accountLevel = characInfoList[n]._accountLevel;
                pCharacInfo._slotIndex = characInfoList[n]._slotIndex;

                ToPacket(DefinedProtocol.eToServer.ShowCharacterInfo, pCharacInfo);
            }

            DefinedStructure.P_CheckRequest pCompleteCharacterInfo;
            pCompleteCharacterInfo._UUID = uuid;

            ToPacket(DefinedProtocol.eToServer.CompleteCharacterInfo, pCompleteCharacterInfo);
        }

        void CreateCharacter(long uuid, string nickName, int characIndex, int slot, int[] startCardList)
        {
            DefinedStructure.P_Result pResult;
            pResult._UUID = uuid;

            if(_dbQuery.InsertCharacterInfo(uuid, nickName, characIndex, slot))
            {
                pResult._result = 0;
                Console.WriteLine("캐릭터 등록이 완료되었습니다.");

                for(int n = 0; n < startCardList.Length; n++)
                {
                    if (startCardList[n] == 0)
                        break;

                    _dbQuery.InsertCardReleaseInfo(nickName, startCardList[n]);
                }
            }
            else
            {
                pResult._result = 1;
                Console.WriteLine("캐릭터 등록 과정에서 문제가 발견되었습니다.");
            }

            ToPacket(DefinedProtocol.eToServer.CreateCharacterResult, pResult);
        }

        void UserMyInfoData(long uuid, string nickname)
        {
            DefinedStructure.P_CheckMyInfoData pCheckMyInfoData;
            pCheckMyInfoData._UUID = uuid;
            pCheckMyInfoData._characIndex = _dbQuery.SearchCharacterIndex(nickname);

            List<int> temp = new List<int>();

            _dbQuery.SearchLevelInfo(nickname, temp);
            pCheckMyInfoData._levelArr = new int[5];
            for (int n = 0; n < temp.Count; n++)
                pCheckMyInfoData._levelArr[n] = temp[n];
            temp.Clear();

            _dbQuery.SearchExpInfo(nickname, temp);
            pCheckMyInfoData._expArr = new int[5];
            for (int n = 0; n < temp.Count; n++)
                pCheckMyInfoData._expArr[n] = temp[n];
            temp.Clear();

            _dbQuery.SearchCardReleaseInfo(nickname, temp);
            pCheckMyInfoData._cardReleaseArr = new int[48];
            for (int n = 0; n < temp.Count; n++)
                pCheckMyInfoData._cardReleaseArr[n] = temp[n];
            temp.Clear();

            _dbQuery.SearchCardRentalInfo(nickname, temp);
            pCheckMyInfoData._cardRentalArr = new int[48];
            for (int n = 0; n < temp.Count; n++)
                pCheckMyInfoData._cardRentalArr[n] = temp[n];
            temp.Clear();

            List<float> temp2 = new List<float>();
            _dbQuery.SearchRentalTimeInfo(nickname, temp2);
            pCheckMyInfoData._rentalTimeArr = new float[48];
            for (int n = 0; n < temp.Count; n++)
                pCheckMyInfoData._rentalTimeArr[n] = temp2[n];
            temp2.Clear();

            _dbQuery.SearchMyDeckInfo(nickname, temp);
            pCheckMyInfoData._myDeckArr = new int[12];
            for (int n = 0; n < temp.Count; n++)
                pCheckMyInfoData._myDeckArr[n] = temp[n];
            temp.Clear();

            ToPacket(DefinedProtocol.eToServer.ShowMyInfoData, pCheckMyInfoData);
        }

        void AddCardRelease(string nickName, int cardIndex)
        {
            _dbQuery.InsertCardReleaseInfo(nickName, cardIndex);

            DefinedStructure.P_CheckRequest pCheckRequest;
            pCheckRequest._UUID = _dbQuery.SearchUUIDwithNickName(nickName);

            ToPacket(DefinedProtocol.eToServer.CompleteAddReleaseCard, pCheckRequest);
        }

        void GetBattleInfo(int roomNumber, long uuid, string nickName)
        {
            DefinedStructure.P_ShowBattleInfo pShowBattleInfo;
            pShowBattleInfo._roomNumber = roomNumber;
            pShowBattleInfo._UUID = uuid;
            pShowBattleInfo._nickName = nickName;
            pShowBattleInfo._accountlevel = _dbQuery.SearchAccountLevel(nickName);

            ToPacket(DefinedProtocol.eToServer.ShowBattleInfo, pShowBattleInfo);
        }

        void GetAllCard(int roomNum, string nickNameArr)
        {
            List<int> allcard = new List<int>();
            _dbQuery.SearchAllCard(nickNameArr, allcard);

            DefinedStructure.P_ShowAllCard pShowAllCard;
            pShowAllCard._roomNum = roomNum;
            pShowAllCard._cardCount = allcard.Count;
            pShowAllCard._cardArr = new int[48];
            for (int n = 0; n < allcard.Count; n++)
                pShowAllCard._cardArr[n] = allcard[n];

            ToPacket(DefinedProtocol.eToServer.ShowAllCard, pShowAllCard);
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
