using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SABI
{
	public static class NavMeshAgentExtensions
	{
		public static NavMeshAgent IncreaseAngularSpeed(this NavMeshAgent agent, float angularSpeed = 1000f)
		{
			return null;
		}

		public static bool HasReachedDestination(this NavMeshAgent agent)
		{
			return false;
		}

		public static bool HasReachedDestination(this NavMeshAgent agent, Transform destination, float tolerence = 0.1f)
		{
			return false;
		}

		public static bool HasReachedDestination(this NavMeshAgent agent, Vector3 destination, float tolerence = 0.1f)
		{
			return false;
		}

		public static bool SetRandomDestination(this NavMeshAgent agent, float radius, Vector3? origin = null, int areaMask = -1)
		{
			return false;
		}

		public static NavMeshAgent SmoothSpeedChange(this NavMeshAgent agent, MonoBehaviour monobehaviour, float targetSpeed, float duration)
		{
			return null;
		}

		public static NavMeshAgent PatrolDestination(this NavMeshAgent agent, List<Vector3> patrolPath, float tolerance = 1f)
		{
			return null;
		}

		public static NavMeshAgent PatrolDestination(this NavMeshAgent agent, List<Transform> patrolPath, float tolerance = 1f)
		{
			return null;
		}

		public static NavMeshAgent AddKnockBack(this NavMeshAgent agent, Transform target, float force)
		{
			return null;
		}

		public static NavMeshAgent SetTemporarySpeed(this NavMeshAgent agent, MonoBehaviour monoBehaviour, float temporarySpeed, float duration)
		{
			return null;
		}

		public static NavMeshAgent Wander(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<float> radius, bool isContinues = true, Func<float> waitTime = null, Func<bool> loopWhile = null, Action OnStartMoving = null, Action OnStopMoving = null, Action OnUpdate = null)
		{
			return null;
		}

		public static NavMeshAgent Chase(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<Transform> target, Func<float> minDistanceKeep, Func<float> maxDistanceKeep, Func<float> delayBetweenSettingDestination = null, Func<bool> loopWhile = null, Func<float> distanceToPlayer = null, Action OnUpdate = null)
		{
			return null;
		}

		public static NavMeshAgent Flee(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<Transform> target = null, Func<float> fleeDistance = null, Func<bool> loopWhile = null, Action OnUpdate = null)
		{
			return null;
		}

		public static NavMeshAgent Patroll(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<List<Transform>> waypoints, float tolerance = 0.1f, Func<float> waitTime = null, Func<bool> followWaypointOrder = null, Func<bool> loopWhile = null, Action OnStartMoving = null, Action OnStopMoving = null, Action OnUpdate = null)
		{
			return null;
		}

		public static NavMeshAgent ContinuesAvoidObstaclesWhile(this NavMeshAgent agent, LayerMask obstacleMask, float avoidanceRadius, MonoBehaviour monoBehaviour, Func<bool> condition = null)
		{
			return null;
		}
	}
}
