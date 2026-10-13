using Cysharp.Threading.Tasks;
using _Scripts.Services.DataModel.DataStorages;

namespace _Scripts.Services.DataModel.Models;

public abstract class ABaseDataHandler<T> where T : BaseDataStorage, new()
{
	protected IDataStorage _dataStorage;

	protected string KeyName => typeof(T).Name;

	protected T Data { get; set; }

	protected abstract bool UseSteamCloud { get; }

	protected ABaseDataHandler(IDataStorage dataStorage)
	{
		_dataStorage = dataStorage;
	}

	public async UniTask LoadData()
	{
		T val = await _dataStorage.Load<T>(KeyName, UseSteamCloud);
		if (val != null)
		{
			Data = val;
		}
		if (Data == null)
		{
			Data = new T();
		}
		LoadInner();
	}

	public async UniTask LoadLocalData()
	{
		Data = await _dataStorage.Load<T>(KeyName, useSteamCloud: false);
		if (Data == null)
		{
			Data = new T();
		}
		LoadInner();
	}

	protected virtual void LoadInner()
	{
	}

	public async UniTask SaveData()
	{
		T data = Data;
		int revision = data.Revision;
		data.Revision = revision + 1;
		await _dataStorage.Save(KeyName, Data, UseSteamCloud);
	}
}
