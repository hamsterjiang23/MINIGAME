using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SteamAchievements : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CWaitForSteamReady_003Ed__18 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public SteamAchievements _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CWaitForSteamReady_003Ed__18(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[Tooltip("Every Steam achievement ID (exact spelling).")]
	[SerializeField]
	private List<string> achievements;

	private const string CacheKey = "STEAM_PENDING_ACH";

	private readonly HashSet<string> pending;

	private HashSet<string> fastLookup;

	private Callback<UserStatsReceived_t> statsReceived;

	private Callback<UserStatsStored_t> statsStored;

	private bool callbacksReady;

	public static SteamAchievements Instance { get; private set; }

	public List<string> Achievements => null;

	public static void Unlock(string id)
	{
	}

	public static void Increment(string id, int step, int target)
	{
	}

	public static void Clear(string id)
	{
	}

	public static void ClearAll()
	{
	}

	private void Awake()
	{
	}

	[IteratorStateMachine(typeof(_003CWaitForSteamReady_003Ed__18))]
	private IEnumerator WaitForSteamReady()
	{
		return null;
	}

	private void SetupCallbacks()
	{
	}

	private void OnApplicationQuit()
	{
	}

	private void _Unlock(string id)
	{
	}

	private void _Increment(string id, int step, int target)
	{
	}

	private void _Clear(string id)
	{
	}

	private void _ClearAll()
	{
	}

	private void OnStatsReceived(UserStatsReceived_t p)
	{
	}

	private void OnStatsStored(UserStatsStored_t p)
	{
	}

	private void LoadCache()
	{
	}

	private void SaveCache()
	{
	}

	private void FlushCache()
	{
	}
}
