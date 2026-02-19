using GameManagers.Interface.DataManager;

namespace GameManagers
{
    public class GameDataSpreadSheet : IGameDataSpreadSheet,ILoginDataSpreadSheet
    {
        private const string _gameDataSpreadsheetID = "1t5eJgAEduoRUPpf59HsxzKnSdNMJzIWJCJuImCMKv_U";
        private const string _loginDataSpreadsheetID = "154GC5cBgKZ1SEcXZql5xaHnXXtgTpyLRNETRqLyW4FA";
        private const string _userAuthenticateDatasheetName  = "UserAuthenticateData";
        
        public string GameDataSpreadsheetID => _gameDataSpreadsheetID;
        public string LoginDataSpreadsheetID => _loginDataSpreadsheetID;
        public string UserAuthenticateDatasheetName => _userAuthenticateDatasheetName;

    }
}