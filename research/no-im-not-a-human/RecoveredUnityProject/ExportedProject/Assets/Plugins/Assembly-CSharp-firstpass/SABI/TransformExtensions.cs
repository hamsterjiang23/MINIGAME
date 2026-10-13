using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SABI
{
	public static class TransformExtensions
	{
		public static Transform ResetValues(this Transform transform)
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			return transform;
		}

		public static Transform SetLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localEulerAngles = transform.localEulerAngles;
			if (x.HasValue)
			{
				localEulerAngles.x = x.Value;
			}
			if (y.HasValue)
			{
				localEulerAngles.y = y.Value;
			}
			if (z.HasValue)
			{
				localEulerAngles.z = z.Value;
			}
			transform.localEulerAngles = localEulerAngles;
			return transform;
		}

		public static Transform AddLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localEulerAngles = transform.localEulerAngles;
			if (x.HasValue)
			{
				localEulerAngles.x += x.Value;
			}
			if (y.HasValue)
			{
				localEulerAngles.y += y.Value;
			}
			if (z.HasValue)
			{
				localEulerAngles.z += z.Value;
			}
			transform.localEulerAngles = localEulerAngles;
			return transform;
		}

		public static Transform SubtractLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localEulerAngles = transform.localEulerAngles;
			if (x.HasValue)
			{
				localEulerAngles.x -= x.Value;
			}
			if (y.HasValue)
			{
				localEulerAngles.y -= y.Value;
			}
			if (z.HasValue)
			{
				localEulerAngles.z -= z.Value;
			}
			transform.localEulerAngles = localEulerAngles;
			return transform;
		}

		public static Transform SetLocalEulerAngleX(this Transform transform, float x)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithX(x);
			return transform;
		}

		public static Transform SetLocalEulerAngleY(this Transform transform, float y)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithY(y);
			return transform;
		}

		public static Transform SetLocalEulerAngleZ(this Transform transform, float z)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithZ(z);
			return transform;
		}

		public static Transform AddLocalEulerAngleX(this Transform transform, float x)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithAddX(x);
			return transform;
		}

		public static Transform AddLocalEulerAngleY(this Transform transform, float y)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithAddY(y);
			return transform;
		}

		public static Transform AddLocalEulerAngleZ(this Transform transform, float z)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithAddZ(z);
			return transform;
		}

		public static Transform SubtractLocalEulerAngleX(this Transform transform, float x)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithSubtractX(x);
			return transform;
		}

		public static Transform SubtractLocalEulerAngleY(this Transform transform, float y)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithSubtractY(y);
			return transform;
		}

		public static Transform SubtractLocalEulerAngleZ(this Transform transform, float z)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithSubtractZ(z);
			return transform;
		}

		public static Transform SetEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 eulerAngles = transform.eulerAngles;
			if (x.HasValue)
			{
				eulerAngles.x = x.Value;
			}
			if (y.HasValue)
			{
				eulerAngles.y = y.Value;
			}
			if (z.HasValue)
			{
				eulerAngles.z = z.Value;
			}
			transform.eulerAngles = eulerAngles;
			return transform;
		}

		public static Transform AddEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 eulerAngles = transform.eulerAngles;
			if (x.HasValue)
			{
				eulerAngles.x += x.Value;
			}
			if (y.HasValue)
			{
				eulerAngles.y += y.Value;
			}
			if (z.HasValue)
			{
				eulerAngles.z += z.Value;
			}
			transform.eulerAngles = eulerAngles;
			return transform;
		}

		public static Transform SubtractEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 eulerAngles = transform.eulerAngles;
			if (x.HasValue)
			{
				eulerAngles.x -= x.Value;
			}
			if (y.HasValue)
			{
				eulerAngles.y -= y.Value;
			}
			if (z.HasValue)
			{
				eulerAngles.z -= z.Value;
			}
			transform.eulerAngles = eulerAngles;
			return transform;
		}

		public static Transform SetEulerAngleX(this Transform transform, float x)
		{
			transform.eulerAngles = transform.eulerAngles.WithX(x);
			return transform;
		}

		public static Transform SetEulerAngleY(this Transform transform, float y)
		{
			transform.eulerAngles = transform.eulerAngles.WithY(y);
			return transform;
		}

		public static Transform SetEulerAngleZ(this Transform transform, float z)
		{
			transform.eulerAngles = transform.eulerAngles.WithZ(z);
			return transform;
		}

		public static Transform AddEulerAngleX(this Transform transform, float x)
		{
			transform.eulerAngles = transform.eulerAngles.WithAddX(x);
			return transform;
		}

		public static Transform AddEulerAngleY(this Transform transform, float y)
		{
			transform.eulerAngles = transform.eulerAngles.WithAddY(y);
			return transform;
		}

		public static Transform AddEulerAngleZ(this Transform transform, float z)
		{
			transform.eulerAngles = transform.eulerAngles.WithAddZ(z);
			return transform;
		}

		public static Transform SubtractEulerAngleX(this Transform transform, float x)
		{
			transform.eulerAngles = transform.eulerAngles.WithSubtractX(x);
			return transform;
		}

		public static Transform SubtractEulerAngleY(this Transform transform, float y)
		{
			transform.eulerAngles = transform.eulerAngles.WithSubtractY(y);
			return transform;
		}

		public static Transform SubtractEulerAngleZ(this Transform transform, float z)
		{
			transform.eulerAngles = transform.eulerAngles.WithSubtractZ(z);
			return transform;
		}

		public static Transform SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 position = transform.position;
			if (x.HasValue)
			{
				position.x = x.Value;
			}
			if (y.HasValue)
			{
				position.y = y.Value;
			}
			if (z.HasValue)
			{
				position.z = z.Value;
			}
			transform.position = position;
			return transform;
		}

		public static Transform AddPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 position = transform.position;
			if (x.HasValue)
			{
				position.x += x.Value;
			}
			if (y.HasValue)
			{
				position.y += y.Value;
			}
			if (z.HasValue)
			{
				position.z += z.Value;
			}
			transform.position = position;
			return transform;
		}

		public static Transform SubtractPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 position = transform.position;
			if (x.HasValue)
			{
				position.x -= x.Value;
			}
			if (y.HasValue)
			{
				position.y -= y.Value;
			}
			if (z.HasValue)
			{
				position.z -= z.Value;
			}
			transform.position = position;
			return transform;
		}

		public static Transform SetPositionX(this Transform transform, float x)
		{
			transform.position = transform.position.WithX(x);
			return transform;
		}

		public static Transform SetPositionY(this Transform transform, float y)
		{
			transform.position = transform.position.WithY(y);
			return transform;
		}

		public static Transform SetPositionZ(this Transform transform, float z)
		{
			transform.position = transform.position.WithZ(z);
			return transform;
		}

		public static Transform SetPositionX(this Transform transform, Vector3 x)
		{
			transform.position = transform.position.WithX(x.x);
			return transform;
		}

		public static Transform SetPositionY(this Transform transform, Vector3 y)
		{
			transform.position = transform.position.WithY(y.y);
			return transform;
		}

		public static Transform SetPositionZ(this Transform transform, Vector3 z)
		{
			transform.position = transform.position.WithZ(z.z);
			return transform;
		}

		public static Transform SetPositionX(this Transform transform, Transform x)
		{
			transform.position = transform.position.WithX(x.position.x);
			return transform;
		}

		public static Transform SetPositionY(this Transform transform, Transform y)
		{
			transform.position = transform.position.WithY(y.position.y);
			return transform;
		}

		public static Transform SetPositionZ(this Transform transform, Transform z)
		{
			transform.position = transform.position.WithZ(z.position.z);
			return transform;
		}

		public static Transform SetLocalPositionX(this Transform transform, float x)
		{
			transform.localPosition = transform.localPosition.WithX(x);
			return transform;
		}

		public static Transform SetLocalPositionY(this Transform transform, float y)
		{
			transform.localPosition = transform.localPosition.WithY(y);
			return transform;
		}

		public static Transform SetLocalPositionZ(this Transform transform, float z)
		{
			transform.localPosition = transform.localPosition.WithZ(z);
			return transform;
		}

		public static Transform SetLocalPositionX(this Transform transform, Vector3 x)
		{
			transform.localPosition = transform.localPosition.WithX(x.x);
			return transform;
		}

		public static Transform SetLocalPositionY(this Transform transform, Vector3 y)
		{
			transform.localPosition = transform.localPosition.WithY(y.y);
			return transform;
		}

		public static Transform SetLocalPositionZ(this Transform transform, Vector3 z)
		{
			transform.localPosition = transform.localPosition.WithZ(z.z);
			return transform;
		}

		public static Transform SetLocalPositionX(this Transform transform, Transform x)
		{
			transform.localPosition = transform.localPosition.WithX(x.localPosition.x);
			return transform;
		}

		public static Transform SetLocalPositionY(this Transform transform, Transform y)
		{
			transform.localPosition = transform.localPosition.WithY(y.localPosition.y);
			return transform;
		}

		public static Transform SetLocalPositionZ(this Transform transform, Transform z)
		{
			transform.localPosition = transform.localPosition.WithZ(z.localPosition.z);
			return transform;
		}

		public static Transform SetEulerAnglesX(this Transform transform, float x)
		{
			transform.eulerAngles = transform.eulerAngles.WithX(x);
			return transform;
		}

		public static Transform SetEulerAnglesY(this Transform transform, float y)
		{
			transform.eulerAngles = transform.eulerAngles.WithY(y);
			return transform;
		}

		public static Transform SetEulerAnglesZ(this Transform transform, float z)
		{
			transform.eulerAngles = transform.eulerAngles.WithZ(z);
			return transform;
		}

		public static Transform SetEulerAnglesX(this Transform transform, Vector3 x)
		{
			transform.eulerAngles = transform.eulerAngles.WithX(x.x);
			return transform;
		}

		public static Transform SetEulerAnglesY(this Transform transform, Vector3 y)
		{
			transform.eulerAngles = transform.eulerAngles.WithY(y.y);
			return transform;
		}

		public static Transform SetEulerAnglesZ(this Transform transform, Vector3 z)
		{
			transform.eulerAngles = transform.eulerAngles.WithZ(z.z);
			return transform;
		}

		public static Transform SetEulerAnglesX(this Transform transform, Transform x)
		{
			transform.eulerAngles = transform.eulerAngles.WithX(x.eulerAngles.x);
			return transform;
		}

		public static Transform SetEulerAnglesY(this Transform transform, Transform y)
		{
			transform.eulerAngles = transform.eulerAngles.WithY(y.eulerAngles.y);
			return transform;
		}

		public static Transform SetEulerAnglesZ(this Transform transform, Transform z)
		{
			transform.eulerAngles = transform.eulerAngles.WithZ(z.eulerAngles.z);
			return transform;
		}

		public static Transform SetLocalEulerAnglesX(this Transform transform, float x)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithX(x);
			return transform;
		}

		public static Transform SetLocalEulerAnglesY(this Transform transform, float y)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithY(y);
			return transform;
		}

		public static Transform SetLocalEulerAnglesZ(this Transform transform, float z)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithZ(z);
			return transform;
		}

		public static Transform SetLocalEulerAnglesX(this Transform transform, Vector3 x)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithX(x.x);
			return transform;
		}

		public static Transform SetLocalEulerAnglesY(this Transform transform, Vector3 y)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithY(y.y);
			return transform;
		}

		public static Transform SetLocalEulerAnglesZ(this Transform transform, Vector3 z)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithZ(z.z);
			return transform;
		}

		public static Transform SetLocalEulerAnglesX(this Transform transform, Transform x)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithX(x.localEulerAngles.x);
			return transform;
		}

		public static Transform SetLocalEulerAnglesY(this Transform transform, Transform y)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithY(y.localEulerAngles.y);
			return transform;
		}

		public static Transform SetLocalEulerAnglesZ(this Transform transform, Transform z)
		{
			transform.localEulerAngles = transform.localEulerAngles.WithZ(z.localEulerAngles.z);
			return transform;
		}

		public static Transform SetLocalScale(this Transform transform, float scale)
		{
			transform.localScale = new Vector3(scale, scale, scale);
			return transform;
		}

		public static Transform SetLocalScaleX(this Transform transform, float x)
		{
			transform.localScale = transform.localScale.WithX(x);
			return transform;
		}

		public static Transform SetLocalScaleY(this Transform transform, float y)
		{
			transform.localScale = transform.localScale.WithY(y);
			return transform;
		}

		public static Transform SetLocalScaleZ(this Transform transform, float z)
		{
			transform.localScale = transform.localScale.WithZ(z);
			return transform;
		}

		public static Transform SetLocalScaleX(this Transform transform, Vector3 x)
		{
			transform.localScale = transform.localScale.WithX(x.x);
			return transform;
		}

		public static Transform SetLocalScaleY(this Transform transform, Vector3 y)
		{
			transform.localScale = transform.localScale.WithY(y.y);
			return transform;
		}

		public static Transform SetLocalScaleZ(this Transform transform, Vector3 z)
		{
			transform.localScale = transform.localScale.WithZ(z.z);
			return transform;
		}

		public static Transform SetLocalScaleX(this Transform transform, Transform x)
		{
			transform.localScale = transform.localScale.WithX(x.localScale.x);
			return transform;
		}

		public static Transform SetLocalScaleY(this Transform transform, Transform y)
		{
			transform.localScale = transform.localScale.WithY(y.localScale.y);
			return transform;
		}

		public static Transform SetLocalScaleZ(this Transform transform, Transform z)
		{
			transform.localScale = transform.localScale.WithZ(z.localScale.z);
			return transform;
		}

		public static Transform AddPositionX(this Transform transform, float x)
		{
			transform.position = transform.position.WithAddX(x);
			return transform;
		}

		public static Transform AddPositionY(this Transform transform, float y)
		{
			transform.position = transform.position.WithAddY(y);
			return transform;
		}

		public static Transform AddPositionZ(this Transform transform, float z)
		{
			transform.position = transform.position.WithAddZ(z);
			return transform;
		}

		public static Transform SubtractPositionX(this Transform transform, float x)
		{
			transform.position = transform.position.WithSubtractX(x);
			return transform;
		}

		public static Transform SubtractPositionY(this Transform transform, float y)
		{
			transform.position = transform.position.WithSubtractY(y);
			return transform;
		}

		public static Transform SubtractPositionZ(this Transform transform, float z)
		{
			transform.position = transform.position.WithSubtractZ(z);
			return transform;
		}

		public static Transform SetLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localPosition = transform.localPosition;
			if (x.HasValue)
			{
				localPosition.x = x.Value;
			}
			if (y.HasValue)
			{
				localPosition.y = y.Value;
			}
			if (z.HasValue)
			{
				localPosition.z = z.Value;
			}
			transform.localPosition = localPosition;
			return transform;
		}

		public static Transform AddLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localPosition = transform.localPosition;
			if (x.HasValue)
			{
				localPosition.x += x.Value;
			}
			if (y.HasValue)
			{
				localPosition.y += y.Value;
			}
			if (z.HasValue)
			{
				localPosition.z += z.Value;
			}
			transform.localPosition = localPosition;
			return transform;
		}

		public static Transform SubtractLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localPosition = transform.localPosition;
			if (x.HasValue)
			{
				localPosition.x -= x.Value;
			}
			if (y.HasValue)
			{
				localPosition.y -= y.Value;
			}
			if (z.HasValue)
			{
				localPosition.z -= z.Value;
			}
			transform.localPosition = localPosition;
			return transform;
		}

		public static Transform AddLocalPositionX(this Transform transform, float x)
		{
			transform.localPosition = transform.localPosition.WithAddX(x);
			return transform;
		}

		public static Transform AddLocalPositionY(this Transform transform, float y)
		{
			transform.localPosition = transform.localPosition.WithAddY(y);
			return transform;
		}

		public static Transform AddLocalPositionZ(this Transform transform, float z)
		{
			transform.localPosition = transform.localPosition.WithAddZ(z);
			return transform;
		}

		public static Transform SubtractLocalPositionX(this Transform transform, float x)
		{
			transform.localPosition = transform.localPosition.WithSubtractX(x);
			return transform;
		}

		public static Transform SubtractLocalPositionY(this Transform transform, float y)
		{
			transform.localPosition = transform.localPosition.WithSubtractY(y);
			return transform;
		}

		public static Transform SubtractLocalPositionZ(this Transform transform, float z)
		{
			transform.localPosition = transform.localPosition.WithSubtractZ(z);
			return transform;
		}

		public static Transform SetScale(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localScale = transform.localScale;
			if (x.HasValue)
			{
				localScale.x = x.Value;
			}
			if (y.HasValue)
			{
				localScale.y = y.Value;
			}
			if (z.HasValue)
			{
				localScale.z = z.Value;
			}
			transform.localScale = localScale;
			return transform;
		}

		public static Transform AddScale(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localScale = transform.localScale;
			if (x.HasValue)
			{
				localScale.x += x.Value;
			}
			if (y.HasValue)
			{
				localScale.y += y.Value;
			}
			if (z.HasValue)
			{
				localScale.z += z.Value;
			}
			transform.localScale = localScale;
			return transform;
		}

		public static Transform SubtractScale(this Transform transform, float? x = null, float? y = null, float? z = null)
		{
			Vector3 localScale = transform.localScale;
			if (x.HasValue)
			{
				localScale.x -= x.Value;
			}
			if (y.HasValue)
			{
				localScale.y -= y.Value;
			}
			if (z.HasValue)
			{
				localScale.z -= z.Value;
			}
			transform.localScale = localScale;
			return transform;
		}

		public static Transform SetScale(this Transform transform, float UniformScaleValue)
		{
			return transform.SetScale(UniformScaleValue, UniformScaleValue, UniformScaleValue);
		}

		public static Transform AddScale(this Transform transform, float UniformScaleValue)
		{
			return transform.AddScale(UniformScaleValue, UniformScaleValue, UniformScaleValue);
		}

		public static Transform SubtractScale(this Transform transform, float UniformScaleValue)
		{
			return transform.SubtractScale(UniformScaleValue, UniformScaleValue, UniformScaleValue);
		}

		public static Transform SetScaleX(this Transform transform, float x)
		{
			transform.localScale = transform.localScale.WithX(x);
			return transform;
		}

		public static Transform SetScaleY(this Transform transform, float y)
		{
			transform.localScale = transform.localScale.WithY(y);
			return transform;
		}

		public static Transform SetScaleZ(this Transform transform, float z)
		{
			transform.localScale = transform.localScale.WithZ(z);
			return transform;
		}

		public static Transform AddScaleX(this Transform transform, float x)
		{
			transform.localScale = transform.localScale.WithAddX(x);
			return transform;
		}

		public static Transform AddScaleY(this Transform transform, float y)
		{
			transform.localScale = transform.localScale.WithAddY(y);
			return transform;
		}

		public static Transform AddScaleZ(this Transform transform, float z)
		{
			transform.localScale = transform.localScale.WithAddZ(z);
			return transform;
		}

		public static Transform SubtractScaleX(this Transform transform, float x)
		{
			transform.localScale = transform.localScale.WithSubtractX(x);
			return transform;
		}

		public static Transform SubtractScaleY(this Transform transform, float y)
		{
			transform.localScale = transform.localScale.WithSubtractY(y);
			return transform;
		}

		public static Transform SubtractScaleZ(this Transform transform, float z)
		{
			transform.localScale = transform.localScale.WithSubtractZ(z);
			return transform;
		}

		public static Transform DestroyChildren(this Transform transform)
		{
			foreach (Transform item in transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			return transform;
		}

		public static Transform DetachChildren(this Transform transform)
		{
			foreach (Transform item in transform)
			{
				item.SetParent(null);
			}
			return transform;
		}

		public static Transform GetRandomChild(this Transform transform)
		{
			return transform.GetChild(UnityEngine.Random.Range(0, transform.childCount));
		}

		public static float Distance(this Transform transform, Transform target)
		{
			return Vector3.Distance(transform.position, target.position);
		}

		public static float Distance(this Transform transform, Vector3 target)
		{
			return Vector3.Distance(transform.position, target);
		}

		public static float DistanceWithoutHeight(this Transform transform, Transform target)
		{
			return Vector3.Distance(transform.position.WithY(0f), target.position.WithY(0f));
		}

		public static float DistanceWithoutHeight(this Transform transform, Vector3 target)
		{
			return Vector3.Distance(transform.position.WithY(0f), target.WithY(0f));
		}

		public static Vector3 DirectionTo(this Transform source, Transform target)
		{
			return (target.position - source.position).normalized;
		}

		public static Vector3 DirectionFrom(this Transform target, Transform source)
		{
			return (source.position - target.position).normalized;
		}

		public static Vector3 DirectionToIgnoringHeight(this Transform source, Transform target)
		{
			return (target.position.WithY(0f) - source.position.WithY(0f)).normalized;
		}

		public static Vector3 DirectionFromIgnoringHeight(this Transform target, Transform source)
		{
			return (source.position.WithY(0f) - target.position.WithY(0f)).normalized;
		}

		public static Vector3 DirectionTo(this Transform source, Vector3 target)
		{
			return (target - source.position).normalized;
		}

		public static Vector3 DirectionFrom(this Transform target, Vector3 source)
		{
			return (source - target.position).normalized;
		}

		public static Vector3 DirectionToIgnoringHeight(this Transform source, Vector3 target)
		{
			return (target.WithY(0f) - source.position.WithY(0f)).normalized;
		}

		public static Vector3 DirectionFromIgnoringHeight(this Transform target, Vector3 source)
		{
			return (source.WithY(0f) - target.position.WithY(0f)).normalized;
		}

		public static Vector3 back(this Transform v)
		{
			return -v.forward;
		}

		public static Vector3 left(this Transform v)
		{
			return -v.right;
		}

		public static Vector3 down(this Transform v)
		{
			return -v.up;
		}

		public static Transform MoveTowards(this Transform source, Transform target, float speed)
		{
			source.position = Vector3.MoveTowards(source.position, target.position, speed);
			return source;
		}

		public static Transform MoveTowards(this Transform source, Vector3 target, float speed)
		{
			source.position = Vector3.MoveTowards(source.position, target, speed);
			return source;
		}

		public static Transform ContinuesChaseTargetWhile(this Transform agent, Transform target, MonoBehaviour monoBehaviour, float speed = 5f, float? minDistanceKeep = null, float? maxDistanceKeep = null, float? delayBetweenSettingDestination = null, Func<bool> loopCondition = null, Func<float> distanceToPlayer = null)
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

		public static bool HasReachedDestination(this Transform agent, Transform destination, float tolerence = 0.1f)
		{
			return agent.Distance(destination) < tolerence;
		}

		public static bool HasReachedDestination(this Transform agent, Vector3 destination, float tolerence = 0.1f)
		{
			return agent.Distance(destination) < tolerence;
		}

		public static bool SetRandomDestination(this Transform agent, out Vector3 randomLocation, float radius, float speed, Vector3? origin = null)
		{
			Vector3 vector = UnityEngine.Random.insideUnitSphere * radius;
			vector = (randomLocation = vector + (origin ?? agent.transform.position));
			agent.MoveTowards(randomLocation, speed);
			return true;
		}

		public static bool SetRandomDestination(this Transform agent, float radius, float speed, Vector3? origin = null)
		{
			Vector3 target = UnityEngine.Random.insideUnitSphere * radius + (origin ?? agent.transform.position);
			agent.MoveTowards(target, speed);
			return true;
		}

		public static Transform Wander(this Transform agent, float radius, MonoBehaviour monoBehaviour, float speed, bool isContinues = true, float waitTime = 1f, Func<bool> condition = null, bool useSameHeight = true)
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
					if (agent.HasReachedDestination(randomLocation))
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

		public static Transform ContinuesFleeFromTargetWhile(this Transform agent, Transform target, MonoBehaviour monoBehaviour, float speed, float fleeDistance = 10f, Func<bool> condition = null)
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

		public static Transform ContinuesPatrolWaypointsWhile(this Transform agent, List<Transform> waypoints, float speed, MonoBehaviour monoBehaviour, bool followWaypointOrder = true, Func<bool> condition = null)
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
					if (agent.HasReachedDestination(transform))
					{
						currentWaypointIndex = (followWaypointOrder ? ((currentWaypointIndex + 1) % waypoints.Count) : UnityEngine.Random.Range(0, waypoints.Count));
					}
					yield return null;
				}
			}
		}

		public static Transform LookAtIgnoringY(this Transform source, Transform target)
		{
			source.LookAt(target.position.WithPosY(source));
			return source;
		}

		public static Transform LookAtIgnoringY(this Transform source, Vector3 target)
		{
			source.LookAt(target.WithPosY(source));
			return source;
		}

		public static float Dot(this Transform v, Transform target)
		{
			return Vector3.Dot(v.forward, (target.position.WithY(0f) - v.position.WithY(0f)).normalized);
		}

		public static float Dot(this Transform v, Vector3 target)
		{
			return Vector3.Dot(v.forward, (target.WithY(0f) - v.position.WithY(0f)).normalized);
		}

		public static Vector3 Cross(this Transform v, Transform target)
		{
			return Vector3.Cross(v.forward, (target.position.WithY(0f) - v.position.WithY(0f)).normalized);
		}

		public static Vector3 Cross(this Transform v, Vector3 target)
		{
			return Vector3.Cross(v.forward, (target.WithY(0f) - v.position.WithY(0f)).normalized);
		}

		public static Vector3 WorldSpacePositionToLocalSpace(this Transform transform, Vector3 position)
		{
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse.MultiplyPoint3x4(position);
		}

		public static Vector3 LocalSpacePositionToWorldSpace(this Transform transform, Vector3 position)
		{
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).MultiplyPoint3x4(position);
		}
	}
}
