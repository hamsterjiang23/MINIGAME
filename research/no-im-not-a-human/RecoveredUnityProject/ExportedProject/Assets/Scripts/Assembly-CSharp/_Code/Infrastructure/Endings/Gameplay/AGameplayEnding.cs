using System;
using Cysharp.Threading.Tasks;
using _Code.Infrastructure.Endings.View;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public abstract class AGameplayEnding
	{
		private IEndingShower _endingShower;

		public abstract int Priority { get; }

		public abstract EEnding Ending { get; }

		public abstract bool AreConditionsMet { get; }

		public event Action Triggered;

		protected abstract void TriggerInner();

		public void Trigger()
		{
			Triggered?.Invoke();
			TriggerInner();
		}

		public void PreInit(IEndingShower endingShower)
		{
			_endingShower = endingShower;
		}

		public async UniTask ShowClip(float delayBeforeShow = 0f)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(delayBeforeShow));
			_endingShower.ShowEnding(Ending);
		}
	}
}
