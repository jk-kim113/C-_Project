using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class PacketClass
    {
        int _protocolID;
        int _castIdentifier = -999; // 소켓의 인덱스
        long _uniqueUserIndex; // 유저의 UUID
        int _dataSize;
        byte[] _data;

        public PacketClass(int protocolID, byte[] data, int dataSize, int castIdentifier, long uuid)
        {
            _protocolID = protocolID;
            _data = data;
            _dataSize = dataSize;
            _castIdentifier = castIdentifier;
            _uniqueUserIndex = uuid;
        }

    }
}
