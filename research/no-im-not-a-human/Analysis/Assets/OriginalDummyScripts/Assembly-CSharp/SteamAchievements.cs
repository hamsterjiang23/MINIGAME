using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SteamAchievements : MonoBehaviour
{
	[Tooltip("Every Steam achievement ID (exact spelling).")]
	[SerializeField]
	private List<string> achievements = new List<string>
	{
		"END_LIVE_TILL_END", "END_IMPOSTERS", "END_SUPER", "END_ENTRING_DEATH", "END_MUSHROOM", "END_CULTISTS", "END_FATAL_CHILD", "END_KILLED", "END_FEMA", "END_VIGILANTE",
		"ALL_ENDINGS", "SPEC_FAN_PUKE", "SPEC_COLD_KILL", "SPEC_BALLERINA_TWIST", "SPEC_TEACHER_SUICIDE", "SPEC_BLIND_HANGED", "SPEC_SEXY_PREG", "ALL_CHARACTERS", "ALL_RADIO", "ALL_TV",
		"HOUSE_INSPECTOR", "PHONEBOOK", "MIRROR_SELF", "CAT_FEED", "CONSUM_TRY_EACH", "CONSUM_COFFEE_CIG", "COURIER_ORDER", "LET_ALL_IN", "COMMERCIALISM", "CITY_VIEW",
		"FRENEMY", "KILL_HUMAN", "WEEK1_IMPOSTERS_DOESNT_KILL", "NO_SAVE", "SPEEDYGONZALEZ", "KILL_IMPOSTER", "REFUSE_WOLFHOUND_GREATMOTHER", "KILL_PROVOCATEUR", "KILL_RASK", "WHEELCHAIR_FINGERS",
		"YAKOB_LETIN", "PSYCHIC_CALL", "WTF", "FEMA_PAPERS", "WIDOW_PHOTO", "HOT_POTATO", "FIRST_CHECK", "SUPER_LAST", "BODY_EATER"
	};

	private const string CacheKey = "STEAM_PENDING_ACH";

	private readonly HashSet<string> pending = new HashSet<string>();

	private HashSet<string> fastLookup;

	private Callback<UserStatsReceived_t> statsReceived;

	private Callback<UserStatsStored_t> statsStored;

	private bool callbacksReady;

	public static SteamAchievements Instance { get; private set; }

	public List<string> Achievements => achievements;

	public static void Unlock(string id)
	{
		Instance?._Unlock(id);
	}

	public static void Increment(string id, int step, int target)
	{
		Instance?._Increment(id, step, target);
	}

	public static void Clear(string id)
	{
		Instance?._Clear(id);
	}

	public static void ClearAll()
	{
		Instance?._ClearAll();
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(this);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad(gameObject);
		fastLookup = new HashSet<string>(achievements);
		LoadCache();
		if (SteamManager.Initialized)
		{
			SetupCallbacks();
		}
		else
		{
			StartCoroutine(WaitForSteamReady());
		}
	}

	private IEnumerator WaitForSteamReady()
	{
		yield return new WaitUntil(() => SteamManager.Initialized);
		SetupCallbacks();
	}

	private void SetupCallbacks()
	{
		if (!callbacksReady)
		{
			callbacksReady = true;
			statsReceived = Callback<UserStatsReceived_t>.Create(OnStatsReceived);
			statsStored = Callback<UserStatsStored_t>.Create(OnStatsStored);
			SteamUserStats.RequestCurrentStats();
		}
	}

	private void OnApplicationQuit()
	{
		SaveCache();
	}

	private void _Unlock(string id)
	{
		if (!fastLookup.Contains(id))
		{
			Debug.LogError("SteamAchievements: unknown ID \"" + id + "\"");
			return;
		}
		if (!SteamManager.Initialized)
		{
			pending.Add(id);
			return;
		}
		SteamUserStats.GetAchievement(id, out var pbAchieved);
		if (!pbAchieved)
		{
			SteamUserStats.SetAchievement(id);
			SteamUserStats.StoreStats();
		}
	}

	private void _Increment(string id, int step, int target)
	{
		if (!fastLookup.Contains(id))
		{
			Debug.LogError("SteamAchievements: unknown stat \"" + id + "\"");
		}
		else if (SteamManager.Initialized)
		{
			SteamUserStats.GetStat(id, out int pData);
			int num = Mathf.Clamp(pData + step, 0, target);
			SteamUserStats.SetStat(id, num);
			if (num >= target)
			{
				SteamUserStats.SetAchievement(id);
			}
			SteamUserStats.StoreStats();
		}
	}

	private void _Clear(string id)
	{
		if (!fastLookup.Contains(id))
		{
			Debug.LogError("SteamAchievements: unknown ID \"" + id + "\"");
		}
		else if (SteamManager.Initialized)
		{
			SteamUserStats.ClearAchievement(id);
			SteamUserStats.StoreStats();
		}
	}

	private void _ClearAll()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		foreach (string achievement in achievements)
		{
			SteamUserStats.ClearAchievement(achievement);
		}
		SteamUserStats.StoreStats();
	}

	private void OnStatsReceived(UserStatsReceived_t p)
	{
		FlushCache();
	}

	private void OnStatsStored(UserStatsStored_t p)
	{
	}

	private void LoadCache()
	{
		if (!PlayerPrefs.HasKey("STEAM_PENDING_ACH"))
		{
			return;
		}
		string[] array = PlayerPrefs.GetString("STEAM_PENDING_ACH").Split('|');
		foreach (string text in array)
		{
			if (text.Length > 0)
			{
				pending.Add(text);
			}
		}
	}

	private void SaveCache()
	{
		if (pending.Count == 0)
		{
			PlayerPrefs.DeleteKey("STEAM_PENDING_ACH");
			return;
		}
		PlayerPrefs.SetString("STEAM_PENDING_ACH", string.Join("|", pending));
		PlayerPrefs.Save();
	}

	private void FlushCache()
	{
		if (pending.Count == 0)
		{
			return;
		}
		foreach (string item in new List<string>(pending))
		{
			SteamUserStats.SetAchievement(item);
			pending.Remove(item);
		}
		SteamUserStats.StoreStats();
		SaveCache();
	}
}
