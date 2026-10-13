using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.Sound;
using _Code.Rooms;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Characters
{
	[CreateAssetMenu(menuName = "Character")]
	public class CharacterSOData : ScriptableObject
	{
		[SerializeField]
		[SearchableEnum]
		private ECharacterType _characterType;

		[SerializeField]
		private RoomObjectState<ERoomPeopleState>[] _poses;

		[SerializeReference]
		[SerializeField]
		private ACharacterSpriteByEmotion[] _emotions;

		[SerializeField]
		private SerializedDictionary<EDialogEmotionState, DialogTransformData> _emotionOverrides;

		[SerializeField]
		[SearchableEnum]
		private ERoom _room;

		[SerializeField]
		private LocalizedString _nameLocalizationKey;

		[SerializeField]
		private Texture2D _corpseMask;

		[SerializeField]
		private Sprite _handsSpriteHuman;

		[SerializeField]
		private CharacterEyeData _eyeSpriteHuman;

		[SerializeField]
		private Sprite _teethSpriteHuman;

		[SerializeField]
		private AnimationData _earHuman;

		[SerializeField]
		private AnimationData _armpitHuman;

		[SerializeField]
		private Sprite _photoHuman;

		[SerializeField]
		private Sprite _handsSpriteImposter;

		[SerializeField]
		private CharacterEyeData _eyeSpriteImposter;

		[SerializeField]
		private Sprite _teethSpriteImposter;

		[SerializeField]
		private AnimationData _earImposter;

		[SerializeField]
		private AnimationData _armpitImposter;

		[SerializeField]
		private Sprite _photoImposter;

		[SerializeField]
		[SearchableEnum]
		private ESound _knockSound;

		[SerializeField]
		[SearchableEnum]
		private ESound _entranceTheme;

		[SerializeField]
		private AudioClip _nightRoomSound;

		[SerializeField]
		private bool _isImposter;

		[SerializeField]
		private float _entranceScale;

		[SerializeField]
		private float _roomScale;

		[SerializeField]
		private bool _canExile;

		[SerializeField]
		private bool _isPreset;

		[SerializeField]
		[SearchableEnum]
		private ECharacterPlace _place;

		[SerializeField]
		private bool _isStatusRandomlyGenerated;

		[Tooltip("When the character can come, list of days and number of guest for this day")]
		[SerializeField]
		private List<CharacterVisitData> _visits;

		[Tooltip("In which days the character can come if they're randomly generated")]
		[SerializeField]
		private int[] _allowedDays;

		[SerializeField]
		private int _minimumGamesCompletedToAppear;

		[SerializeField]
		private LocalizedString _quote;

		[SerializeField]
		private Vector2 _gachaPosBias;

		[SerializeField]
		private Vector2 _gachaScaleBias;

		[SerializeField]
		[Range(-1f, 1f)]
		private float _FEMARatingBonus;

		public ECharacterType CharacterType => ECharacterType.Sanya;

		public RoomObjectState<ERoomPeopleState>[] Poses => null;

		public SerializedDictionary<EDialogEmotionState, DialogTransformData> EmotionOverrides => null;

		public ERoom Room => ERoom.Kitchen;

		public Sprite HandsSprite => null;

		public CharacterEyeData EyeSprite => null;

		public Sprite TeethSprite => null;

		public AnimationData ArmpitSprite => null;

		public AnimationData EarSprite => null;

		public Sprite PhotoSprite => null;

		public bool IsImposter
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public float EntranceScale => 0f;

		public float RoomScale => 0f;

		public bool IsPreset => false;

		public ECharacterPlace Place => ECharacterPlace.None;

		public ESound KnockSound => ESound.None;

		public bool IsStatusRandomlyGenerated => false;

		public IReadOnlyList<CharacterVisitData> Visits => null;

		public IReadOnlyList<int> AllowedDays => null;

		public float FEMARatingBonus => 0f;

		public int MinimumGamesCompletedToAppear => 0;

		public AudioClip NightRoomSound => null;

		public ESound EntranceTheme => ESound.None;

		[field: HideInInspector]
		[field: SerializeField]
		public Vector3[] ExtraPosition { get; private set; }

		[field: HideInInspector]
		[field: SerializeField]
		public Vector3[] ExtraRotation { get; private set; }

		[field: HideInInspector]
		[field: SerializeField]
		public Vector3[] ExtraScale { get; private set; }

		[field: HideInInspector]
		[field: SerializeField]
		public Vector2 DialogPosition { get; private set; }

		[field: HideInInspector]
		[field: SerializeField]
		public float DialogScale { get; private set; }

		public bool CanExile => false;

		public string Quote => null;

		public Vector2 GachaPosBias
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public Vector2 GachaScaleBias
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public Texture2D CorpseMask => null;

		public AnimationData GetEmotion(EDialogEmotionState emotion)
		{
			return null;
		}

		public void FillWeeks(int weekIndex)
		{
		}

		public void SetImposter(bool isImposter)
		{
		}
	}
}
