using System;
using System.Runtime.CompilerServices;
using System.Text;
using AOT;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SteamManager : MonoBehaviour
{
	[CompilerGenerated]
	private static Action m_OnSteamReady;

	[CompilerGenerated]
	private static Action<string> m_OnSteamFailure;

	private static SteamManager _instance;

	private static bool _everInit;

	private static string _lastError;

	private bool _initialized;

	private SteamAPIWarningMessageHook_t _msgHook;

	public static bool Initialized => false;

	public static string LastError => null;

	public static event Action OnSteamReady
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	public static event Action<string> OnSteamFailure
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	private void Awake()
	{
	}

	private void BootstrapSteam()
	{
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
	}

	[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	private static void DebugHook(int severity, StringBuilder text)
	{
	}

	private static void _Fail(string msg)
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void DomainReload()
	{
	}
}
