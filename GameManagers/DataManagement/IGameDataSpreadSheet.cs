using System;

namespace GameManagers.DataManagement
{
    public interface IGameDataSpreadSheet
    {
        public string GameDataSpreadsheetID { get; }
        public bool TryGetPublishedCsvUrl(Type dataType, out string csvUrl);
    }
}
