using System;

namespace _Code.Utils.EditorWindows
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TabsNamesAttribute : Attribute
	{
		public string[] TabsNames { get; }

		public TabsNamesAttribute(params string[] tabsNames)
		{
		}
	}
}
