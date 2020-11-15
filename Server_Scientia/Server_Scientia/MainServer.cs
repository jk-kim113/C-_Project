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
    class MainServer
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

        Thread _tAccept;
        Thread _tFromClient;
        Thread _tToClient;
        Thread _tFromServer;
        Thread _tToServer;

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

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.CreateCharacter, pCreateCharacterInfo));

                                    break;
                                #endregion

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
            //if (_tFromServer.IsAlive)
            //{
            //    _tFromServer.Interrupt();
            //    _tFromServer.Join();
            //}
            //if (_tToServer.IsAlive)
            //{
            //    _tToServer.Interrupt();
            //    _tToServer.Join();
            //}
        }
    }
}
