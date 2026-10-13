using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Utils.Attributes.MinMaxRange;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure.OtherGameData;

[TabsNames(new string[] { "RoomsData", "CloseUps", "Screamers", "Consumables", "FEMA", "Dreams", "Phone" })]
[CreateAssetMenu(menuName = "Other Game Data")]
public sealed class OtherGameSOData : DataClass
{
	[Header("Bedroom")]
	[TabIndex(0)]
	[SerializeField]
	private int[] _tvReportageCountByDay;

	[Header("Entrance")]
	[TabIndex(0)]
	[MinMaxRange(0f, 60f)]
	[SerializeField]
	private Vector2 _startKnockTime = new Vector2(7f, 12f);

	[TabIndex(0)]
	[MinMaxRange(0f, 60f)]
	[SerializeField]
	private Vector2 _endKnockTime = new Vector2(4f, 8f);

	[TabIndex(0)]
	[Range(0f, 120f)]
	[SerializeField]
	private float _knockTimeProgressDuration = 30f;

	[Header("Radio")]
	[TabIndex(1)]
	[SerializeField]
	private float _radioDelta;

	[TabIndex(1)]
	[SerializeField]
	private float _radioThresholdStart;

	[TabIndex(1)]
	[SerializeField]
	private float _wrongStateMultiplier;

	[TabIndex(2)]
	[Range(0f, 1f)]
	[SerializeField]
	private float _screamerOnImposterChance;

	[TabIndex(3)]
	[SerializeField]
	[MinMaxRange(0f, 10f)]
	private Vector2Int _daysRange;

	[TabIndex(3)]
	[SerializeField]
	private SerializedDictionary<EConsumable, int> _deficitConsumables;

	[Header("Favorite Rating")]
	[TabIndex(4)]
	[SerializeField]
	[Range(0f, 1f)]
	private float _badFemaRatingChance;

	[TabIndex(4)]
	[SerializeField]
	[Range(-1f, 1f)]
	private float _takeGunAfterCheck;

	[TabIndex(4)]
	[SerializeField]
	[Range(-1f, 1f)]
	private float _notTakeGunAfterCheck;

	[TabIndex(4)]
	[SerializeField]
	[Range(-1f, 1f)]
	private float _isHuman;

	[TabIndex(4)]
	[SerializeField]
	[Range(-1f, 1f)]
	private float _listenDialogFirstTime;

	[TabIndex(4)]
	[SerializeField]
	[Range(-1f, 1f)]
	private float _listenDialogRepeat;

	[TabIndex(4)]
	[SerializeField]
	[Range(-1f, 1f)]
	private float _everyDayIncrement;

	[TabIndex(5)]
	[SerializeField]
	private DreamData[] _dreams;

	[TabIndex(6)]
	[SerializeField]
	private SerializedDictionary<EPhoneSubscriber, string> _phoneNumbers;

	public int[] TvReportageCountByDay => _tvReportageCountByDay;

	public float RadioDelta => _radioDelta;

	public float RadioThresholdStart => _radioThresholdStart;

	public float WrongStateMultiplier => _wrongStateMultiplier;

	public float ScreamerOnImposterChance => _screamerOnImposterChance;

	public float StartKnockTimeMin => _startKnockTime.x;

	public float StartKnockTimeMax => _startKnockTime.y;

	public float EndKnockTimeMin => _endKnockTime.x;

	public float EndKnockTimeMax => _endKnockTime.y;

	public float KnockTimeProgressDuration => _knockTimeProgressDuration;

	public int GetDeficitRandomDaysCount => Random.Range(_daysRange.x, _daysRange.y);

	public Dictionary<EConsumable, int> DeficitConsumables => _deficitConsumables;

	public float TakeGunAfterCheckRatingValue => _takeGunAfterCheck;

	public float DontTakeGunAfterCheckRatingValue => _notTakeGunAfterCheck;

	public float IsHumanRatingValue => _isHuman;

	public float ListenDialogFirstTimeRatingValue => _listenDialogFirstTime;

	public float ListenDialogRepeatRatingValue => _listenDialogRepeat;

	public float EveryDayIncrementRatingValue => _everyDayIncrement;

	public float BadFemaRatingChance => _badFemaRatingChance;

	public DreamData[] Dreams => _dreams;

	public Dictionary<EPhoneSubscriber, string> PhoneNumbers => _phoneNumbers;
}
