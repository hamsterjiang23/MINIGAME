using System;
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
			CatPosition[] dayPositions = DayPositions;
			foreach (CatPosition catPosition in dayPositions)
			{
				Gizmos.color = new Color(0.4f, 0.4f, 0.2f);
				Gizmos.DrawSphere(catPosition.Position, 0.1f);
				Vector3 direction = new Vector3(Mathf.Cos((90f - catPosition.Rotation) * ((float)Math.PI / 180f)), 0f, Mathf.Sin((90f - catPosition.Rotation) * ((float)Math.PI / 180f))) * 0.3f;
				Gizmos.DrawRay(catPosition.Position, direction);
			}
			dayPositions = NightPositions;
			foreach (CatPosition catPosition2 in dayPositions)
			{
				Gizmos.color = new Color(0.2f, 0.2f, 0.4f);
				Gizmos.DrawSphere(catPosition2.Position, 0.1f);
				Vector3 direction2 = new Vector3(Mathf.Cos((90f - catPosition2.Rotation) * ((float)Math.PI / 180f)), 0f, Mathf.Sin((90f - catPosition2.Rotation) * ((float)Math.PI / 180f))) * 0.3f;
				Gizmos.DrawRay(catPosition2.Position, direction2);
			}
		}
	}
}
