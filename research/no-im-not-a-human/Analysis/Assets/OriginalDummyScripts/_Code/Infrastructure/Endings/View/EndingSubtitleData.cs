using System;
using UnityEngine;

namespace _Code.Infrastructure.Endings.View
{
	[Serializable]
	public sealed class EndingSubtitleData
	{
		[SerializeField]
		private string _nodeName;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private float _beforeNodeDelay;

		public string Node => null;

		public float Duration => 0f;

		public float BeforeNodeDelay => 0f;
	}
}
