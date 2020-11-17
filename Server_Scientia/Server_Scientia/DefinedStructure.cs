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
        public struct P_Send_ID_Pw
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _pw;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_OverlapCheck
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _target;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_Request
        {
            
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_CreateCharacter
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
            [MarshalAs(UnmanagedType.I4)]
            public int _characterIndex;
            [MarshalAs(UnmanagedType.I4)]
            public int _slot;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_MyCardReleaseInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_ReleaseCard
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
            [MarshalAs(UnmanagedType.I4)]
            public int _cardIndex;
        }
        #endregion

        #region ToClient
        [StructLayout(LayoutKind.Sequential)]
        public struct P_ResultLogIn
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _isSuccess;
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_ResultCheck
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_CharacterInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
            [MarshalAs(UnmanagedType.I4)]
            public int _chracIndex;
            [MarshalAs(UnmanagedType.I4)]
            public int _accountLevel;
            [MarshalAs(UnmanagedType.I4)]
            public int _slotIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_ShowCardReleaseInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public int[] _cardIndexList;
        }
        #endregion

        #region FromServer
        [StructLayout(LayoutKind.Sequential)]
        public struct P_Check_ID_Pw
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _pw;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_CheckOverlap
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _target;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_CheckRequest
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_CreateCharacterInfo
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
            [MarshalAs(UnmanagedType.I4)]
            public int _characterIndex;
            [MarshalAs(UnmanagedType.I4)]
            public int _slot;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public int[] _startCardList;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_UserCardReleaseInfo
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
        }
        #endregion

        #region ToServer
        [StructLayout(LayoutKind.Sequential)]
        public struct P_LogInResult
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _isSuccess;
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_CheckResult
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _result;
            [MarshalAs(UnmanagedType.I4)]
            public int _index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_ShowCharacterInfo
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
            [MarshalAs(UnmanagedType.I4)]
            public int _chracIndex;
            [MarshalAs(UnmanagedType.I4)]
            public int _accountLevel;
            [MarshalAs(UnmanagedType.I4)]
            public int _slotIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_Result
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.I4)]
            public int _result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_CheckCardReleaseInfo
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public int[] _cardIndexList;
        }
        #endregion
    }
}
