using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LearnTCPServer
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
        public struct Packet_Login
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_LoginAfter
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string _name;
            [MarshalAs(UnmanagedType.I4)]
            public int _avatarIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_Chat
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _chat;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Packet_CurrentUser
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.I4)]
            public int _avatarIndex;
        }
    }
}
