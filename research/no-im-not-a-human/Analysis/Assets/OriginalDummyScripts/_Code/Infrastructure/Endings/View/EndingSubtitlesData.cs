using System;
using UnityEngine;

namespace _Code.Infrastructure.Endings.View
{
	[Serializable]
	public sealed class EndingSubtitlesData
	{
		[SerializeField]
		private EndingSubtitleData[] _endingSubtitles;

		public EndingSubtitleData[] EndingSubtitles => null;
	}
}
