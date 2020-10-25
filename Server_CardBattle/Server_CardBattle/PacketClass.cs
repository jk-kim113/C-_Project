using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Server_CardBattle
{
    class PacketClass
    {
        int _protocolID;
        int _castIdentifier = -999; // 소켓의 인덱스
        long _uniqueUserIndex; // 유저의 UUID
        int _dataSize;
        byte[] _data;

        public int _ProtocolID { get { return _protocolID; } }
        public int _CastIdendifier { get { return _castIdentifier; } }
        public long _UUID { get { return _uniqueUserIndex; } }
        public byte[] _Data { get { return _data; } }
        

        public PacketClass()
        {

        }

        public PacketClass(int protocolID, byte[] data, int dataSize, int castIdentifier, long uuid)
        {
            _protocolID = protocolID;
            _data = data;
            _dataSize = dataSize;
            _castIdentifier = castIdentifier;
            _uniqueUserIndex = uuid;
        }

        public PacketClass(int protocolID, byte[] data, int dataSize, int castIdentifier)
        {
            _protocolID = protocolID;
            _data = data;
            _dataSize = dataSize;
            _castIdentifier = castIdentifier;
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

            //DefinedStructure.PacketInfo packet = ConvertPacket.CreatePack((int)toClientID, Marshal.SizeOf(str), ConvertPacket.StructureToByteArray(str));

            MakePacket((int)toClientID, str);
        }

        public void CreatePacket(DefinedProtocol.eToClient toClientID, object str, int castIdentifier)
        {
            _castIdentifier = castIdentifier;

            MakePacket((int)toClientID, str);
        }

        public void CreatePacket(DefinedProtocol.eFromServer fromServerID, object str)
        {
            MakePacket((int)fromServerID, str);
        }

        void MakePacket(int packetID, object str)
        {
            DefinedStructure.PacketInfo packet;
            packet._id = packetID;
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
