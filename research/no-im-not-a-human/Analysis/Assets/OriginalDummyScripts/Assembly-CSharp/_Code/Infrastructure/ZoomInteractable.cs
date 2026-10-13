using UnityEngine;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Pause;
using _Code.Menues.HUD;
using _Code.Player;

namespace _Code.Infrastructure;

public class ZoomInteractable : AInteractableObject
{
	[SerializeField]
	private GameObject _camera;

	private bool _isOpened;

	protected IHUDPresenter _hudPresenter;

	protected InputHandling _inputHandler;

	public override bool HardConditions => true;

	public override bool SoftConditions => true;

	public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IInputHandlerProvider inputHandlerProvider)
	{
		_hudPresenter = hudPresenter;
		_inputHandler = inputHandlerProvider.InputHandler;
		_raycastTarget?.Init(hudPresenter, pauseController, () => HardConditions);
	}

	public override void Interact()
	{
		_hudPresenter.HideHint();
		_raycastTarget?.SetLockedState(isLocked: true);
		_isOpened = true;
		_camera.SetActive(value: true);
		_hudPresenter.SetupAndShowControlsView(EControlsList.InUsualCloseUp);
		_inputHandler.SetIsInUIState(isInUi: true);
	}

	private void Update()
	{
		if (_isOpened && _inputHandler.UIExitTriggered)
		{
			Close();
		}
	}

	private void Close()
	{
		_isOpened = false;
		_raycastTarget?.SetLockedState(isLocked: false);
		_camera.SetActive(value: false);
		_hudPresenter.HideControlsView();
		_inputHandler.SetIsInUIState(isInUi: false);
	}
}
