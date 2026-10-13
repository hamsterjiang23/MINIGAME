using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Raycast
{
	public sealed class RaycastTargetUnityEvent : ARaycastTarget
	{
		[SerializeField]
		private UnityEvent _onFocused;

		[SerializeField]
		private UnityEvent _onLostFocus;

		protected override void OnFocused()
		{
		}

		protected override void OnLostFocus()
		{
		}

		protected override void OnTargetedWrongConditions()
		{
		}

		protected override void OnTargetedCorrectConditions()
		{
		}
	}
}
