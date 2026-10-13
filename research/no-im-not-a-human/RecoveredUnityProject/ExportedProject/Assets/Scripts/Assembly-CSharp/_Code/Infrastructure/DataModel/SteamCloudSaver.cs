using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Steamworks;

namespace _Code.Infrastructure.DataModel
{
	public static class SteamCloudSaver
	{
		private const int LoadTimeoutSeconds = 5;

		private static bool _cloudUnavailableForSession;

		private static string _cloudUnavailableError;

		public static bool LastLoadFailed { get; private set; }

		public static string LastLoadError { get; private set; }

		public static async UniTaskVoid Init()
		{
			if (await WaitForSteamInitialization() && SteamUser.BLoggedOn())
			{
				int fileCount = SteamRemoteStorage.GetFileCount();
				for (int i = 0; i < fileCount; i++)
				{
					SteamRemoteStorage.GetFileNameAndSize(i, out var _);
				}
			}
		}

		public static async UniTask SaveToSteamCloud(string fileName, string content)
		{
			if (SteamManager.Initialized && SteamUser.BLoggedOn())
			{
				SteamRemoteStorage.BeginFileWriteBatch();
				byte[] bytes = Encoding.UTF8.GetBytes(content);
				if (SteamRemoteStorage.FileWrite(fileName + ".sav", bytes, bytes.Length))
				{
					SteamRemoteStorage.SetSyncPlatforms(fileName + ".sav", ERemoteStoragePlatform.k_ERemoteStoragePlatformAll);
					SteamRemoteStorage.FileShare(fileName + ".sav");
				}
				SteamRemoteStorage.EndFileWriteBatch();
			}
		}

		public static async UniTask<string> LoadFromSteamCloud(string fileName)
		{
			ResetLastLoadResult();
			if (_cloudUnavailableForSession)
			{
				MarkLoadFailed(_cloudUnavailableError);
				return null;
			}
			if (!(await WaitForSteamInitialization()))
			{
				MarkCloudUnavailable(string.IsNullOrEmpty(SteamManager.LastError) ? $"Steam was not initialized within {5} seconds." : SteamManager.LastError);
				return null;
			}
			if (!SteamUser.BLoggedOn())
			{
				MarkCloudUnavailable("Steam user is not logged on.");
				return null;
			}
			if (!SteamRemoteStorage.IsCloudEnabledForApp())
			{
				MarkCloudUnavailable("Steam Cloud is disabled for this app.");
				return null;
			}
			if (!SteamRemoteStorage.IsCloudEnabledForAccount())
			{
				MarkCloudUnavailable("Steam Cloud is disabled for this account.");
				return null;
			}
			string text = fileName + ".sav";
			if (!SteamRemoteStorage.FileExists(text))
			{
				return null;
			}
			int fileSize = SteamRemoteStorage.GetFileSize(text);
			if (fileSize <= 0)
			{
				MarkLoadFailed("Steam Cloud file is empty: " + text);
				return null;
			}
			byte[] array = new byte[fileSize];
			int num = SteamRemoteStorage.FileRead(text, array, fileSize);
			if (num <= 0)
			{
				MarkLoadFailed("Failed to read Steam Cloud file: " + text);
				return null;
			}
			return Encoding.UTF8.GetString(array, 0, num);
		}

		private static async UniTask<bool> WaitForSteamInitialization()
		{
			if (SteamManager.Initialized)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(SteamManager.LastError))
			{
				return false;
			}
			CancellationTokenSource cancellationToken = new CancellationTokenSource();
			UniTask uniTask = UniTask.WaitUntil(() => SteamManager.Initialized || !string.IsNullOrEmpty(SteamManager.LastError), PlayerLoopTiming.Update, cancellationToken.Token);
			UniTask uniTask2 = UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken.Token);
			await UniTask.WhenAny(uniTask, uniTask2);
			cancellationToken.Cancel();
			cancellationToken.Dispose();
			return SteamManager.Initialized;
		}

		private static void ResetLastLoadResult()
		{
			LastLoadFailed = false;
			LastLoadError = null;
		}

		private static void MarkCloudUnavailable(string error)
		{
			_cloudUnavailableForSession = true;
			_cloudUnavailableError = error;
			MarkLoadFailed(error);
		}

		private static void MarkLoadFailed(string error)
		{
			LastLoadFailed = true;
			LastLoadError = error;
		}
	}
}
