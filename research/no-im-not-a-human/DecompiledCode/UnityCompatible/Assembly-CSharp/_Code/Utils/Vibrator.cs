using System;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace _Code.Utils
{
	public static class Vibrator
	{
		private static bool _isEnabled = true;

		public static async UniTask VibrateForTime(float duration, float lowStrength, float highStrength)
		{
			if (Gamepad.current != null && _isEnabled)
			{
				StartVibrate(lowStrength, highStrength);
				await UniTask.Delay(TimeSpan.FromSeconds(duration));
				StopVibrate();
			}
		}

		public static void StartVibrate(float lowStrength, float highStrength)
		{
			if (Gamepad.current != null && _isEnabled)
			{
				Gamepad.current.SetMotorSpeeds(lowStrength, highStrength);
			}
		}

		public static void StopVibrate()
		{
			if (Gamepad.current != null && _isEnabled)
			{
				Gamepad.current.SetMotorSpeeds(0f, 0f);
			}
		}

		public static void SetEnabledState(bool value)
		{
			_isEnabled = value;
		}
	}
}
