using System;

namespace _Code.Utils.EditorWindows
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class TabIndexAttribute : Attribute
	{
		public readonly int TabIndex;

		public TabIndexAttribute(int tabIndex)
		{
			TabIndex = tabIndex;
		}
	}
}
