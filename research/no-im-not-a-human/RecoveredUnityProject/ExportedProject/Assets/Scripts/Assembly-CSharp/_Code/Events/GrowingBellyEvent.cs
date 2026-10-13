using System;

namespace _Code.Events
{
	[Serializable]
	public sealed class GrowingBellyEvent : AGameEvent
	{
		public GrowingBellyEvent(GameEventConditions conditions)
		{
			Condition = conditions;
		}
	}
}
