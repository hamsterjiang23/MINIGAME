using UnityEngine;

namespace _Code.DialogSystem
{
	public sealed class DialogData
	{
		public string SpeakerName { get; }

		public Sprite SpeakerSprite { get; }

		public string Speech { get; }

		public DialogData(string speakerName, Sprite speakerSprite, string speech)
		{
		}
	}
}
