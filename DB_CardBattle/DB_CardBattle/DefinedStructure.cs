using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DB_CardBattle
{
    class DefinedStructure
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PacketInfo                                        // 전체 사이즈 1032byte
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _id;
            [MarshalAs(UnmanagedType.I4)]
            public int _totalSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public byte[] _data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_OverlapCheckID
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _id;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_OverlapCheckResultID
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _result;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_JoinGame
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _pw;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_CompleteJoin
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_LogIn
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _pw;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_LogInResult
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string _name;
            [MarshalAs(UnmanagedType.I4)]
            public int _avatarIndex;
            [MarshalAs(UnmanagedType.I4)]
            public int _isSuccess;
            [MarshalAs(UnmanagedType.I4)]
            public int _isFirst;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_MyInfo
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string _name;
            [MarshalAs(UnmanagedType.I4)]
            public int _avatarIndex;
        }
    }
}
