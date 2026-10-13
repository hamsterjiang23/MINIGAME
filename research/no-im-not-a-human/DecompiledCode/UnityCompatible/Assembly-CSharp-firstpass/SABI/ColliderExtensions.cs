using System.Collections.Generic;
using UnityEngine;

namespace SABI
{
	public static class ColliderExtensions
	{
		public static bool IsCollidingWith(this Collider collider, Collider otherCollider)
		{
			return collider.bounds.Intersects(otherCollider.bounds);
		}

		public static Collider DisableCollisionWith(this Collider collider, Collider otherCollider)
		{
			Physics.IgnoreCollision(collider, otherCollider, ignore: true);
			return collider;
		}

		public static Collider EnableCollisionWith(this Collider collider, Collider otherCollider)
		{
			Physics.IgnoreCollision(collider, otherCollider, ignore: false);
			return collider;
		}

		public static Collider DisableCollisionWith(this Collider collider, List<Collider> otherColliders)
		{
			if (collider == null || otherColliders == null)
			{
				return null;
			}
			for (int i = 0; i < otherColliders.Count; i++)
			{
				collider.DisableCollisionWith(otherColliders[i]);
			}
			return collider;
		}

		public static Collider EnableCollisionWith(this Collider collider, List<Collider> otherColliders)
		{
			if (collider == null || otherColliders == null)
			{
				return null;
			}
			for (int i = 0; i < otherColliders.Count; i++)
			{
				collider.EnableCollisionWith(otherColliders[i]);
			}
			return collider;
		}

		public static bool IsCollidingWithLayer(this Collider collider, LayerMask layerMask)
		{
			if (collider == null)
			{
				return false;
			}
			return Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, collider.transform.rotation, layerMask).Length != 0;
		}
	}
}
