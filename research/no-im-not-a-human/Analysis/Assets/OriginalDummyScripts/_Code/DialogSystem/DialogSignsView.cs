using RTLTMPro;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.UI;
using _Code.Characters;
using _Code.Infrastructure.Sound;
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

		private ADialogSignElementView[] AllContainers => null;

		public void ShowSign(CharacterSOData character, ECharacterSign sign)
		{
		}

		public void StopShowingSign()
		{
		}

		public void InitModules(INotAHumanSoundService soundService)
		{
		}
	}
}
