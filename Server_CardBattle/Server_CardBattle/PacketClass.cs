using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_CardBattle
{
    class PacketClass
    {
        int _protocolID;
        int _castIdentifier;
        long _uniqueUserIndex;
        int _dataSize;
        byte[] _data;

        public int _ProtocolID { get { return _protocolID; } }
        public long _UUID { get { return _uniqueUserIndex; } }
        public byte[] _Data { get { return _data; } }
        

        public PacketClass()
        {

        }

        public PacketClass(int protocolID, byte[] data, int dataSize)
        {
            _protocolID = protocolID;
            _data = data;
            _dataSize = dataSize;
        }

        public void CreatePacket(DefinedProtocol.eToClient toClientID, object str, long uuid)
        {
            _uniqueUserIndex = uuid;

            DefinedStructure.PacketInfo packet;
            packet._id = (int)toClientID;
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

            _data = ConvertPacket.StructureToByteArray(packet);
            _dataSize = _data.Length;
        }

        public object Convert(Type type)
        {
            return ConvertPacket.ByteArrayToStructure(_data, type, _dataSize);
        }
    }
}
