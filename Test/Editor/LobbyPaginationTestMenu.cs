using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using Util;

namespace Test.Editor
{
    public static class LobbyPaginationTestMenu
    {
        private const string TestLobbyNamePrefix = "TEST_ROOM_";
        private const string TestLobbyTagKey = "LobbyTestTag";
        private const string TestLobbyTagValue = "RoomPaginationTest";
        private const string CreatedLobbyIdsSessionKey = "LobbyPaginationTestMenu.CreatedLobbyIds";
        private const int TestRoomCount = 11;
        private const int TestRoomMaxPlayers = 2;
        private const double HeartbeatIntervalSeconds = 15d;
        private const int LobbyOperationDelayMs = 1_200;
        private const int RateLimitRetryDelayMs = 3_000;
        private const int RateLimitMaxRetryCount = 5;

        private static readonly List<string> CreatedLobbyIds = new List<string>();
        private static bool _isWorking;
        private static bool _isHeartbeatSending;
        private static double _nextHeartbeatTime;

        static LobbyPaginationTestMenu()
        {
            LoadCreatedLobbyIds();
            EditorApplication.update -= SendHeartbeatForCreatedRooms;
            EditorApplication.update += SendHeartbeatForCreatedRooms;
        }

        [MenuItem("Tools/Lobby Test/Create 11 Test Rooms")]
        private static async void CreateTestRooms()
        {
            if (CanRunLobbyTestMenu() == false || _isWorking)
                return;

            _isWorking = true;
            try
            {
                int startIndex = CreatedLobbyIds.Count;
                for (int i = 0; i < TestRoomCount; i++)
                {
                    int roomNumber = startIndex + i + 1;
                    Lobby lobby = await RunLobbyRequest(() =>
                        LobbyService.Instance.CreateLobbyAsync(
                            $"{TestLobbyNamePrefix}{roomNumber:000}",
                            TestRoomMaxPlayers,
                            CreateTestRoomOptions()));

                    CreatedLobbyIds.Add(lobby.Id);
                    await Task.Delay(LobbyOperationDelayMs);
                }

                SaveCreatedLobbyIds();
                _nextHeartbeatTime = 0d;
                EditorUtility.DisplayDialog(
                    "Lobby Test",
                    $"{TestRoomCount} test rooms created. Refresh the room board in Play Mode.",
                    "OK");
            }
            catch (Exception exception)
            {
                UtilDebug.LogError(exception);
                EditorUtility.DisplayDialog("Lobby Test Error", exception.Message, "OK");
            }
            finally
            {
                _isWorking = false;
            }
        }

        [MenuItem("Tools/Lobby Test/Delete Test Rooms")]
        private static async void DeleteTestRooms()
        {
            if (CanRunLobbyTestMenu() == false || _isWorking)
                return;

            _isWorking = true;
            try
            {
                int deletedCount = await DeleteTestRoomsInternal();
                CreatedLobbyIds.Clear();
                SaveCreatedLobbyIds();
                EditorUtility.DisplayDialog("Lobby Test", $"{deletedCount} test rooms deleted.", "OK");
            }
            catch (Exception exception)
            {
                UtilDebug.LogError(exception);
                EditorUtility.DisplayDialog("Lobby Test Error", exception.Message, "OK");
            }
            finally
            {
                _isWorking = false;
            }
        }

        [MenuItem("Tools/Lobby Test/Create 11 Test Rooms", true)]
        [MenuItem("Tools/Lobby Test/Delete Test Rooms", true)]
        private static bool ValidateLobbyTestMenu()
        {
            return EditorApplication.isPlaying;
        }

