using Cysharp.Threading.Tasks;

namespace _Scripts.Services.DataModel.DataStorages;

public interface IDataStorage
{
	UniTask Save(string key, object data, bool useSteamCloud);

	UniTask<T> Load<T>(string key, bool useSteamCloud);
}
