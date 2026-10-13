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
			Position = new Vector3(0f, 0f, 0f);
			Scale = new Vector3(0f, 0f, 0f);
		}

		public DialogTransformData(Vector3 position, Vector3 scale)
		{
			Position = position;
			Scale = scale;
		}
	}
}
