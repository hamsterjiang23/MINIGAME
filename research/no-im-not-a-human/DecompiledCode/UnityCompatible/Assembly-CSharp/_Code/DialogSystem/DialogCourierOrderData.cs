using System;
using Newtonsoft.Json;
using _Code.Infrastructure.Consumables;

namespace _Code.DialogSystem
{
	[Serializable]
	public sealed class DialogCourierOrderData
	{
		[JsonProperty]
		public EConsumable Consumable { get; private set; }

		[JsonProperty]
		public int Count { get; private set; }

		public DialogCourierOrderData(EConsumable consumable, int count)
		{
			Consumable = consumable;
			Count = count;
		}

		public DialogCourierOrderData()
		{
		}
	}
}
