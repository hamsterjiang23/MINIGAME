using System;
using UnityEngine;

namespace _Code.Infrastructure.Locations
{
	public sealed class StartPoint : MonoBehaviour
	{
		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(base.transform.position, 0.3f);
			Gizmos.color = Color.green;
			Vector3 direction = new Vector3(Mathf.Cos((90f - base.transform.rotation.eulerAngles.y) * ((float)Math.PI / 180f)), 0f, Mathf.Sin((90f - base.transform.rotation.eulerAngles.y) * ((float)Math.PI / 180f))) * 0.3f;
			Gizmos.DrawRay(base.transform.position, direction);
			Gizmos.DrawSphere(base.transform.position, 0.03f);
		}
	}
}
