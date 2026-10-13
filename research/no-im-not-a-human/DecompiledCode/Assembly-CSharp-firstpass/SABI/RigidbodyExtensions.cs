using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SABI;

public static class RigidbodyExtensions
{
	public static Rigidbody ChangeDirection(this Rigidbody rigidbody, Vector3 direction)
	{
		direction.Normalize();
		rigidbody.linearVelocity = direction * rigidbody.linearVelocity.magnitude;
		return rigidbody;
	}

	public static Rigidbody AddForceToReachVelocity(this Rigidbody rigidbody, Vector3 targetVelocity, float maxForce)
	{
		Vector3 vector = (targetVelocity - rigidbody.linearVelocity) * rigidbody.mass / Time.fixedDeltaTime;
		vector = Vector3.ClampMagnitude(vector, maxForce);
		rigidbody.AddForce(vector, ForceMode.Force);
		return rigidbody;
	}

	public static Rigidbody Stop(this Rigidbody rigidbody)
	{
		rigidbody.linearVelocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		return rigidbody;
	}

	public static bool IsAlmostStopped(this Rigidbody rigidbody, float threshold = 0.1f)
	{
		return rigidbody.linearVelocity.magnitude < threshold;
	}

	public static Rigidbody MoveTowards(this Rigidbody source, Transform target, float speed)
	{
		source.position = Vector3.MoveTowards(source.position, target.position, speed);
		return source;
	}

	public static Rigidbody MoveTowards(this Rigidbody source, Vector3 target, float speed)
	{
		source.position = Vector3.MoveTowards(source.position, target, speed);
		return source;
	}

	public static Rigidbody ContinuesChaseTargetWhile(this Rigidbody agent, Transform target, MonoBehaviour monoBehaviour, float speed = 5f, float? minDistanceKeep = null, float? maxDistanceKeep = null, float? delayBetweenSettingDestination = null, Func<bool> loopCondition = null, Func<float> distanceToPlayer = null)
	{
		monoBehaviour.StartCoroutine(ChaseTargetCoroutine());
		return agent;
		IEnumerator ChaseTargetCoroutine()
		{
			WaitForSeconds delay = new WaitForSeconds(delayBetweenSettingDestination.GetValueOrDefault());
			Vector3 selfPosition = agent.transform.position;
			while (!(agent != null) || !(target != null) || loopCondition == null || loopCondition())
			{
				float num = ((distanceToPlayer == null) ? agent.transform.Distance(target) : distanceToPlayer());
				if (!minDistanceKeep.HasValue || !maxDistanceKeep.HasValue)
				{
					agent.MoveTowards(target.position, speed * Time.deltaTime);
				}
				else if (num < minDistanceKeep.Value)
				{
					Vector3 normalized = (target.position - selfPosition).normalized;
					Vector3 v = selfPosition + normalized * -1f * (maxDistanceKeep.Value - num);
					agent.MoveTowards(v.WithY(selfPosition.y), speed * Time.deltaTime);
				}
				else
				{
					agent.MoveTowards(target.position, speed * Time.deltaTime);
				}
				if (!delayBetweenSettingDestination.HasValue)
				{
					yield return null;
				}
				else
				{
					yield return delay;
				}
			}
		}
	}

	public static bool SetRandomDestination(this Rigidbody agent, out Vector3 randomLocation, float radius, float speed, Vector3? origin = null)
	{
		Vector3 vector = UnityEngine.Random.insideUnitSphere * radius;
		vector = (randomLocation = vector + (origin ?? agent.transform.position));
		agent.MoveTowards(randomLocation, speed);
		return true;
	}

	public static bool SetRandomDestination(this Rigidbody agent, float radius, float speed, Vector3? origin = null)
	{
		Vector3 target = UnityEngine.Random.insideUnitSphere * radius + (origin ?? agent.transform.position);
		agent.MoveTowards(target, speed);
		return true;
	}

	public static Rigidbody Wander(this Rigidbody agent, float radius, MonoBehaviour monoBehaviour, float speed, bool isContinues = true, float waitTime = 1f, Func<bool> condition = null, bool useSameHeight = true)
	{
		if (isContinues)
		{
			monoBehaviour.StartCoroutine(WanderCoroutine());
		}
		else
		{
			agent.SetRandomDestination(radius, speed * Time.deltaTime);
		}
		return agent;
		IEnumerator WanderCoroutine()
		{
			Vector3 randomLocation = UnityEngine.Random.insideUnitSphere * radius;
			if (useSameHeight)
			{
				randomLocation.SetY(agent.position);
			}
			while (!(agent != null) || condition == null || condition())
			{
				if (agent.transform.HasReachedDestination(randomLocation))
				{
					yield return new WaitForSeconds(waitTime);
					randomLocation = UnityEngine.Random.insideUnitSphere * radius;
					if (useSameHeight)
					{
						randomLocation.SetY(agent.position);
					}
				}
				else
				{
					agent.MoveTowards(randomLocation, 1f);
				}
				yield return null;
			}
		}
	}

	public static Rigidbody ContinuesFleeFromTargetWhile(this Rigidbody agent, Transform target, MonoBehaviour monoBehaviour, float speed, float fleeDistance = 10f, Func<bool> condition = null)
	{
		if (agent == null || target == null)
		{
			return null;
		}
		monoBehaviour.StartCoroutine(FleeFromTargetCoroutine());
		return agent;
		IEnumerator FleeFromTargetCoroutine()
		{
			while (!(agent != null) || !(target != null) || condition == null || condition())
			{
				Vector3 normalized = (agent.transform.position - target.position).normalized;
				Vector3 target2 = agent.transform.position + normalized * fleeDistance;
				agent.MoveTowards(target2, speed * Time.deltaTime);
				yield return null;
			}
		}
	}

	public static Rigidbody ContinuesPatrolWaypointsWhile(this Rigidbody agent, List<Transform> waypoints, float speed, MonoBehaviour monoBehaviour, bool followWaypointOrder = true, Func<bool> condition = null)
	{
		if (agent == null || waypoints == null || waypoints.Count == 0)
		{
			return null;
		}
		monoBehaviour.StartCoroutine(PatrolWaypointsCoroutine());
		return agent;
		IEnumerator PatrolWaypointsCoroutine()
		{
			int currentWaypointIndex = 0;
			while (!(agent != null) || condition == null || condition())
			{
				Transform transform = waypoints[currentWaypointIndex];
				agent.MoveTowards(transform.position, speed * Time.deltaTime);
				if (agent.transform.HasReachedDestination(transform))
				{
					currentWaypointIndex = (followWaypointOrder ? ((currentWaypointIndex + 1) % waypoints.Count) : UnityEngine.Random.Range(0, waypoints.Count));
				}
				yield return null;
			}
		}
	}
}
