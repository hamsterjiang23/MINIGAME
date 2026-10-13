using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Code.DialogSystem
{
	public sealed class DialogSignElementViewMovingContent : ADialogSignElementView
	{
		[field: SerializeField]
		public Vector3 StartPosition { get; private set; }

		[field: SerializeField]
		public Vector3 EndPosition { get; private set; }

		[field: SerializeField]
		public Ease Ease { get; private set; }

		[field: SerializeField]
		public float Duration { get; private set; }

		[field: SerializeField]
		public float BeforeDelay { get; private set; }

		public async UniTask Move()
		{
			Content.transform.localPosition = StartPosition;
			await UniTask.Delay(TimeSpan.FromSeconds(BeforeDelay));
			await (Tween)Content.transform.DOLocalMove(EndPosition, Duration).SetEase(Ease);
		}
	}
}
