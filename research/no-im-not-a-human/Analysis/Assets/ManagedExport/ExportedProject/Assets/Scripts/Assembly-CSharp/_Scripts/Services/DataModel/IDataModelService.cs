using System;
using Cysharp.Threading.Tasks;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DataModel.Models.Meta;
using _Code.Infrastructure.DataModel.Models.Settings;

namespace _Scripts.Services.DataModel
{
	public interface IDataModelService
	{
		IGameSaveDataHandler GameSaveDataHandler { get; }

		ISettingsPrefsDataHandler SettingsPrefsDataHandler { get; }

		IMetaPrefsDataHandler MetaPrefsDataHandler { get; }

		bool IsInitialized { get; }

		bool IsCloudSaveLoading { get; }

		string CloudSaveLoadError { get; }

		event Action<bool> CloudSaveLoadingChanged;

		event Action<string> CloudSaveLoadFailed;

		UniTask WaitForInitialization();
	}
}
