using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.Consumables;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Consumables
{
	[CreateAssetMenu(menuName = "Consumables/Localization")]
	public sealed class ConsumableLocalizationSOData : ScriptableObject
	{
		[field: SerializeField]
		public EConsumable Consumable { get; private set; }

		[field: SerializeField]
		public LocalizedString Name { get; private set; }

		[field: SerializeField]
		public LocalizedString GameplayDescription { get; private set; }

		[field: SerializeField]
		public LocalizedString NarrativeDescription { get; private set; }
	}
}
