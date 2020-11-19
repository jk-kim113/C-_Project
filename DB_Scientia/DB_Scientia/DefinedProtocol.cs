using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Scientia
{
    class DefinedProtocol
    {
        public enum eFromServer
        {
            CheckLogIn,
            OverlapCheck_ID,
            OverlapCheck_NickName,
            CheckEnroll,
            CheckCharacterInfo,
            CreateCharacter,
            UserMyInfoData,
            AddReleaseCard,
            GetBattleInfo,

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
            ShowMyInfoData,
            CompleteAddReleaseCard,
            ShowBattleInfo,

            max
        }
    }
}
