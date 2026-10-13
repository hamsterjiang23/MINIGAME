using UnityEngine;
using UnityEngine.Video;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure._NINAH__MainMenu.Gacha;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure.Endings.View
{
	[CreateAssetMenu(menuName = "Ending")]
	[TabsNames(new string[] { "Video", "[Old] Slides", "Gacha" })]
	public sealed class EndingViewSOData : DataClass
	{
		[TabIndex(0)]
		[SerializeField]
		private EEnding _endingType;

		[TabIndex(0)]
		[SerializeField]
		private VideoClip _videoClip;

		[TabIndex(0)]
		[SerializeField]
		private EndingSubtitlesData _subtitles;

		[TabIndex(1)]
		[SerializeField]
		private ESound _music;

		[TabIndex(1)]
		[SerializeField]
		private EndingViewDataSlide[] _slides;

		[TabIndex(2)]
		[SerializeField]
		private GachaEndingData _gachaData;

		public EEnding EndingType => EEnding.TestEnding;

		public ESound Music => ESound.None;

		public EndingViewDataSlide[] Slides => null;

		public VideoClip VideoClip => null;

		public EndingSubtitlesData Subtitles => null;

		public GachaEndingData GachaData => null;
	}
}
