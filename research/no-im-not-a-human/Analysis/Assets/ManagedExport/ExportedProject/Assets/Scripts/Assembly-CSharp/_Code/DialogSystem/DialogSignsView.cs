using Cysharp.Threading.Tasks;
using RTLTMPro;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.UI;
using _Code.Characters;
using _Code.Infrastructure.Sound;
using _Code.Utils;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.DialogSystem
{
	public sealed class DialogSignsView : MonoBehaviour
	{
		[SerializeField]
		private RTLTextMeshPro _normalText;

		[SerializeField]
		private Image _textContainer;

		[SerializeField]
		private DialogSignElementViewEye _eyesContainer;

		[SerializeField]
		private DialogSignElementViewMovingContent _handsContainer;

		[SerializeField]
		private ADialogSignElementView _teethContainer;

		[SerializeField]
		private DialogSignViewAnimatedContent _earContainer;

		[SerializeField]
		private DialogSignViewAnimatedContent _armpitContainer;

		[SerializeField]
		private DialogSignElementViewMovingContent _photoContainer;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _eyesSounds;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _teethSounds;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _handsSounds;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _armpitsSounds;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _earSounds;

		private INotAHumanSoundService _soundService;

		private ADialogSignElementView[] AllContainers => new ADialogSignElementView[6] { _eyesContainer, _handsContainer, _teethContainer, _earContainer, _armpitContainer, _photoContainer };

		public void ShowSign(CharacterSOData character, ECharacterSign sign)
		{
			_textContainer.color = Color.clear;
			ADialogSignElementView[] allContainers = AllContainers;
			for (int i = 0; i < allContainers.Length; i++)
			{
				allContainers[i].gameObject.SetActive(value: false);
			}
			switch (sign)
			{
			case ECharacterSign.Eye:
				Vibrator.VibrateForTime(0.15f, 0f, 0.2f).Forget();
				_eyesContainer.gameObject.SetActive(value: true);
				_eyesContainer.Init(character.EyeSprite);
				_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _eyesSounds.GetRandomElement());
				break;
			case ECharacterSign.Hands:
			{
				Vibrator.VibrateForTime(0.225f, 0.07f, 0.12f).Forget();
				Sprite photoSprite = character.HandsSprite;
				_handsContainer.gameObject.SetActive(value: true);
				_handsContainer.Content.sprite = photoSprite;
				_handsContainer.Move().Forget();
				_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _handsSounds.GetRandomElement());
				break;
			}
			case ECharacterSign.Teeth:
			{
				Vibrator.VibrateForTime(0.175f, 0.05f, 0.15f).Forget();
				Sprite photoSprite = character.TeethSprite;
				_teethContainer.gameObject.SetActive(value: true);
				_teethContainer.Content.sprite = photoSprite;
				_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _teethSounds.GetRandomElement());
				break;
			}
			case ECharacterSign.AuraPhoto:
			{
				_normalText.color = Color.clear;
				Sprite photoSprite = character.PhotoSprite;
				_photoContainer.gameObject.SetActive(value: true);
				_photoContainer.Content.sprite = photoSprite;
				_photoContainer.Move().Forget();
				_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Char_sign_auracam_01);
				break;
			}
			case ECharacterSign.Armpit:
				Vibrator.VibrateForTime(0.175f, 0.08f, 0.1f).Forget();
				_armpitContainer.gameObject.SetActive(value: true);
				_armpitContainer.AnimatedContent.SetData(character.ArmpitSprite);
				_armpitContainer.AnimatedContent.StartAnimationOrSetFirstFrame();
				_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _armpitsSounds.GetRandomElement());
				break;
			case ECharacterSign.Ear:
				_earContainer.gameObject.SetActive(value: true);
				_earContainer.AnimatedContent.SetData(character.EarSprite);
				_earContainer.AnimatedContent.StartAnimationOrSetFirstFrame();
				_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _earSounds.GetRandomElement());
				break;
			}
		}

		public void StopShowingSign()
		{
			_normalText.color = Color.white;
			_textContainer.color = Color.black;
			ADialogSignElementView[] allContainers = AllContainers;
			for (int i = 0; i < allContainers.Length; i++)
			{
				allContainers[i].gameObject.SetActive(value: false);
			}
		}

		public void InitModules(INotAHumanSoundService soundService)
		{
			_soundService = soundService;
		}
	}
}
