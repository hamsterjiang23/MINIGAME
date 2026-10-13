using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Code.Events
{
	[Serializable]
	public sealed class VisualsByTimeOfDay
	{
		[field: SerializeField]
		public ETimeOfDay TimeOfDay { get; private set; }

		[field: SerializeField]
		public VolumeProfile VolumeProfile { get; private set; }

		[field: SerializeField]
		public bool HasLight { get; private set; }

		[field: SerializeField]
		public bool HasActionsCount { get; private set; }
	}
}
