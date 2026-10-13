using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SABI;

public static class NavMeshAgentExtensions
{
	public static NavMeshAgent IncreaseAngularSpeed(this NavMeshAgent agent, float angularSpeed = 1000f)
	{
		agent.angularSpeed = angularSpeed;
		return agent;
	}

	public static bool HasReachedDestination(this NavMeshAgent agent)
	{
		if (agent == null)
		{
			return false;
		}
		if (!agent.pathPending)
		{
			return agent.remainingDistance <= agent.stoppingDistance;
		}
		return false;
	}

	public static bool HasReachedDestination(this NavMeshAgent agent, Transform destination, float tolerence = 0.1f)
	{
		return agent.transform.Distance(destination) < tolerence;
	}

	public static bool HasReachedDestination(this NavMeshAgent agent, Vector3 destination, float tolerence = 0.1f)
	{
		return agent.transform.Distance(destination) < tolerence;
	}

	public static bool SetRandomDestination(this NavMeshAgent agent, float radius, Vector3? origin = null, int areaMask = -1)
	{
		if (agent == null)
		{
			return false;
		}
		for (int i = 0; i < 10; i++)
		{
			if (NavMesh.SamplePosition(UnityEngine.Random.insideUnitSphere * radius + (origin ?? agent.transform.position), out var hit, radius, areaMask))
			{
				agent.SetDestination(hit.position);
				return true;
			}
		}
		return false;
	}

	public static NavMeshAgent SmoothSpeedChange(this NavMeshAgent agent, MonoBehaviour monobehaviour, float targetSpeed, float duration)
	{
		if (agent == null)
		{
			return null;
		}
		float startSpeed = agent.speed;
		float elapsedTime = 0f;
		monobehaviour.StartCoroutine(Routine());
		return agent;
		IEnumerator Routine()
		{
			while (elapsedTime < duration)
			{
				agent.speed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / duration);
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			agent.speed = targetSpeed;
		}
	}

	public static NavMeshAgent PatrolDestination(this NavMeshAgent agent, List<Vector3> patrolPath, float tolerance = 1f)
	{
		if (agent == null || patrolPath == null || patrolPath.Count == 0)
		{
			return null;
		}
		Vector3 vector = patrolPath[0];
		if (Vector3.Distance(agent.transform.position, vector) <= tolerance)
		{
			int index = (patrolPath.IndexOf(vector) + 1) % patrolPath.Count;
			vector = patrolPath[index];
		}
		agent.SetDestination(vector);
		return agent;
	}

	public static NavMeshAgent PatrolDestination(this NavMeshAgent agent, List<Transform> patrolPath, float tolerance = 1f)
	{
		if (agent == null || patrolPath == null || patrolPath.Count == 0)
		{
			return null;
		}
		Transform transform = patrolPath[0];
		if (Vector3.Distance(agent.transform.position, transform.position) <= tolerance)
		{
			int index = (patrolPath.IndexOf(transform) + 1) % patrolPath.Count;
			transform = patrolPath[index];
		}
		agent.SetDestination(transform.position);
		return agent;
	}

	public static NavMeshAgent AddKnockBack(this NavMeshAgent agent, Transform target, float force)
	{
		agent.ResetPath();
		Vector3 position = agent.transform.position;
		_ = target.position;
		agent.SetDestination(position + (target.position.WithY(0f) - position.WithY(0f)).normalized * force);
		return agent;
	}

	public static NavMeshAgent SetTemporarySpeed(this NavMeshAgent agent, MonoBehaviour monoBehaviour, float temporarySpeed, float duration)
	{
		if (agent == null || !agent.isActiveAndEnabled)
		{
			return null;
		}
		monoBehaviour.StartCoroutine(TemporarySpeedCoroutine(agent, temporarySpeed, duration));
		return agent;
		static IEnumerator TemporarySpeedCoroutine(NavMeshAgent navMeshAgent, float speed, float seconds)
		{
			float originalSpeed = navMeshAgent.speed;
			navMeshAgent.speed = speed;
			yield return new WaitForSeconds(seconds);
			if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
			{
				navMeshAgent.speed = originalSpeed;
			}
		}
	}

