using System;
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
            OverlapCheck_NickName,
            EnrollTry,
            GetMyCharacterInfo,
            CreateCharacter,
            MyCardReleaseInfo,
            AddReleaseCard,

            ConnectionTerminate,

            max
        }

        public enum eToClient
        {
            LogInResult,
            ResultOverlap_ID,
            ResultOverlap_NickName,
            EnrollResult,
            CharacterInfo,
            EndCharacterInfo,
            EndCreateCharacter,
            ShowCardReleaseInfo,
            CompleteAddReleaseCard,

            max
        }

        public enum eFromServer
        {
            CheckLogIn,
            OverlapCheck_ID,
            OverlapCheck_NickName,
            CheckEnroll,
            CheckCharacterInfo,
            CreateCharacter,
            UserCardReleaseInfo,
            AddReleaseCard,

            max
        }

        public enum eToServer
        {
            LogInResult,
            OverlapResult_ID,
            OverlapResult_NickName,
            EnrollResult,
            ShowCharacterInfo,
            CompleteCharacterInfo,
            CreateCharacterResult,
            ShowCardReleaseInfo,
            CompleteAddReleaseCard,

            max
        }
    }
}
