using UnityEngine;
using _Code.Events;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure;

[TabsNames(new string[] { "Changeable Objects Sources", "By Day Data", "Special days", "Other" })]
public sealed class DayNightControllerSOData : DataClass
{
	[TabIndex(0)]
	[SerializeField]
	private VisualsByTimeOfDay[] _volumeProfiles;

	[TabIndex(0)]
	[SerializeField]
	private Material _lightBeamNightMaterial;

	[TabIndex(0)]
	[SerializeField]
	private Material _lightBeamDayMaterial;

	[TabIndex(0)]
	[SerializeField]
	private Material _fullscreenEffectMaterial;

	[TabIndex(0)]
	[SerializeField]
	private Material _windowMaterial;

	[TabIndex(1)]
	[SerializeField]
	private int[] _maxDayActionsByDays;

	[TabIndex(1)]
	[SerializeField]
	private float[] _lensDistortionByDays;

	[TabIndex(1)]
	[SerializeField]
	private int[] _colorsCountByDays;

	[TabIndex(1)]
	[SerializeField]
	private int _lastDay;

	[TabIndex(2)]
	[SerializeField]
	private int _babyEndingDay;

	[TabIndex(2)]
	[SerializeField]
	private int _killerEndingDay;

	[TabIndex(2)]
	[SerializeField]
	private int _femaEndingDay;

	[TabIndex(3)]
	[SerializeField]
	private int _coffeeBoostDays;

	public VisualsByTimeOfDay[] VolumeProfiles => _volumeProfiles;

	public Material LightBeamNightMaterial => _lightBeamNightMaterial;

	public Material LightBeamDayMaterial => _lightBeamDayMaterial;

	public Material FullscreenEffectMaterial => _fullscreenEffectMaterial;

	public Material WindowMaterial => _windowMaterial;

	public int[] MaxDayActionsByDays => _maxDayActionsByDays;

	public int LastDay => _lastDay;

	public int BabyEndingDay => _babyEndingDay;

	public int KillerEndingDay => _killerEndingDay;

	public int FemaEndingDay => _femaEndingDay;

	public int CoffeeBoostDays => _coffeeBoostDays;
}
