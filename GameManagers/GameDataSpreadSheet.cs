using UnityEngine;

public class GameDataSpreadSheetDummy : MonoBehaviour
{
    private const string _gameDataSpreadsheetID = "PublicValue";
    private const string _loginDataSpreadsheetID = "PublicValue";
    private const string _userAuthenticateDatasheetName = "PublicValue";

    public string GameDataSpreadsheetID => _gameDataSpreadsheetID;
    public string LoginDataSpreadsheetID => _loginDataSpreadsheetID;
    public string UserAuthenticateDatasheetName => _userAuthenticateDatasheetName;
}