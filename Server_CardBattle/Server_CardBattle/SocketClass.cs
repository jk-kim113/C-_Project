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
        long _uniqueIdx; // 소켓의 인덱스
        long _clientID; // 클라이언트의 UUID

        public Socket _MySocket { get { return _mySocket; } }
        public long _UUID { get { return _clientID; } }

        public SocketClass(Socket socket)
        {
            _mySocket = socket;
        }

        public void ConnectSocket(long uniqueIdx)
        {
            _uniqueIdx = uniqueIdx;
        }

        public void ConnectCompletely(long clientID)
        {
            _clientID = clientID;
        }

        public void CloseSocket()
        {
            _mySocket.Shutdown(SocketShutdown.Both);
            _mySocket.Close();
            _mySocket = null;
        }

        public void SendBuffer(byte[] buffer)
        {
            if(_mySocket != null)
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
