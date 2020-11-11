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

        Thread _tAccept;

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

        public void MainLoop()
        {
            _tAccept = new Thread(new ThreadStart(Accept));
            
            if (!_tAccept.IsAlive)
                _tAccept.Start();
        }

        public void ExitProgram()
        {
            if (_tAccept.IsAlive)
            {
                _tAccept.Interrupt();
                _tAccept.Join();
            }
        }
    }
}
