using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Raycast;

public sealed class RaycastTargetUnityEvent : ARaycastTarget
{
	[SerializeField]
	private UnityEvent _onFocused;

	[SerializeField]
	private UnityEvent _onLostFocus;

	protected override void OnFocused()
	{
		_onFocused?.Invoke();
	}

	protected override void OnLostFocus()
	{
		_onLostFocus?.Invoke();
	}

	protected override void OnTargetedWrongConditions()
	{
	}

	protected override void OnTargetedCorrectConditions()
	{
	}
}
