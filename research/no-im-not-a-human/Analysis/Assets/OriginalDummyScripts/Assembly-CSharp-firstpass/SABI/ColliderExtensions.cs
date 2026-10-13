using System.Collections.Generic;
using UnityEngine;

namespace SABI
{
	public static class ColliderExtensions
	{
		public static bool IsCollidingWith(this Collider collider, Collider otherCollider)
		{
			return false;
		}

		public static Collider DisableCollisionWith(this Collider collider, Collider otherCollider)
		{
			return null;
		}

		public static Collider EnableCollisionWith(this Collider collider, Collider otherCollider)
		{
			return null;
		}

		public static Collider DisableCollisionWith(this Collider collider, List<Collider> otherColliders)
		{
			return null;
		}

		public static Collider EnableCollisionWith(this Collider collider, List<Collider> otherColliders)
		{
			return null;
		}

		public static bool IsCollidingWithLayer(this Collider collider, LayerMask layerMask)
		{
			return false;
		}
	}
}
