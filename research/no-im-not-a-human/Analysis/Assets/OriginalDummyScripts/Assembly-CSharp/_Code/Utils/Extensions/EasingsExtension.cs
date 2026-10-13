using System;
using DG.Tweening;
using UnityEngine;

namespace _Code.Utils.Extensions;

public static class EasingsExtension
{
	public static float Ease(this float value, Ease ease)
	{
		switch (ease)
		{
		case DG.Tweening.Ease.InSine:
			return 1f - Mathf.Cos(value * (float)Math.PI / 2f);
		case DG.Tweening.Ease.OutSine:
			return Mathf.Sin(value * (float)Math.PI / 2f);
		case DG.Tweening.Ease.InOutSine:
			return (0f - (Mathf.Cos((float)Math.PI * value) - 1f)) / 2f;
		case DG.Tweening.Ease.InQuad:
			return value * value;
		case DG.Tweening.Ease.OutQuad:
			return 1f - (1f - value) * (1f - value);
		case DG.Tweening.Ease.InOutQuad:
			if (!((double)value < 0.5))
			{
				return 1f - Mathf.Pow(-2f * value + 2f, 2f) / 2f;
			}
			return 2f * value * value;
		case DG.Tweening.Ease.InCubic:
			return value * value * value;
		case DG.Tweening.Ease.OutCubic:
			return 1f - Mathf.Pow(1f - value, 3f);
		case DG.Tweening.Ease.InOutCubic:
			if (!((double)value < 0.5))
			{
				return 1f - Mathf.Pow(-2f * value + 2f, 3f) / 2f;
			}
			return 4f * value * value * value;
		case DG.Tweening.Ease.InQuart:
			return value * value * value * value;
		case DG.Tweening.Ease.OutQuart:
			return 1f - Mathf.Pow(1f - value, 4f);
		case DG.Tweening.Ease.InOutQuart:
			if (!((double)value < 0.5))
			{
				return 1f - Mathf.Pow(-2f * value + 2f, 4f) / 2f;
			}
			return 8f * value * value * value * value;
		case DG.Tweening.Ease.InQuint:
			return value * value * value * value * value;
		case DG.Tweening.Ease.OutQuint:
			return 1f - Mathf.Pow(1f - value, 5f);
		case DG.Tweening.Ease.InOutQuint:
			if (!((double)value < 0.5))
			{
				return 1f - Mathf.Pow(-2f * value + 2f, 5f) / 2f;
			}
			return 16f * value * value * value * value * value;
		case DG.Tweening.Ease.InExpo:
			if (value != 0f)
			{
				return Mathf.Pow(2f, 10f * value - 10f);
			}
			return 0f;
		case DG.Tweening.Ease.OutExpo:
			if (!Mathf.Approximately(value, 1f))
			{
				return 1f - Mathf.Pow(2f, -10f * value);
			}
			return 1f;
		case DG.Tweening.Ease.InOutExpo:
			if (value != 0f)
			{
				if (!Mathf.Approximately(value, 1f))
				{
					if (!((double)value < 0.5))
					{
						return (2f - Mathf.Pow(2f, -20f * value + 10f)) / 2f;
					}
					return Mathf.Pow(2f, 20f * value - 10f) / 2f;
				}
				return 1f;
			}
			return 0f;
		case DG.Tweening.Ease.InCirc:
			return 1f - Mathf.Sqrt(1f - Mathf.Pow(value, 2f));
		case DG.Tweening.Ease.OutCirc:
			return Mathf.Sqrt(1f - Mathf.Pow(value - 1f, 2f));
		case DG.Tweening.Ease.InOutCirc:
			if (!((double)value < 0.5))
			{
				return (Mathf.Sqrt(1f - Mathf.Pow(-2f * value + 2f, 2f)) + 1f) / 2f;
			}
			return (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * value, 2f))) / 2f;
		case DG.Tweening.Ease.InBack:
		{
			float num5 = 1.70158f;
			return (num5 + 1f) * value * value * value - num5 * value * value;
		}
		case DG.Tweening.Ease.OutBack:
		{
			float num = 1.70158f;
			float num2 = num + 1f;
			return 1f + num2 * Mathf.Pow(value - 1f, 3f) + num * Mathf.Pow(value - 1f, 2f);
		}
		case DG.Tweening.Ease.InOutBack:
		{
			float num9 = 1.70158f * 1.525f;
			if (!((double)value < 0.5))
			{
				return (Mathf.Pow(2f * value - 2f, 2f) * ((num9 + 1f) * (value * 2f - 2f) + num9) + 2f) / 2f;
			}
			return Mathf.Pow(2f * value, 2f) * ((num9 + 1f) * 2f * value - num9) / 2f;
		}
		case DG.Tweening.Ease.InElastic:
		{
			float num8 = (float)Math.PI * 2f / 3f;
			if (value != 0f)
			{
				if (!Mathf.Approximately(value, 1f))
				{
					return (0f - Mathf.Pow(2f, 10f * value - 10f)) * Mathf.Sin((value * 10f - 10.75f) * num8);
				}
				return 1f;
			}
			return 0f;
		}
		case DG.Tweening.Ease.OutElastic:
		{
			float num7 = (float)Math.PI * 2f / 3f;
			if (value != 0f)
			{
				if (!Mathf.Approximately(value, 1f))
				{
					return Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * 10f - 0.75f) * num7) + 1f;
				}
				return 1f;
			}
			return 0f;
		}
		case DG.Tweening.Ease.InOutElastic:
		{
			float num6 = (float)Math.PI * 4f / 9f;
			if (value != 0f)
			{
				if (!Mathf.Approximately(value, 1f))
				{
					if (!((double)value < 0.5))
					{
						return Mathf.Pow(2f, -20f * value + 10f) * Mathf.Sin((20f * value - 11.125f) * num6) / 2f + 1f;
					}
					return (0f - Mathf.Pow(2f, 20f * value - 10f) * Mathf.Sin((20f * value - 11.125f) * num6)) / 2f;
				}
				return 1f;
			}
			return 0f;
		}
		case DG.Tweening.Ease.InBounce:
			return 1f - (1f - value).Ease(DG.Tweening.Ease.OutBounce);
		case DG.Tweening.Ease.OutBounce:
		{
			float num3 = 7.5625f;
			float num4 = 2.75f;
			if (value < 1f / num4)
			{
				return num3 * value * value;
			}
			if (value < 2f / num4)
			{
				return num3 * (value -= 1.5f / num4) * value + 0.75f;
			}
			if ((double)value < 2.5 / (double)num4)
			{
				return num3 * (value -= 2.25f / num4) * value + 0.9375f;
			}
			return num3 * (value -= 2.625f / num4) * value + 63f / 64f;
		}
		case DG.Tweening.Ease.InOutBounce:
			if (!((double)value < 0.5))
			{
				return (1f + (2f * value - 1f).Ease(DG.Tweening.Ease.OutBounce)) / 2f;
			}
			return (1f - (1f - 2f * value).Ease(DG.Tweening.Ease.OutBounce)) / 2f;
		case DG.Tweening.Ease.Linear:
			return value;
		default:
			Debug.LogWarning($"There is no ease function for {ease}");
			return value;
		}
	}
}
