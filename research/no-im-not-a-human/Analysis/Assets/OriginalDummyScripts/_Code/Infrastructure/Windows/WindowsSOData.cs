using UnityEngine;
using _Code.Rooms;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure.Windows
{
	[TabsNames(new string[] { "Blinds bedroom", "Blinds pantry", "Curtains" })]
	public sealed class WindowsSOData : DataClass
	{
		[TabIndex(0)]
		[SerializeField]
		private WindowDayImageData[] _blindsBedroomData;

		[TabIndex(1)]
		[SerializeField]
		private WindowDayImageData[] _blindsPantryData;

		[TabIndex(2)]
		[SerializeField]
		private WindowDayImageData[] _curtainsData;

		public WindowDayImageData[] BlindsBedroomData => null;

		public WindowDayImageData[] BlindsPantryData => null;

		public WindowDayImageData[] CurtainsData => null;
	}
}
