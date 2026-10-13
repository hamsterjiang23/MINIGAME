using Cysharp.Threading.Tasks;

namespace _Scripts.Services.DataModel.DataStorages;

public sealed class RamDataStorage : IDataStorage
{
	public async UniTask Save(string key, object data, bool useSteamCloud)
	{
	}

	public UniTask<T> Load<T>(string key, bool useSteamCloud)
	{
		return default;
	}
}
