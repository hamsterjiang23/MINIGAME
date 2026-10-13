using UnityEngine;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Consumables;

[CreateAssetMenu(menuName = "Consumables/LocalizationsList")]
public sealed class ConsumableLocalizationsListSOData : ScriptableObject
{
	[field: SerializeField]
	public ConsumableLocalizationSOData[] Consumables { get; private set; }
}
