using UnityEngine;

namespace _Code.Infrastructure._NINAH__CommonView
{
	public sealed class CommonViewProvider : MonoBehaviour, ICommonViewProvider
	{
		[field: SerializeField]
		public Camera PlayerCamera { get; private set; }
	}
}
