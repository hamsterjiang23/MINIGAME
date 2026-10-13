using Cysharp.Threading.Tasks;
using _Scripts.Services.DataModel.Models.CustomData;

namespace _Scripts.Services.DataModel.Models.PlayerData;

public interface ISimplePrefsDataHandler
{
	UniTask LoadData();

	void SavePref(ESimpleDataType type, string key, string value);

	string LoadString(string key);

	int? LoadInt(string key);

	float? LoadFloat(string key);
}
