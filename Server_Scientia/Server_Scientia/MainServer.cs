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

        SocketManagerClass _socketManager = new SocketManagerClass();

        Queue<PacketClass> _fromClientQueue = new Queue<PacketClass>();
        Queue<PacketClass> _toClientQueue = new Queue<PacketClass>();

        Queue<PacketClass> _fromServerQueue = new Queue<PacketClass>();

        Thread _tAccept;
        Thread _tFromClient;
        Thread _tToClient;
        //Thread _tFromServer;
        //Thread _tToServer;

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
                                case DefinedProtocol.eFromClient.LogInTry:

                                    DefinedStructure.P_LogInTry pLogInTry = new DefinedStructure.P_LogInTry();
                                    pLogInTry = (DefinedStructure.P_LogInTry)packet.Convert(pLogInTry.GetType());

                                    DefinedStructure.P_CheckLogIn pCheckLogIn;
                                    pCheckLogIn._id = pLogInTry._id;
                                    pCheckLogIn._pw = pLogInTry._pw;
                                    pCheckLogIn._index = packet._CastIdendifier;

                                    _fromServerQueue.Enqueue(_socketManager.AddToQueue(DefinedProtocol.eFromServer.CheckLogIn, pCheckLogIn));

                                    Console.WriteLine("ID가 {0}인 유저가 접속을 시도 하고 있습니다.", pLogInTry._id);

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
