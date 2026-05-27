using Cysharp.Threading.Tasks;

namespace GameManagers.LoginManagement
{
    public static class LoginErrorCode
    {
        public const string MissingCredential = "MISSING_CREDENTIAL";
        public const string InvalidCredential = "INVALID_CREDENTIAL";
        public const string MissingNickname = "MISSING_NICKNAME";
        public const string NicknameAlreadyExists = "NICKNAME_ALREADY_EXISTS";
        public const string MissingLoginContext = "MISSING_LOGIN_CONTEXT";
        public const string SteamUnavailable = "STEAM_UNAVAILABLE";
        public const string NetworkError = "NETWORK_ERROR";
        public const string InvalidSteamTicket = "INVALID_STEAM_TICKET";
        public const string SteamAuthFailed = "STEAM_AUTH_FAILED";
        public const string SteamApiError = "STEAM_API_ERROR";
        public const string ServerError = "SERVER_ERROR";
    }

    public readonly struct LoginResult
    {
        public readonly bool Success;
        public readonly string ErrorCode;
        public readonly string PlayerKey;
        public readonly string NickName;

        public bool NeedsNickName => Success && string.IsNullOrEmpty(NickName);

        private LoginResult(bool success, string errorCode, string playerKey, string nickName)
        {
            Success = success;
            ErrorCode = errorCode;
            PlayerKey = playerKey;
            NickName = nickName;
        }

        public static LoginResult Succeed(string playerKey, string nickName)
        {
            return new LoginResult(true, string.Empty, playerKey, nickName);
        }

        public static LoginResult Fail(string errorCode)
        {
            return new LoginResult(false, errorCode, string.Empty, string.Empty);
        }
    }

    public interface ILoginService
    {
        string PlayerNickName { get; }
        UniTask<LoginResult> LoginAsync();
        UniTask<LoginResult> SaveNickNameAsync(string nickName);
    }
}
