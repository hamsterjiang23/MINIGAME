using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Utils.Attributes.MinMaxRange;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure.OtherGameData
{
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
		private Vector2 _startKnockTime;

		[TabIndex(0)]
		[MinMaxRange(0f, 60f)]
		[SerializeField]
		private Vector2 _endKnockTime;

		[TabIndex(0)]
		[Range(0f, 120f)]
		[SerializeField]
		private float _knockTimeProgressDuration;

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

		public int[] TvReportageCountByDay => null;

		public float RadioDelta => 0f;

		public float RadioThresholdStart => 0f;

		public float WrongStateMultiplier => 0f;

		public float ScreamerOnImposterChance => 0f;

		public float StartKnockTimeMin => 0f;

		public float StartKnockTimeMax => 0f;

		public float EndKnockTimeMin => 0f;

		public float EndKnockTimeMax => 0f;

		public float KnockTimeProgressDuration => 0f;

		public int GetDeficitRandomDaysCount => 0;

		public Dictionary<EConsumable, int> DeficitConsumables => null;

		public float TakeGunAfterCheckRatingValue => 0f;

		public float DontTakeGunAfterCheckRatingValue => 0f;

		public float IsHumanRatingValue => 0f;

		public float ListenDialogFirstTimeRatingValue => 0f;

		public float ListenDialogRepeatRatingValue => 0f;

		public float EveryDayIncrementRatingValue => 0f;

		public float BadFemaRatingChance => 0f;

		public DreamData[] Dreams => null;

		public Dictionary<EPhoneSubscriber, string> PhoneNumbers => null;
	}
}
