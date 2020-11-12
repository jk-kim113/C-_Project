using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Server_Scientia
{
    class DefinedStructure
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PacketInfo
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _id;
            [MarshalAs(UnmanagedType.I4)]
            public int _totalSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public byte[] _data;
        }

        #region FromClient
        [StructLayout(LayoutKind.Sequential)]
        public struct P_LogInTry
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _pw;
        }
        #endregion

        #region FromServer
        [StructLayout(LayoutKind.Sequential)]
        public struct P_CheckLogIn
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _pw;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }
        #endregion
    }
}
