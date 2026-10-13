using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Events;

namespace _Code.Menues
{
	public sealed class ActionsView : MonoBehaviour
	{
		[SerializeField]
		private ActionView[] _actions;

		public void SetCount(int count, bool isSkipAnim = false)
		{
			for (int i = 0; i < _actions.Length; i++)
			{
				_actions[i].SetActive(i < count, isSkipAnim).Forget();
			}
		}

		public async UniTask SetFilledCount(int filledCount)
		{
			if (_actions.Count((ActionView x) => x.IsFilled) > filledCount)
			{
				for (int i = _actions.Length - 1; i >= 0; i--)
				{
					await _actions[i].SetFilled(i < filledCount);
				}
			}
			else
			{
				for (int i = 0; i < _actions.Length; i++)
				{
					await _actions[i].SetFilled(i < filledCount);
				}
			}
		}
	}
}
