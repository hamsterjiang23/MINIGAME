using System;
using Cysharp.Threading.Tasks;
using DefaultNamespace.EnumEventBus;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Pause;
using _Code.Menues.HUD;

namespace _Code.Infrastructure._NINAH__InteractableObjects.Objects;

public sealed class CloseSceneInteractable : AInteractableObject
{
	[SerializeField]
	private int _sceneIndex;

	private IHUDPresenter _hudPresenter;

	private CommonEnumEventus _commonEnumEventus;

	public override bool HardConditions => true;

	public override bool SoftConditions => true;

	public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, CommonEnumEventus commonEnumEventus)
	{
		_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
		_hudPresenter = hudPresenter;
		_commonEnumEventus = commonEnumEventus;
	}

	public override void Interact()
	{
		InteractAsync().Forget();
	}

	private async UniTaskVoid InteractAsync()
	{
		_commonEnumEventus.Invoke<float>(ECommonEvents.HudFadeIn, new object[1] { 1f });
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		SceneManager.UnloadSceneAsync(_sceneIndex);
	}
}
