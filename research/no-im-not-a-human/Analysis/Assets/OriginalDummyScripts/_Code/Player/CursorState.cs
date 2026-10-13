using System;

namespace _Code.Player
{
	[Serializable]
	public sealed class CursorState
	{
		public bool EnabledForGamepad;

		public bool EnabledForKeyboardAndMouse;

		public CursorState(bool enabledForGamepad, bool enabledForKeyboardAndMouse)
		{
		}
	}
}
