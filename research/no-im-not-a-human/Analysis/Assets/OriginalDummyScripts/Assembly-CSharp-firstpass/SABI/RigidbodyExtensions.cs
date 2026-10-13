using System;
using System.Collections.Generic;
using UnityEngine;

namespace SABI
{
	public static class RigidbodyExtensions
	{
		public static Rigidbody ChangeDirection(this Rigidbody rigidbody, Vector3 direction)
		{
			return null;
		}

		public static Rigidbody AddForceToReachVelocity(this Rigidbody rigidbody, Vector3 targetVelocity, float maxForce)
		{
			return null;
		}

		public static Rigidbody Stop(this Rigidbody rigidbody)
		{
			return null;
		}

		public static bool IsAlmostStopped(this Rigidbody rigidbody, float threshold = 0.1f)
		{
			return false;
		}

		public static Rigidbody MoveTowards(this Rigidbody source, Transform target, float speed)
		{
			return null;
		}

		public static Rigidbody MoveTowards(this Rigidbody source, Vector3 target, float speed)
		{
			return null;
		}

		public static Rigidbody ContinuesChaseTargetWhile(this Rigidbody agent, Transform target, MonoBehaviour monoBehaviour, float speed = 5f, float? minDistanceKeep = null, float? maxDistanceKeep = null, float? delayBetweenSettingDestination = null, Func<bool> loopCondition = null, Func<float> distanceToPlayer = null)
		{
			return null;
		}

		public static bool SetRandomDestination(this Rigidbody agent, out Vector3 randomLocation, float radius, float speed, Vector3? origin = null)
		{
			randomLocation = default;
			return false;
		}

		public static bool SetRandomDestination(this Rigidbody agent, float radius, float speed, Vector3? origin = null)
		{
			return false;
		}

		public static Rigidbody Wander(this Rigidbody agent, float radius, MonoBehaviour monoBehaviour, float speed, bool isContinues = true, float waitTime = 1f, Func<bool> condition = null, bool useSameHeight = true)
		{
			return null;
		}

		public static Rigidbody ContinuesFleeFromTargetWhile(this Rigidbody agent, Transform target, MonoBehaviour monoBehaviour, float speed, float fleeDistance = 10f, Func<bool> condition = null)
		{
			return null;
		}

		public static Rigidbody ContinuesPatrolWaypointsWhile(this Rigidbody agent, List<Transform> waypoints, float speed, MonoBehaviour monoBehaviour, bool followWaypointOrder = true, Func<bool> condition = null)
		{
			return null;
		}
	}
}
