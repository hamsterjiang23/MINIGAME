using System;
using Cysharp.Threading.Tasks;
using DefaultNamespace.EnumEventBus;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Code.Infrastructure.EnumEventBus;

namespace _Code.Infrastructure.TriggerObjects.Objects;

public sealed class TriggerObjectCloseScene : ATriggerObject
{
	[SerializeField]
	private int _sceneIndex;

	private CommonEnumEventus _commonEnumEventBus;

	public void Init(CommonEnumEventus commonEnumEventBus)
	{
		_commonEnumEventBus = commonEnumEventBus;
	}

	protected override void OnEnterInner(Collider other)
	{
		OnEnterAsync().Forget();
	}

	private async UniTask OnEnterAsync()
	{
		_commonEnumEventBus.Invoke<float>(ECommonEvents.HudFadeIn, new object[1] { 1f });
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		SceneManager.UnloadSceneAsync(_sceneIndex);
	}
}
