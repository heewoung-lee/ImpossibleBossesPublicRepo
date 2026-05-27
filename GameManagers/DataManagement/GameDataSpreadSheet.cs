using System;
using System.Collections.Generic;

namespace GameManagers.DataManagement
{
    public class GameDataSpreadSheet : IGameDataSpreadSheet,ILoginDataSpreadSheet
    {
        private const string _gameDataSpreadsheetID = "1t5eJgAEduoRUPpf59HsxzKnSdNMJzIWJCJuImCMKv_U";
        private const string _loginDataSpreadsheetID = "154GC5cBgKZ1SEcXZql5xaHnXXtgTpyLRNETRqLyW4FA";
        private const string _userAuthenticateDatasheetName  = "UserAuthenticateData";
        private const string _publishedSpreadsheetDocumentId = "2PACX-1vQgPxR_nec4J_MgfdgaD6rjsbp_Utvjp-4qThbMcNLc15cnwO4ytlm3yx7rxCBAavqy_xas0YQ9h_8r";

        private readonly Dictionary<string, string> _publishedSheetGids = new Dictionary<string, string>()
        {
            { "FighterStat", "0" },
            { "MageStat", "1621990871" },
            { "ArcherStat", "914622083" },
            { "NecromancerStat", "926255946" },
            { "MonkStat", "193548036" },
            { "MonsterStat", "315201274" },
            { "BossStat", "574931508" }
        };
        
        public string GameDataSpreadsheetID => _gameDataSpreadsheetID;
        public string LoginDataSpreadsheetID => _loginDataSpreadsheetID;
        public string UserAuthenticateDatasheetName => _userAuthenticateDatasheetName;

        public bool TryGetPublishedCsvUrl(Type dataType, out string csvUrl)
        {
            csvUrl = null;
            if (dataType == null)
            {
                return false;
            }

            if (_publishedSheetGids.TryGetValue(dataType.Name, out string gid) == false)
            {
                return false;
            }

            csvUrl = $"https://docs.google.com/spreadsheets/d/e/{_publishedSpreadsheetDocumentId}/pub?gid={gid}&single=true&output=csv";
            return true;
        }
    }
}
