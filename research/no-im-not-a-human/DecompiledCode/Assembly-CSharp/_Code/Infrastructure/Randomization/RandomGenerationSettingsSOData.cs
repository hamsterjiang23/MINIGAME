using UnityEngine;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure.Randomization;

[CreateAssetMenu(menuName = "RandomGenerationSettingsSO")]
[TabsNames(new string[] { "Settings" })]
public sealed class RandomGenerationSettingsSOData : DataClass
{
	[TabIndex(0)]
	[SerializeField]
	private int[] _randomGuestsNumberByNights;

	[TabIndex(0)]
	[Range(0f, 1f)]
	[SerializeField]
	private float _minImpostersPercent;

	[TabIndex(0)]
	[Range(0f, 1f)]
	[SerializeField]
	private float _maxImpostersPercent;

	public int[] RandomGuestsNumberByNights => _randomGuestsNumberByNights;

	public float MinImpostersPercent => _minImpostersPercent;

	public float MaxImpostersPercent => _maxImpostersPercent;
}
