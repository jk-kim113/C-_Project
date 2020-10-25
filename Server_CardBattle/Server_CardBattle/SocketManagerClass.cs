using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_CardBattle
{
    class SocketManagerClass
    {
        List<SocketClass> _socketList = new List<SocketClass>();

        public void AddSocket(SocketClass socket)
        {
            _socketList.Add(socket);
            socket.ConnectSocket(_socketList.Count - 1);
        }

        public void SocketComplete(long uuid, int index)
        {
            _socketList[index].ConnectCompletely(uuid);
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

                        PacketClass packet = new PacketClass(pInfo._id, pInfo._data, pInfo._totalSize, n, _socketList[n]._UUID);
                        fromClient.Enqueue(packet);
                    }
                }
            }
        }

        public PacketClass AddToQueue(DefinedProtocol.eToClient toClientID, object str, long uuid)
        {
            PacketClass packet = new PacketClass();
            packet.CreatePacket(toClientID, str, uuid);

            return packet;
        }

        public PacketClass AddToQueue(DefinedProtocol.eToClient toClientID, object str, int castIdentifier)
        {
            PacketClass packet = new PacketClass();
            packet.CreatePacket(toClientID, str, castIdentifier);

            return packet;
        }

        public PacketClass AddToQueue(DefinedProtocol.eFromServer fromServerID, object str)
        {
            PacketClass packet = new PacketClass();
            packet.CreatePacket(fromServerID, str);

            return packet;
        }

        public void Send(byte[] buffer, long uuid)
        {
            SocketClass socket = SearchByUUID(uuid);
            if (socket != null)
                socket.SendBuffer(buffer);
        }

        public void Send(byte[] buffer, int index)
        {   
            if (_socketList[index] != null)
                _socketList[index].SendBuffer(buffer);
        }

        public void SendAll(byte[] buffer)
        {
            for(int n = 0; n < _socketList.Count; n++)
                if(_socketList[n]._MySocket != null)
                    _socketList[n].SendBuffer(buffer);
        }

        public SocketClass SearchByUUID(long uuid)
        {
            for(int n = 0; n < _socketList.Count; n++)
                if (_socketList[n]._UUID == uuid)
                    return _socketList[n];

            return null;
        }

        public void CloseAllSocket()
        {
            for(int n = 0; n < _socketList.Count; n++)
            {
                if(_socketList[n]._MySocket != null)
                    _socketList[n].CloseSocket();
            }

            _socketList.Clear();
        }

        public void CloseSocket(long uuid)
        {
            SocketClass socket = SearchByUUID(uuid);
            if (socket != null)
                socket.CloseSocket();
        }
    }
}