	public static NavMeshAgent Wander(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<float> radius, bool isContinues = true, Func<float> waitTime = null, Func<bool> loopWhile = null, Action OnStartMoving = null, Action OnStopMoving = null, Action OnUpdate = null)
	{
		if (agent == null || !agent.isActiveAndEnabled)
		{
			return null;
		}
		if (isContinues)
		{
			monoBehaviour.StartCoroutine(WanderCoroutine());
		}
		else
		{
			agent.SetRandomDestination(radius());
		}
		return agent;
		IEnumerator WanderCoroutine()
		{
			while (agent != null && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true))
			{
				if (agent.SetRandomDestination(radius()))
				{
					OnStartMoving?.Invoke();
					yield return new WaitUntil(() => agent.HasReachedDestination());
					OnStopMoving?.Invoke();
				}
				yield return new WaitForSeconds(waitTime?.Invoke() ?? 1f);
				OnUpdate?.Invoke();
			}
		}
	}

	public static NavMeshAgent Chase(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<Transform> target, Func<float> minDistanceKeep, Func<float> maxDistanceKeep, Func<float> delayBetweenSettingDestination = null, Func<bool> loopWhile = null, Func<float> distanceToPlayer = null, Action OnUpdate = null)
	{
		if (agent == null || !agent.isActiveAndEnabled || target == null)
		{
			return null;
		}
		monoBehaviour.StartCoroutine(ChaseTargetCoroutine());
		return agent;
		IEnumerator ChaseTargetCoroutine()
		{
			Vector3 selfPosition = agent.transform.position;
			while (agent != null && agent.isActiveAndEnabled && target != null && (loopWhile?.Invoke() ?? true))
			{
				WaitForSeconds waitForSeconds = new WaitForSeconds((delayBetweenSettingDestination == null) ? 0f : delayBetweenSettingDestination());
				float num = ((distanceToPlayer == null) ? agent.transform.Distance(target()) : distanceToPlayer());
				if (minDistanceKeep == null || maxDistanceKeep == null)
				{
					agent.SetDestination(target().position);
				}
				else if (num < minDistanceKeep())
				{
					Vector3 normalized = (target().position - selfPosition).normalized;
					Vector3 vector = selfPosition + normalized * -1f * (maxDistanceKeep() - num);
					NavMeshPath path = new NavMeshPath();
					if (agent.CalculatePath(vector, path))
					{
						agent.SetDestination(vector);
					}
				}
				else
				{
					agent.SetDestination(target().position);
				}
				if (delayBetweenSettingDestination == null)
				{
					yield return null;
				}
				else
				{
					yield return waitForSeconds;
				}
				OnUpdate?.Invoke();
			}
		}
	}

	public static NavMeshAgent Flee(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<Transform> target = null, Func<float> fleeDistance = null, Func<bool> loopWhile = null, Action OnUpdate = null)
	{
		if (agent == null || !agent.isActiveAndEnabled || target == null)
		{
			return null;
		}
		monoBehaviour.StartCoroutine(FleeFromTargetCoroutine());
		return agent;
		IEnumerator FleeFromTargetCoroutine()
		{
			while (agent != null && agent.isActiveAndEnabled && target != null && (loopWhile?.Invoke() ?? true))
			{
				float num = ((fleeDistance == null) ? 10f : fleeDistance());
				Vector3 normalized = (agent.transform.position - target().position).normalized;
				if (NavMesh.SamplePosition(agent.transform.position + normalized * num, out var hit, num, -1))
				{
					agent.SetDestination(hit.position);
				}
				yield return null;
				OnUpdate?.Invoke();
			}
		}
	}

	public static NavMeshAgent Patroll(this NavMeshAgent agent, MonoBehaviour monoBehaviour, Func<List<Transform>> waypoints, float tolerance = 0.1f, Func<float> waitTime = null, Func<bool> followWaypointOrder = null, Func<bool> loopWhile = null, Action OnStartMoving = null, Action OnStopMoving = null, Action OnUpdate = null)
	{
		monoBehaviour.StartCoroutine(PatrolWaypointsCoroutine());
		return agent;
		IEnumerator PatrolWaypointsCoroutine()
		{
			int currentWaypointIndex = 0;
			while (agent != null && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true))
			{
				Transform currentWaypoint = waypoints()[currentWaypointIndex];
				agent.SetDestination(currentWaypoint.position);
				OnStartMoving?.Invoke();
				yield return new WaitUntil(() => agent.HasReachedDestination(currentWaypoint.position, tolerance));
				OnStopMoving?.Invoke();
				yield return new WaitForSeconds(waitTime?.Invoke() ?? 1f);
				currentWaypointIndex = ((followWaypointOrder == null || followWaypointOrder()) ? ((currentWaypointIndex + 1) % waypoints().Count) : UnityEngine.Random.Range(0, waypoints().Count));
				OnUpdate?.Invoke();
			}
		}
	}

	public static NavMeshAgent ContinuesAvoidObstaclesWhile(this NavMeshAgent agent, LayerMask obstacleMask, float avoidanceRadius, MonoBehaviour monoBehaviour, Func<bool> condition = null)
	{
		if (agent == null || !agent.isActiveAndEnabled)
		{
			return null;
		}
		monoBehaviour.StartCoroutine(AvoidObstaclesCoroutine());
		return agent;
		IEnumerator AvoidObstaclesCoroutine()
		{
			while (agent != null && agent.isActiveAndEnabled && (condition?.Invoke() ?? true))
			{
				Collider[] array = Physics.OverlapSphere(agent.transform.position, avoidanceRadius, obstacleMask);
				if (array.Length != 0)
				{
					Vector3 zero = Vector3.zero;
					Collider[] array2 = array;
					foreach (Collider collider in array2)
					{
						zero += (agent.transform.position - collider.transform.position).normalized;
					}
					zero /= (float)array.Length;
					if (NavMesh.SamplePosition(agent.transform.position + zero * avoidanceRadius, out var hit, avoidanceRadius, -1))
					{
						agent.SetDestination(hit.position);
					}
				}
				yield return null;
			}
		}
	}
}
