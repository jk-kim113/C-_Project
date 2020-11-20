using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DB_Scientia
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
        public struct P_CheckRequest
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
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
        public struct P_Result
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.I4)]
            public int _result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_UserMyInfoData
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_CheckMyInfoData
        {
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.I4)]
            public int _characIndex;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] _levelArr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] _expArr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public int[] _cardReleaseArr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public int[] _cardRentalArr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public float[] _rentalTimeArr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public int[] _myDeckArr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_ReleaseCard
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
            [MarshalAs(UnmanagedType.I4)]
            public int _cardIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_GetBattleInfo
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _roomNumber;
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_ShowBattleInfo
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _roomNumber;
            [MarshalAs(UnmanagedType.I8)]
            public long _UUID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickName;
            [MarshalAs(UnmanagedType.I4)]
            public int _accountlevel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_GetAllCard
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _roomNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string _nickNameArr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct P_ShowAllCard
        {
            [MarshalAs(UnmanagedType.I4)]
            public int _roomNum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public int[] _cardArr;
            [MarshalAs(UnmanagedType.I4)]
            public int _cardCount;
        }
    }
}
