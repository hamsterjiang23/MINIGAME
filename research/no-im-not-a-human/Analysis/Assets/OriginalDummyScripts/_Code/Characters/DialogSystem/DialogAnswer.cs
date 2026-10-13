using System;
using UnityEngine;

namespace _Code.Characters.DialogSystem
{
	[Serializable]
	public sealed class DialogAnswer
	{
		[field: SerializeField]
		public string Text { get; private set; }

		[field: SerializeField]
		public Action Action { get; private set; }

		public DialogAnswer(string text, Action action)
		{
		}
	}
}
