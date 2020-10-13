using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server_CardBattle
{
    class SocketManagerClass
    {
        List<SocketClass> _socketList = new List<SocketClass>();

        public void AddSocket(SocketClass socket)
        {
            _socketList.Add(socket);
        }

        public void AddFromQueue(Queue<PacketClass> fromClient)
        {
            if(_socketList.Count != 0)
            {
                for(int n = 0; n < _socketList.Count; n++)
                {
                    byte[] buffer;
                    int recvLen;
                    if (_socketList[n].ReceiveBuffer(out buffer, out recvLen))
                    {
                        DefinedStructure.PacketInfo pInfo = new DefinedStructure.PacketInfo();
                        pInfo = (DefinedStructure.PacketInfo)ConvertPacket.ByteArrayToStructure(buffer, pInfo.GetType(), recvLen);

                        PacketClass packet = new PacketClass(pInfo._id, pInfo._data, pInfo._totalSize);
                        fromClient.Enqueue(packet);
                    }
                }
            }
        }

        public void AddToQueue(Queue<PacketClass> toClient, DefinedProtocol.eToClient toClientID, object str, long uuid)
        {
            PacketClass packet = new PacketClass();
            packet.CreatePacket(toClientID, str, uuid);

            toClient.Enqueue(packet);
        }

        public void Send(byte[] buffer, long uuid)
        {
            SocketClass socket = SearchByUUID(uuid);
            if (socket != null)
                socket.SendBuffer(buffer);
        }

        public void SendAll(byte[] buffer)
        {
            for(int n = 0; n < _socketList.Count; n++)
                _socketList[n].SendBuffer(buffer);
        }

        public SocketClass SearchByUUID(long uuid)
        {
            for(int n = 0; n < _socketList.Count; n++)
                if (_socketList[n]._UUID == uuid)
                    return _socketList[n];

            return null;
        }
    }
}
