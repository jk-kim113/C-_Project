using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server_CardBattle
{
    class SocketClass
    {
        Socket _mySocket;
        long _uniqueIdx;
        long _clientID;

        public Socket _MySocket { get { return _mySocket; } }
        public long _UUID { get { return _clientID; } }

        public SocketClass(Socket socket, long clientID)
        {
            _mySocket = socket;
            _clientID = clientID;
        }

        public void Connect(long uniqueIdx)
        {
            _uniqueIdx = uniqueIdx;
        }

        public void Close()
        {

        }

        public void SendBuffer(byte[] buffer)
        {
            _mySocket.Send(buffer);
        }

        public bool ReceiveBuffer(out byte[] buffer, out int recvLen)
        {
            buffer = new byte[1032];
            recvLen = 0;

            if (_mySocket.Poll(0, SelectMode.SelectRead))
            {   
                try
                {
                    recvLen = _mySocket.Receive(buffer);
                    if (recvLen > 0)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }

            return false;
        }
    }
}
