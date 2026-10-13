using UnityEngine;

namespace _Code.Infrastructure._NINAH__Cat
{
	public sealed class CatViewProvider : MonoBehaviour, ICatViewProvider
	{
		[field: SerializeField]
		public CatInstance Cat { get; private set; }

		[field: SerializeField]
		public CatPosition[] DayPositions { get; private set; }

		[field: SerializeField]
		public CatPosition[] NightPositions { get; private set; }

		private void OnDrawGizmosSelected()
		{
		}
	}
}
