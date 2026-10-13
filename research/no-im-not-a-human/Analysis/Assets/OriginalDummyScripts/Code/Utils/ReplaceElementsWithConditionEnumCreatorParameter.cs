using System;

namespace Code.Utils
{
	public sealed class ReplaceElementsWithConditionEnumCreatorParameter : IEnumCreatorParameter
	{
		public Func<string, bool> Condition { get; }

		public ReplaceElementsWithConditionEnumCreatorParameter(Func<string, bool> condition)
		{
		}
	}
}
