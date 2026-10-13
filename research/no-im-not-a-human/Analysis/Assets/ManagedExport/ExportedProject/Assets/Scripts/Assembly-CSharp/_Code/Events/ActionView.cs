using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Code.Events
{
	public sealed class ActionView : MonoBehaviour
	{
		[SerializeField]
		private GameObject _fill;

		public bool IsFilled { get; private set; }

		public async UniTask SetFilled(bool isFilled)
		{
			IsFilled = isFilled;
			if (isFilled)
			{
				if (!_fill.activeSelf)
				{
					_fill.SetActive(value: true);
					base.transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1.05f), 0.5f, 1);
					await (Tween)_fill.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
				}
			}
			else if (_fill.activeSelf)
			{
				base.transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1.05f), 0.5f, 1);
				await (Tween)_fill.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);
				_fill.SetActive(value: false);
			}
		}

		public async UniTask SetActive(bool isActive, bool isSkipAnim)
		{
			if (isActive)
			{
				if (!base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(value: true);
					if (!isSkipAnim)
					{
						await (Tween)base.transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1.05f), 0.5f, 1);
					}
				}
			}
			else if (base.gameObject.activeSelf)
			{
				if (!isSkipAnim)
				{
					await (Tween)base.transform.DOPunchScale(new Vector3(1.05f, 1.05f, 1.05f), 0.5f, 1);
				}
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
