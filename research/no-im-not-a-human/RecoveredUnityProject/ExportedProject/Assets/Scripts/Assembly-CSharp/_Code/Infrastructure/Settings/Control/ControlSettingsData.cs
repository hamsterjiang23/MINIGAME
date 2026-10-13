using System;
using UnityEngine;

namespace _Code.Infrastructure.Settings.Control
{
	[Serializable]
	public sealed class ControlSettingsData : ASettingsData
	{
		[field: SerializeField]
		public bool GamepadVibration { get; set; } = true;

		[field: SerializeField]
		public float MouseSensitivity { get; set; } = 1f;

		[field: SerializeField]
		public float GamepadSensitivity { get; set; } = 1f;

		[field: SerializeField]
		public float GamepadRoomSensitivity { get; set; } = 1f;

		[field: SerializeField]
		public bool InvertMouseY { get; set; }

		[field: SerializeField]
		public bool InvertGamepadY { get; set; }
	}
}
