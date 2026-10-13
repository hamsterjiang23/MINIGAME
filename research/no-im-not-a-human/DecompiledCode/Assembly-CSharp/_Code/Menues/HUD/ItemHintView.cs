using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using _Code.Infrastructure.Consumables;
using _Code.Utils.Extensions;

namespace _Code.Menues.HUD;

public sealed class ItemHintView : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private LocalizeStringEvent _localizeStringEvent;

	[SerializeField]
	private LocalizedString _receivedString;

	[SerializeField]
	private LocalizedString _givenAwayString;

	private Queue<(EConsumable item, int count, bool isReceiving)> _queue = new Queue<(EConsumable, int, bool)>();

	private bool _isShowingNow;

	public void ShowItemGivenAwayHint(EConsumable item, int count)
	{
		if (!_isShowingNow)
		{
			_localizeStringEvent.StringReference = _givenAwayString;
			_localizeStringEvent.StringReference.Arguments = new object[2]
			{
				count,
				("consumable_" + item.ToString().ToLower() + "_name").GetLocalization("UI")
			};
			_localizeStringEvent.RefreshString();
			ShowAsync().Forget();
		}
		else
		{
			_queue.Enqueue((item, count, false));
		}
	}

	public void ShowItemReceivedHint(EConsumable item, int count)
	{
		if (!_isShowingNow)
		{
			_localizeStringEvent.StringReference = _receivedString;
			_localizeStringEvent.StringReference.Arguments = new object[2]
			{
				count,
				("consumable_" + item.ToString().ToLower() + "_name").GetLocalization("UI")
			};
			_localizeStringEvent.RefreshString();
			ShowAsync().Forget();
		}
		else
		{
			_queue.Enqueue((item, count, true));
		}
	}

	private async UniTask ShowAsync()
	{
		_isShowingNow = true;
		_canvasGroup.DOFade(1f, 0.33f);
		_canvasGroup.transform.localScale = Vector3.one * 0.8f;
		_canvasGroup.transform.DOScale(Vector3.one, 0.33f).SetEase(Ease.InSine);
		await UniTask.Delay(TimeSpan.FromSeconds(0.33000001311302185));
		await UniTask.Delay(TimeSpan.FromSeconds(2.0));
		_canvasGroup.DOFade(0f, 0.33f);
		_canvasGroup.transform.DOScale(Vector3.one * 0.8f, 0.33f).SetEase(Ease.OutSine);
		await UniTask.Delay(TimeSpan.FromSeconds(0.33000001311302185));
		_isShowingNow = false;
		if (_queue.Count > 0)
		{
			(EConsumable, int, bool) tuple = _queue.Dequeue();
			if (tuple.Item3)
			{
				ShowItemReceivedHint(tuple.Item1, tuple.Item2);
			}
			else
			{
				ShowItemGivenAwayHint(tuple.Item1, tuple.Item2);
			}
		}
	}
}
