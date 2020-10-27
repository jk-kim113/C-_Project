using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public enum eTableUserInfo
        {
            All,

            UUID,
            ID,
            PW,
            NickName,
            AvatarIndex,

            None
        }

        public enum eTableGameInfo
        {
            All,

            UUID,
            ClearStage,
            MinClearTime,
            TotalPlayCount,

            None
        }

        public enum eTableTotalResult
        {
            All,

            Index,
            UUID,
            IsWin,

            None
        }

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
            _dbQuery = new DB_Query("127.0.0.1", "3306", "cardbattle", "root", "1234");
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

                                _dbQuery.UpdateUserInfo(pMyInfo._UUID, pMyInfo._name, pMyInfo._avatarIndex);

                                break;

                            case DefinedProtocol.eFromServer.ExitServer:

                                _conncetServer.Shutdown(SocketShutdown.Both);
                                _conncetServer.Close();
                                _conncetServer = null;

                                break;

                            case DefinedProtocol.eFromServer.SaveResult:

                                DefinedStructure.Packet_SaveResult pSaveResult = new DefinedStructure.Packet_SaveResult();
                                pSaveResult = (DefinedStructure.Packet_SaveResult)ConvertPacket.ByteArrayToStructure(packet._data, pSaveResult.GetType(), packet._totalSize);

                                SaveGameInfo(pSaveResult._UUID, pSaveResult._clearTime, pSaveResult._isWin);

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
            DefinedStructure.Packet_OverlapCheckResultID pOverlapResult;
            pOverlapResult._index = index;

            if (_dbQuery.SearchID(id))
                pOverlapResult._result = 0;
            else
                pOverlapResult._result = 1;

            ToPacket(DefinedProtocol.eToServer.OverlapCheckResult_ID, pOverlapResult);

            Console.WriteLine("{0} <- 해당 아이디는 중복이 {1}니다.", id, pOverlapResult._result == 0 ? "맞습" : "아닙");
        }

        void JoinGame(string id, string pw, int index)
        {
            if (_dbQuery.InsertUserInfo(id, pw))
            {
                DefinedStructure.Packet_CompleteJoin pCompleteJoin;
                pCompleteJoin._UUID = _dbQuery.SearchUUID(id);
                pCompleteJoin._index = index;

                _dbQuery.InsertGameInfo(pCompleteJoin._UUID);

                ToPacket(DefinedProtocol.eToServer.CompleteJoin, pCompleteJoin);

                Console.WriteLine("회원 등록이 완료되었습니다.");
            }

            Console.WriteLine("회원 등록 과정에서 문제가 발견되었습니다.");
        }

        void LogIn(string id, string pw, int index)
        {
            DefinedStructure.Packet_LogInResult pLogInResult;
            pLogInResult._index = index;

            if (_dbQuery.SearchLogIn(id, pw))
            {
                pLogInResult._UUID = _dbQuery.SearchUUID(id);
                pLogInResult._name = _dbQuery.SearchNickName(pLogInResult._UUID);
                pLogInResult._avatarIndex = _dbQuery.SearchAvatarIndex(pLogInResult._UUID);
                pLogInResult._isSuccess = 0;

                if (_dbQuery.SearchIsFirstLogIn(id))
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

            Console.WriteLine("로그인에 {0}했습니다.", pLogInResult._isSuccess == 0 ? "성공" : "실패");
        }

        void SaveGameInfo(long uuid, int cleartime, int isWin)
        {
            if (_dbQuery.SearchMinClearTime(uuid) > cleartime)
                _dbQuery.UpdateClearTime(uuid, cleartime);

            _dbQuery.UpdateTotalPlayCount(uuid);

            _dbQuery.InsertTotalResult(uuid, isWin);

            Console.WriteLine("{0} 유저의 게임결과를 저장했습니다.", uuid);
        }
        
        void ToPacket(DefinedProtocol.eToServer toServer, object str)
        {
            DefinedStructure.PacketInfo packet;
            packet._id = (int)toServer;
            packet._data = new byte[1024];

            if (str != null)
            {
                byte[] temp = ConvertPacket.StructureToByteArray(str);
                for (int n = 0; n < temp.Length; n++)
                    packet._data[n] = temp[n];
                packet._totalSize = temp.Length;
            }
            else
            {
                packet._totalSize = packet._data.Length;
            }

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
