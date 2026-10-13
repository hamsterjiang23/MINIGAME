using UnityEngine;

namespace _Code.Infrastructure.ActionableObjects
{
	public sealed class ActionableObjectsViewProvider : MonoBehaviour, IActionableObjectsViewProvider
	{
		[field: SerializeField]
		public AActionableObjectView[] ActionableObjectViews { get; private set; }
	}
}
