using System;
using Cysharp.Threading.Tasks;
using _Code.Utils.Logger;
using _Scripts.Services.DataModel.DataStorages;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.GameSave
{
	public sealed class GameSaveDataHandler : ABaseDataHandler<GameSaveData>, IGameSaveDataHandler
	{
		private ConditionalLocalLogger _logger = new ConditionalLocalLogger();

		protected override bool UseSteamCloud => true;

		public bool NeedToLoadGame => base.Data?.NeedToLoad ?? false;

		public bool HasSaveData => base.Data?.HasSaveData ?? false;

		public event Action<IGameSaveDataHandler> LoadedAll;

		public event Action<bool> SavedAll;

		public GameSaveDataHandler(IDataStorage dataStorage)
			: base(dataStorage)
		{
		}

		public void ConnectKey<T>(ASavableClass<T> savable, T data) where T : ASavableData
		{
			_logger.Log("Connecting key " + savable.GetType().Name);
			Type type = savable.GetType();
			if (base.Data.Values.ContainsKey(type))
			{
				base.Data.Values.Remove(type);
				base.Data.Values.Add(type, data);
			}
			else
			{
				base.Data.Values.Add(type, data);
			}
		}

		public T LoadKey<T>(ASavableClass<T> savable) where T : ASavableData
		{
			_logger.Log("Loading key " + savable.GetType().Name);
			return base.Data.LoadFromJson(savable);
		}

		public void SaveAll()
		{
			SaveAsync().Forget();
		}

		public async UniTask SaveAllReserve()
		{
			await SaveAsyncReserve();
		}

		private async UniTaskVoid SaveAsync()
		{
			ClearSavedData();
			SavedAll?.Invoke(obj: false);
			await UniTask.DelayFrame(1);
			base.Data.CreateJson();
			SaveData().Forget();
		}

		private async UniTask SaveAsyncReserve()
		{
			SavedAll?.Invoke(obj: true);
			await UniTask.DelayFrame(1);
			base.Data.CreateReserveJson();
			await SaveData();
		}

		public void LoadAll()
		{
			_logger.Log("Loading all data");
			LoadedAll?.Invoke(this);
		}

		public void PrepareToLoadGame()
		{
			base.Data.NeedToLoad = true;
		}

		public void UnprepareToLoadGame()
		{
			base.Data.NeedToLoad = false;
		}

		public void LoadIfNeeded()
		{
			_logger.Log("Loading all data?");
			if (base.Data.NeedToLoad)
			{
				LoadAll();
				UnprepareToLoadGame();
			}
		}

		public void ClearSavedData()
		{
			_logger.Log("Clear saved data");
			base.Data.ClearReserveData();
			SaveData().Forget();
		}

		public void ResetEvents()
		{
			SavedAll = null;
			LoadedAll = null;
			base.Data.ResetValues();
		}
	}
}
