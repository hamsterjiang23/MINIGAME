using System;
using UnityEngine;

namespace _Code.Characters
{
	[Serializable]
	public sealed class DialogTransformData
	{
		[field: SerializeField]
		public Vector3 Position { get; set; }

		[field: SerializeField]
		public Vector3 Scale { get; set; }

		public DialogTransformData()
		{
		}

		public DialogTransformData(Vector3 position, Vector3 scale)
		{
		}
	}
}
