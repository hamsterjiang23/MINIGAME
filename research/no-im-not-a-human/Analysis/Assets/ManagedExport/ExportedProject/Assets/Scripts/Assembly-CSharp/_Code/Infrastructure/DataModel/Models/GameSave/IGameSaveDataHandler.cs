using System;
using Cysharp.Threading.Tasks;

namespace _Code.Infrastructure.DataModel.Models.GameSave
{
	public interface IGameSaveDataHandler
	{
		bool NeedToLoadGame { get; }

		bool HasSaveData { get; }

		event Action<IGameSaveDataHandler> LoadedAll;

		event Action<bool> SavedAll;

		UniTask LoadData();

		UniTask LoadLocalData();

		void ConnectKey<T>(ASavableClass<T> savable, T data) where T : ASavableData;

		T LoadKey<T>(ASavableClass<T> savable) where T : ASavableData;

		void SaveAll();

		void LoadAll();

		void PrepareToLoadGame();

		void UnprepareToLoadGame();

		void LoadIfNeeded();

		void ClearSavedData();

		UniTask SaveAllReserve();

		void ResetEvents();
	}
}
