using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DataModel.Models.Meta;
using _Code.Infrastructure.DataModel.Models.Settings;
using _Scripts.Services.DataModel;
using _Scripts.Services.DataModel.DataStorages;
using _Scripts.Services.DataModel.Models.Meta;

namespace _Code.Infrastructure.DataModel;

public sealed class DataModelService : IDataModelService
{
	private readonly PrefsDataStorage _prefsDataStorage = new PrefsDataStorage();

	private readonly EncryptedPrefsDataStorage _encryptedPrefsDataStorage = new EncryptedPrefsDataStorage();

	private readonly RamDataStorage _ramDataStorage = new RamDataStorage();

	public IGameSaveDataHandler GameSaveDataHandler { get; private set; }

	public ISettingsPrefsDataHandler SettingsPrefsDataHandler { get; private set; }

	public IMetaPrefsDataHandler MetaPrefsDataHandler { get; private set; }

	public bool IsInitialized { get; private set; }

	public bool IsCloudSaveLoading { get; private set; }

	public string CloudSaveLoadError { get; private set; }

	public event Action<bool> CloudSaveLoadingChanged;

	public event Action<string> CloudSaveLoadFailed;

	public DataModelService()
	{
		Init();
	}

	public async UniTask WaitForInitialization()
	{
		if (!IsInitialized)
		{
			await UniTask.WaitUntil(() => IsInitialized);
		}
	}

	private void Init()
	{
		GameSaveDataHandler = new GameSaveDataHandler(_encryptedPrefsDataStorage);
		SettingsPrefsDataHandler = new SettingsPrefsDataHandler(_prefsDataStorage);
		MetaPrefsDataHandler = new MetaPrefsDataHandler(_encryptedPrefsDataStorage);
		InitAsync().Forget();
	}

	private async UniTaskVoid InitAsync()
	{
		_ = 3;
		try
		{
			try
			{
				await SettingsPrefsDataHandler.LoadData();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				SettingsPrefsDataHandler = new SettingsPrefsDataHandler(_prefsDataStorage);
			}
			await GameSaveDataHandler.LoadLocalData();
			await MetaPrefsDataHandler.LoadLocalData();
			await LoadCloudData();
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		finally
		{
			SteamCloudSaver.Init().Forget();
			IsInitialized = true;
		}
	}

	private async UniTask LoadCloudData()
	{
		List<string> errors = new List<string>();
		CloudSaveLoadError = null;
		SetCloudSaveLoading(isLoading: true);
		try
		{
			await LoadCloudHandlerData(GameSaveDataHandler.LoadData(), errors);
			await LoadCloudHandlerData(MetaPrefsDataHandler.LoadData(), errors);
		}
		finally
		{
			SetCloudSaveLoading(isLoading: false);
		}
		if (errors.Count != 0)
		{
			CloudSaveLoadError = string.Join("\n", errors);
			CloudSaveLoadFailed?.Invoke(CloudSaveLoadError);
		}
	}

	private async UniTask LoadCloudHandlerData(UniTask loadTask, List<string> errors)
	{
		try
		{
			await loadTask;
			CollectSteamCloudLoadError(errors);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			if (!errors.Contains(ex.Message))
			{
				errors.Add(ex.Message);
			}
		}
	}

	private void CollectSteamCloudLoadError(List<string> errors)
	{
		if (SteamCloudSaver.LastLoadFailed && !string.IsNullOrEmpty(SteamCloudSaver.LastLoadError) && !errors.Contains(SteamCloudSaver.LastLoadError))
		{
			errors.Add(SteamCloudSaver.LastLoadError);
		}
	}

	private void SetCloudSaveLoading(bool isLoading)
	{
		if (IsCloudSaveLoading != isLoading)
		{
			IsCloudSaveLoading = isLoading;
			CloudSaveLoadingChanged?.Invoke(isLoading);
		}
	}
}
