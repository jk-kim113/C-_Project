using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Server_Scientia
{
    class ConvertPacket
    {
        public static byte[] StructureToByteArray(object obj)
        {
            int datasize = Marshal.SizeOf(obj);
            IntPtr buff = Marshal.AllocHGlobal(datasize);
            Marshal.StructureToPtr(obj, buff, false);
            byte[] data = new byte[datasize];
            Marshal.Copy(buff, data, 0, datasize);
            Marshal.FreeHGlobal(buff);
            return data;
        }

        public static object ByteArrayToStructure(byte[] data, Type type, int size)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, buff, data.Length);
            object obj = Marshal.PtrToStructure(buff, type);
            Marshal.FreeHGlobal(buff);

            if (Marshal.SizeOf(obj) != size)
                return null;

            return obj;
        }

        public static DefinedStructure.PacketInfo CreatePack(int id, int size, byte[] data)
        {
            DefinedStructure.PacketInfo receivePack = new DefinedStructure.PacketInfo();
            receivePack._id = id;
            receivePack._totalSize = size;
            receivePack._data = new byte[1016];
            Array.Copy(data, receivePack._data, data.Length);

            return receivePack;
        }
    }
}
