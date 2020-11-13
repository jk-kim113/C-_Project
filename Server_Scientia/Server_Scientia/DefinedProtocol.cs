﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Scientia
{
    class DefinedProtocol
    {
        public enum eFromClient
        {
            LogInTry,
            OverlapCheck_ID,
            EnrollTry,
            GetMyCharacterInfo,

            max
        }

        public enum eToClient
        {
            LogInResult,
            ResultOverlap_ID,
            EnrollResult,
            CharacterInfo,
            EndCharacterInfo,

            max
        }

        public enum eFromServer
        {
            CheckLogIn,
            OverlapCheck_ID,
            CheckEnroll,
            CheckCharacterInfo,

            max
        }

        public enum eToServer
        {
            LogInResult,
            OverlapResult_ID,
            EnrollResult,
            ShowCharacterInfo,
            CompleteCharacterInfo,

            max
        }
    }
}