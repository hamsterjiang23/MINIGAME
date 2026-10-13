using UnityEngine;
using _Code.Player;
using _Code.Raycast;

namespace _Code.Infrastructure;

public abstract class AInteractableObject : MonoBehaviour, IInteractable
{
	[SerializeField]
	protected RaycastTargetHint _raycastTarget;

	private InputHandling _inputHandler;

	protected bool _isEnabled = true;

	private int _lockCount;

	public abstract bool HardConditions { get; }

	public abstract bool SoftConditions { get; }

	protected virtual int EnergyCost => 0;

	public abstract void Interact();

	public void OnUpdate()
	{
		if (_inputHandler.InteractTriggered)
		{
			RaycastTargetHint raycastTarget = _raycastTarget;
			if ((object)raycastTarget != null && raycastTarget.IsTargeted && !OnUpdateExtraConditions() && SoftConditions && HardConditions && _isEnabled)
			{
				Interact();
			}
		}
	}

	public void PreInit(IInputHandlerProvider inputHandlerProvider)
	{
		_inputHandler = inputHandlerProvider.InputHandler;
	}

	protected virtual bool OnUpdateExtraConditions()
	{
		return false;
	}

	public void Enable()
	{
		if (_lockCount > 0)
		{
			_lockCount--;
		}
		_isEnabled = _lockCount == 0;
		_raycastTarget?.SetLockedState(isLocked: false);
	}

	public void Disable()
	{
		_lockCount++;
		_isEnabled = _lockCount == 0;
		_raycastTarget?.SetLockedState(isLocked: true);
	}

	public virtual void OnLoad()
	{
	}
}
