using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RTLTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using _Code.Infrastructure.Sound;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Characters.DialogSystem
{
	public sealed class HoverableButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
	{
		[SerializeField]
		private RTLTextMeshPro _text;

		[SerializeField]
		private AnimatedImage _targetImage;

		[SerializeField]
		private ESoundSource _soundSource = ESoundSource.s1_GameScene_2DSFXSource;

		[SerializeField]
		private HoverableButtonStyle[] _styles;

		[SerializeField]
		private EDialogButtonStyle _startStyle;

		private HoverableButtonStyle _selectedStyle;

		private INotAHumanSoundService _soundService;

		public void Init(INotAHumanSoundService soundService)
		{
			_soundService = soundService;
			if (_selectedStyle == null)
			{
				SelectStyle(_startStyle);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			SetHoverData();
			_soundService.PlayOneShot(_soundSource, ESound.Ui_button_hover);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			SetBaseData();
		}

		public void OnSelect(BaseEventData eventData)
		{
			SetHoverData();
			_soundService?.PlayOneShot(_soundSource, ESound.Ui_button_hover);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			SetBaseData();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			SetBaseDataAfterPause().Forget();
			_soundService?.PlayOneShot(_soundSource, ESound.Ui_button_press);
		}

		private async UniTaskVoid SetBaseDataAfterPause()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
			SetBaseData();
		}

		private void SetBaseData()
		{
			if (_selectedStyle == null)
			{
				SelectStyle(_startStyle);
			}
			_targetImage.SetData(_selectedStyle.BaseImage);
			_targetImage.StartAnimationOrSetFirstFrame();
			_text.color = _selectedStyle.BaseColor;
			_text.margin = _selectedStyle.BasePaddings;
		}

		private void SetHoverData()
		{
			_targetImage.SetData(_selectedStyle.HoveredImage);
			_targetImage.StartAnimationOrSetFirstFrame();
			_text.color = _selectedStyle.HoveredColor;
			_text.margin = _selectedStyle.HoveredPaddings;
		}

		private void OnEnable()
		{
			SetBaseData();
		}

		public void SelectStyle(EDialogButtonStyle style)
		{
			HoverableButtonStyle selectedStyle = _selectedStyle;
			if ((object)selectedStyle == null || selectedStyle.Style != style)
			{
				_selectedStyle = _styles.FirstOrDefault((HoverableButtonStyle x) => x.Style == style);
				if (_selectedStyle != null)
				{
					SetBaseData();
				}
			}
		}
	}
}
