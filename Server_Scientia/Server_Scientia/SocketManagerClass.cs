using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Server_Scientia
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
            if (_socketList.Count != 0)
            {
                for (int n = 0; n < _socketList.Count; n++)
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
    }
}