        private static CreateLobbyOptions CreateTestRoomOptions()
        {
            return new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "LobbyType",
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            "CharactorSelect",
                            DataObject.IndexOptions.S1)
                    },
                    {
                        TestLobbyTagKey,
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            TestLobbyTagValue,
                            DataObject.IndexOptions.S2)
                    }
                }
            };
        }

        private static async Task<int> DeleteTestRoomsInternal()
        {
            HashSet<string> deleteLobbyIds = new HashSet<string>(CreatedLobbyIds);
            int deletedCount = 0;
            foreach (string lobbyId in deleteLobbyIds)
            {
                try
                {
                    await RunLobbyRequest(() => LobbyService.Instance.DeleteLobbyAsync(lobbyId));
                    deletedCount++;
                    await Task.Delay(LobbyOperationDelayMs);
                }
                catch (LobbyServiceException exception) when (exception.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                }
            }

            return deletedCount;
        }

        private static bool CanRunLobbyTestMenu()
        {
            if (EditorApplication.isPlaying == false)
            {
                EditorUtility.DisplayDialog("Lobby Test", "Enter Play Mode and log in first.", "OK");
                return false;
            }

            if (UnityServices.State != ServicesInitializationState.Initialized ||
                AuthenticationService.Instance.IsSignedIn == false)
            {
                EditorUtility.DisplayDialog(
                    "Lobby Test",
                    "Unity Services is not initialized or the player is not signed in.",
                    "OK");
                return false;
            }

            return true;
        }

        private static async void SendHeartbeatForCreatedRooms()
        {
            if (EditorApplication.isPlaying == false ||
                CreatedLobbyIds.Count == 0 ||
                EditorApplication.timeSinceStartup < _nextHeartbeatTime ||
                _isHeartbeatSending)
            {
                return;
            }

            if (UnityServices.State != ServicesInitializationState.Initialized ||
                AuthenticationService.Instance.IsSignedIn == false)
            {
                return;
            }

            _isHeartbeatSending = true;
            try
            {
                for (int i = CreatedLobbyIds.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        await RunLobbyRequest(() => LobbyService.Instance.SendHeartbeatPingAsync(CreatedLobbyIds[i]));
                        await Task.Delay(LobbyOperationDelayMs);
                    }
                    catch (LobbyServiceException exception) when (exception.Reason == LobbyExceptionReason.LobbyNotFound)
                    {
                        CreatedLobbyIds.RemoveAt(i);
                    }
                }

                SaveCreatedLobbyIds();
            }
            finally
            {
                _nextHeartbeatTime = EditorApplication.timeSinceStartup + HeartbeatIntervalSeconds;
                _isHeartbeatSending = false;
            }
        }

        private static void LoadCreatedLobbyIds()
        {
            CreatedLobbyIds.Clear();
            string idsText = SessionState.GetString(CreatedLobbyIdsSessionKey, string.Empty);
            if (string.IsNullOrEmpty(idsText))
                return;

            CreatedLobbyIds.AddRange(idsText.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static void SaveCreatedLobbyIds()
        {
            SessionState.SetString(CreatedLobbyIdsSessionKey, string.Join("|", CreatedLobbyIds));
        }

        private static async Task<T> RunLobbyRequest<T>(Func<Task<T>> request)
        {
            for (int retryCount = 0; retryCount < RateLimitMaxRetryCount; retryCount++)
            {
                try
                {
                    return await request();
                }
                catch (LobbyServiceException exception) when (exception.Reason == LobbyExceptionReason.RateLimited)
                {
                    UtilDebug.LogWarning(
                        $"Lobby test request rate limited. Retry {retryCount + 1}/{RateLimitMaxRetryCount} after {RateLimitRetryDelayMs} ms.");
                    await Task.Delay(RateLimitRetryDelayMs);
                }
            }

            return await request();
        }

        private static async Task RunLobbyRequest(Func<Task> request)
        {
            for (int retryCount = 0; retryCount < RateLimitMaxRetryCount; retryCount++)
            {
                try
                {
                    await request();
                    return;
                }
                catch (LobbyServiceException exception) when (exception.Reason == LobbyExceptionReason.RateLimited)
                {
                    UtilDebug.LogWarning(
                        $"Lobby test request rate limited. Retry {retryCount + 1}/{RateLimitMaxRetryCount} after {RateLimitRetryDelayMs} ms.");
                    await Task.Delay(RateLimitRetryDelayMs);
                }
            }

            await request();
        }
    }
}
