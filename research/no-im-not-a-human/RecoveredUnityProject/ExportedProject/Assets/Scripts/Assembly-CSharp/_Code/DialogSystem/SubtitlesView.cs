using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RTLTMPro;
using UnityEngine;
using _Code.Characters.DialogSystem;

namespace _Code.DialogSystem
{
	public sealed class SubtitlesView : MonoBehaviour
	{
		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private GameObject _gameObject;

		[SerializeField]
		private RTLTextMeshPro _text;

		[SerializeField]
		private MessageByLanguage[] _messages;

		[SerializeField]
		private Transform _showedPos;

		[SerializeField]
		private Transform _hiddenPos;

		private readonly Queue<(string message, TimeSpan time)> _messageQueue = new Queue<(string, TimeSpan)>();

		private const float SHOW_HIDE_DURATION = 0.5f;

		public void Show()
		{
			_gameObject.SetActive(value: true);
			_gameObject.transform.position = _hiddenPos.position;
			_gameObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InSine);
			_gameObject.transform.DOMove(_showedPos.position, 0.5f).SetEase(Ease.InSine);
			_canvasGroup.DOFade(1f, 0.5f);
		}

		public void Hide()
		{
			AsyncHide().Forget();
		}

		private async UniTask AsyncHide()
		{
			_gameObject.transform.DOScale(new Vector3(0.1f, 0.5f, 1f), 0.5f).SetEase(Ease.OutSine);
			_gameObject.transform.DOMove(_hiddenPos.position, 0.5f).SetEase(Ease.OutSine);
			_canvasGroup.DOFade(0f, 0.5f);
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			_gameObject.SetActive(value: false);
			await UniTask.DelayFrame(1);
		}

		public async UniTask ShowDialogForTime(EInfoMessageType messageType, TimeSpan time)
		{
			MessageByLanguage messageByLanguage = _messages.FirstOrDefault((MessageByLanguage x) => x.Type == messageType);
			if (messageByLanguage != null)
			{
				await ShowDialogForTime(messageByLanguage.MessageLocalizationKey.GetLocalizedString(), time);
			}
		}

		public async UniTask ShowDialogForTime(string message, TimeSpan time)
		{
			if (_gameObject.activeSelf)
			{
				if (_messageQueue.Count != 0 && !(message == _messageQueue.Last().message))
				{
					_messageQueue.Enqueue((message, time));
				}
				return;
			}
			_gameObject.SetActive(value: true);
			_text.text = message;
			_gameObject.transform.position = _hiddenPos.position;
			_gameObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
			_gameObject.transform.DOMove(_showedPos.position, 0.5f);
			_canvasGroup.DOFade(1f, 0.5f);
			await UniTask.Delay(time);
			await AsyncHide();
			if (_messageQueue.Count > 0)
			{
				(string, TimeSpan) tuple = _messageQueue.Dequeue();
				ShowDialogForTime(tuple.Item1, tuple.Item2).Forget();
			}
		}

		public void UpdateText(string text)
		{
			_text.text = text;
		}
	}
}
