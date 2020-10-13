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
        Dictionary<int, ServerInfo.RoomInfo> _roomInfoDic = new Dictionary<int, ServerInfo.RoomInfo>();

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
            while(true)
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

                                _socketManager.AddToQueue(_toClientQueue, DefinedProtocol.eToClient.CheckConnect, pCheckConnect, packet._UUID);

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

                                        _socketManager.AddToQueue(_toClientQueue, DefinedProtocol.eToClient.ShowRoomInfo, pShowRoomInfo, packet._UUID);
                                    }
                                }

                                logMessage = string.Format("{0} 유저가 입장\t\t{1}", pConnect._name, DateTime.Now);
                                break;
                            #endregion
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        void ToClientQueue()
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
        }
    }
}
