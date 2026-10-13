using UnityEngine;

namespace _Code.Infrastructure.TriggerObjects
{
	[RequireComponent(typeof(Collider))]
	public abstract class ATriggerObject : MonoBehaviour
	{
		private void OnEnter(Collider other)
		{
		}

		private void OnExit(Collider other)
		{
		}

		protected virtual void OnEnterInner(Collider other)
		{
		}

		protected virtual void OnExitInner(Collider other)
		{
		}

		private void OnTriggerEnter(Collider other)
		{
		}

		private void OnTriggerExit(Collider other)
		{
		}
	}
}
