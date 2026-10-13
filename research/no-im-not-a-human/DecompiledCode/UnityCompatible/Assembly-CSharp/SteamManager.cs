using System;
using System.Text;
using AOT;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SteamManager : MonoBehaviour
{
	private static SteamManager _instance;

	private static bool _everInit;

	private static string _lastError;

	private bool _initialized;

	private SteamAPIWarningMessageHook_t _msgHook;

	public static bool Initialized
	{
		get
		{
			if (_instance != null)
			{
				return _instance._initialized;
			}
			return false;
		}
	}

	public static string LastError => _lastError;

	public static event Action OnSteamReady;

	public static event Action<string> OnSteamFailure;

	private void Awake()
	{
		if (_instance != null)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		_instance = this;
		if (transform.parent != null)
		{
			transform.SetParent(null, worldPositionStays: false);
		}
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		if (_everInit)
		{
			Debug.LogWarning("SteamManager recreated after domain reload – skipping init.");
			return;
		}
		_everInit = true;
		BootstrapSteam();
	}

	private void BootstrapSteam()
	{
		if (!Packsize.Test())
		{
			_Fail("Packsize test failed – wrong Steamworks.NET build.");
			return;
		}
		if (!DllCheck.Test())
		{
			_Fail("DLL check failed – missing or wrong native steam_api.* file.");
			return;
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
			{
				_Fail("Restarting via Steam client.");
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			_Fail("Native steam_api library not found:\n" + ex);
			Application.Quit();
			return;
		}
		_initialized = SteamAPI.Init();
		if (!_initialized)
		{
			_Fail("SteamAPI.Init() failed.\n • Is the Steam client running and logged in?\n • Does steam_appid.txt exist next to the EXE while testing?\n • Does this account own the game and is the app fully set up on the partner site?");
			return;
		}
		_msgHook = DebugHook;
		SteamClient.SetWarningMessageHook(_msgHook);
		Debug.Log("[SteamManager] SUCCESS – logged-in user: " + SteamFriends.GetPersonaName());
		OnSteamReady?.Invoke();
	}

	private void Update()
	{
		if (_initialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	private void OnDestroy()
	{
		if (!(_instance != this))
		{
			if (_initialized)
			{
				SteamAPI.Shutdown();
			}
			_instance = null;
		}
	}

	[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	private static void DebugHook(int severity, StringBuilder text)
	{
		Debug.LogWarning($"[Steam] {text}");
	}

	private static void _Fail(string msg)
	{
		_lastError = msg;
		Debug.LogError("[SteamManager] " + msg);
		OnSteamFailure?.Invoke(msg);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void DomainReload()
	{
		_instance = null;
		_everInit = false;
		_lastError = null;
	}
}
