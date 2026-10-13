using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD.Animations;
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
		private ESound _knockSound = ESound.Sfx_doorKnock_1;

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
		private bool _canExile = true;

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

		public ECharacterType CharacterType => _characterType;

		public RoomObjectState<ERoomPeopleState>[] Poses => _poses;

		public SerializedDictionary<EDialogEmotionState, DialogTransformData> EmotionOverrides => _emotionOverrides;

		public ERoom Room => _room;

		public Sprite HandsSprite
		{
			get
			{
				if (!_isImposter)
				{
					return _handsSpriteHuman;
				}
				return _handsSpriteImposter;
			}
		}

		public CharacterEyeData EyeSprite
		{
			get
			{
				if (!_isImposter)
				{
					return _eyeSpriteHuman;
				}
				return _eyeSpriteImposter;
			}
		}

		public Sprite TeethSprite
		{
			get
			{
				if (!_isImposter)
				{
					return _teethSpriteHuman;
				}
				return _teethSpriteImposter;
			}
		}

		public AnimationData ArmpitSprite
		{
			get
			{
				if (!_isImposter)
				{
					return _armpitHuman;
				}
				return _armpitImposter;
			}
		}

		public AnimationData EarSprite
		{
			get
			{
				if (!_isImposter)
				{
					return _earHuman;
				}
				return _earImposter;
			}
		}

		public Sprite PhotoSprite
		{
			get
			{
				if (!_isImposter)
				{
					return _photoHuman;
				}
				return _photoImposter;
			}
		}

		public bool IsImposter
		{
			get
			{
				return _isImposter;
			}
			set
			{
				_isImposter = value;
			}
		}

		public float EntranceScale => _entranceScale;

		public float RoomScale => _roomScale;

		public bool IsPreset => _isPreset;

		public ECharacterPlace Place => _place;

		public ESound KnockSound => _knockSound;

		public bool IsStatusRandomlyGenerated => _isStatusRandomlyGenerated;

		public IReadOnlyList<CharacterVisitData> Visits => _visits;

		public IReadOnlyList<int> AllowedDays => _allowedDays;

		public float FEMARatingBonus => _FEMARatingBonus;

		public int MinimumGamesCompletedToAppear => _minimumGamesCompletedToAppear;

		public AudioClip NightRoomSound => _nightRoomSound;

		public ESound EntranceTheme => _entranceTheme;

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

		public bool CanExile => _canExile;

		public string Quote => _quote.GetLocalizedString();

		public Vector2 GachaPosBias
		{
			get
			{
				return _gachaPosBias;
			}
			set
			{
				_gachaPosBias = value;
			}
		}

		public Vector2 GachaScaleBias
		{
			get
			{
				return _gachaScaleBias;
			}
			set
			{
				_gachaScaleBias = value;
			}
		}

		public Texture2D CorpseMask => _corpseMask;

		public AnimationData GetEmotion(EDialogEmotionState emotion)
		{
			ACharacterSpriteByEmotion aCharacterSpriteByEmotion = _emotions.FirstOrDefault((ACharacterSpriteByEmotion x) => x.Emotion == emotion);
			if (aCharacterSpriteByEmotion == null)
			{
				return new AnimationData();
			}
			if (!(aCharacterSpriteByEmotion is CharacterAnimatedSpriteByEmotion { EmotionSprites: var emotionSprites } characterAnimatedSpriteByEmotion))
			{
				return new AnimationData(EAnimationCyclingType.FixLastFrame, aCharacterSpriteByEmotion.EmotionSprites);
			}
			return new AnimationData(characterAnimatedSpriteByEmotion.AnimationData.CyclingType, emotionSprites, characterAnimatedSpriteByEmotion.AnimationData.FramesPerSecond);
		}

		public void FillWeeks(int weekIndex)
		{
			_allowedDays = weekIndex switch
			{
				-1 => new int[0], 
				0 => new int[7] { 1, 2, 3, 4, 5, 6, 7 }, 
				1 => new int[7] { 8, 9, 10, 11, 12, 13, 14 }, 
				2 => new int[14]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
					11, 12, 13, 14
				}, 
			};
		}

		public void SetImposter(bool isImposter)
		{
			if (!IsPreset)
			{
				_isImposter = isImposter;
			}
		}
	}
}
