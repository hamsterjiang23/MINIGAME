using System;
using Cysharp.Threading.Tasks;
using _Code.Utils.Logger;
using _Scripts.Services.DataModel.DataStorages;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.GameSave;

public sealed class GameSaveDataHandler : ABaseDataHandler<GameSaveData>, IGameSaveDataHandler
{
	private ConditionalLocalLogger _logger = new ConditionalLocalLogger();

	protected override bool UseSteamCloud => true;

	public bool NeedToLoadGame => Data?.NeedToLoad ?? false;

	public bool HasSaveData => Data?.HasSaveData ?? false;

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
		if (Data.Values.ContainsKey(type))
		{
			Data.Values.Remove(type);
			Data.Values.Add(type, data);
		}
		else
		{
			Data.Values.Add(type, data);
		}
	}

	public T LoadKey<T>(ASavableClass<T> savable) where T : ASavableData
	{
		_logger.Log("Loading key " + savable.GetType().Name);
		return Data.LoadFromJson(savable);
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
		Data.CreateJson();
		SaveData().Forget();
	}

	private async UniTask SaveAsyncReserve()
	{
		SavedAll?.Invoke(obj: true);
		await UniTask.DelayFrame(1);
		Data.CreateReserveJson();
		await SaveData();
	}

	public void LoadAll()
	{
		_logger.Log("Loading all data");
		LoadedAll?.Invoke(this);
	}

	public void PrepareToLoadGame()
	{
		Data.NeedToLoad = true;
	}

	public void UnprepareToLoadGame()
	{
		Data.NeedToLoad = false;
	}

	public void LoadIfNeeded()
	{
		_logger.Log("Loading all data?");
		if (Data.NeedToLoad)
		{
			LoadAll();
			UnprepareToLoadGame();
		}
	}

	public void ClearSavedData()
	{
		_logger.Log("Clear saved data");
		Data.ClearReserveData();
		SaveData().Forget();
	}

	public void ResetEvents()
	{
		SavedAll = null;
		LoadedAll = null;
		Data.ResetValues();
	}
}
