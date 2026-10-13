using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms
{
	public sealed class OfficeView : ARoomView
	{
		[SerializeField]
		private UIButton _baby;

		[SerializeField]
		private GameObject _cultistsSheets;

		private IGameplayEndingManager _gameplayEndingManager;

		public void Init(IGameplayEndingManager endingManager)
		{
			_gameplayEndingManager = endingManager;
		}

		public void ActivateBaby()
		{
			_baby.gameObject.SetActive(value: true);
			_baby.SetAction(() =>
			{
				((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).FindBaby(ERoom.Office);
				_baby.gameObject.SetActive(value: false);
			});
		}

		public void ActivateCultists()
		{
			_cultistsSheets.gameObject.SetActive(value: true);
		}

		public void OnLoad(RoomsSaveData saveData)
		{
			if (saveData.BellyProgress >= 4)
			{
				OnLoadAsync().Forget();
			}
		}

		private async UniTaskVoid OnLoadAsync()
		{
			await UniTask.WaitUntil(() => _gameplayEndingManager != null);
			if (!((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).IsOfficeBabyFound)
			{
				_baby.gameObject.SetActive(value: true);
				_baby.SetAction(() =>
				{
					((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).FindBaby(ERoom.Office);
					_baby.gameObject.SetActive(value: false);
				});
			}
		}
	}
}
