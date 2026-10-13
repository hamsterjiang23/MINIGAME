using System;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.Sound;

namespace _Code.Infrastructure._NINAH__Dream
{
	[Serializable]
	public sealed class DreamData
	{
		[field: SerializeField]
		public EDream Dream { get; private set; }

		[field: SerializeField]
		public int SceneIndex { get; private set; }

		[field: SerializeField]
		public bool HasMusic { get; private set; }

		[field: SerializeField]
		[field: SearchableEnum]
		public ESound Music { get; private set; }
	}
}
