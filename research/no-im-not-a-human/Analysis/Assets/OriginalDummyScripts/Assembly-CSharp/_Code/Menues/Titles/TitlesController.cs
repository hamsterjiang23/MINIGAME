using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Player;

namespace _Code.Menues.Titles;

public sealed class TitlesController
{
	private readonly WatcherManager _watcherManager;

	public TitlesController(IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
	{
		_watcherManager = watcherManager;
		InitTitles(inputHandlerProvider).Forget();
		watcherManager.EnterState(EWatcherState.Credits);
	}

	private async UniTask InitTitles(IInputHandlerProvider inputHandlerProvider)
	{
		Object.FindFirstObjectByType<TitlesControl>(FindObjectsInactive.Exclude).Init(inputHandlerProvider, _watcherManager);
	}
}
