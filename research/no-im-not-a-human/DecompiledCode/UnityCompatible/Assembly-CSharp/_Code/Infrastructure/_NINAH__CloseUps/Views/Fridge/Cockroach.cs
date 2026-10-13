using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.CloseUps.Views;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD.Animations;
using _Code.Utils;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Instance;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Fridge
{
	public sealed class Cockroach : MonoBehaviour
	{
		[SerializeField]
		private Sprite _killedSprite;

		[SerializeField]
		private AnimatedImage _image;

		[SerializeField]
		private SoundServiceInstance _soundService;

		[SerializeField]
		private FridgeItemView _fridgeItemView;

		private bool _isKilled;

		public void Kill()
		{
			if (!_isKilled)
			{
				_isKilled = true;
				_image.SetData(new AnimationData(EAnimationCyclingType.FixLastFrame, new Sprite[1] { _killedSprite }));
				_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Fridge_cockroach_smash);
				_fridgeItemView.enabled = false;
				Vibrator.VibrateForTime(0.1f, 0.1f, 0.1f).Forget();
				SteamAchievements.Unlock("COCKROACH");
			}
		}
	}
}
