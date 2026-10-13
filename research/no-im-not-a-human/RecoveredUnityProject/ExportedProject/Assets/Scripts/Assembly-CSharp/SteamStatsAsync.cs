using System;
using Cysharp.Threading.Tasks;
using Steamworks;

public static class SteamStatsAsync
{
	public static UniTask<GlobalStatsReceived_t> RequestGlobalStatsAsync(int days)
	{
		UniTaskCompletionSource<GlobalStatsReceived_t> tcs = new UniTaskCompletionSource<GlobalStatsReceived_t>();
		Callback<GlobalStatsReceived_t> callback = null;
		callback = Callback<GlobalStatsReceived_t>.Create((GlobalStatsReceived_t result) =>
		{
			if (result.m_nGameID == SteamUtils.GetAppID().m_AppId)
			{
				tcs.TrySetResult(result);
			}
			else
			{
				tcs.TrySetException(new Exception("GlobalStats received for another AppID"));
			}
			callback.Dispose();
		});
		SteamUserStats.RequestGlobalStats(days);
		return tcs.Task;
	}
}
