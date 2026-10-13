using UnityEngine;
using _Code.Player;
using _Code.Raycast;

namespace _Code.Infrastructure
{
	public abstract class AInteractableObject : MonoBehaviour, IInteractable
	{
		[SerializeField]
		protected RaycastTargetHint _raycastTarget;

		private InputHandling _inputHandler;

		protected bool _isEnabled;

		private int _lockCount;

		public abstract bool HardConditions { get; }

		public abstract bool SoftConditions { get; }

		protected virtual int EnergyCost => 0;

		public abstract void Interact();

		public void OnUpdate()
		{
		}

		public void PreInit(IInputHandlerProvider inputHandlerProvider)
		{
		}

		protected virtual bool OnUpdateExtraConditions()
		{
			return false;
		}

		public void Enable()
		{
		}

		public void Disable()
		{
		}

		public virtual void OnLoad()
		{
		}
	}
}
