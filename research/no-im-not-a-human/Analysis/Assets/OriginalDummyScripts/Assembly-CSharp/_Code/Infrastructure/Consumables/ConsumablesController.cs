using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.OtherGameData;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Consumables;

public sealed class ConsumablesController : ASavableClass<ConsumablesSaveData>, IConsumablesController, IDisposable, ITickable
{
	private ConsumablesSaveData _saveData = new ConsumablesSaveData();

	private Func<int> _getDay;

	private readonly OtherGameSOData _otherGameSoData;

	private readonly IDataModelService _dataModelService;

	public int PovistaskUsedCount => _saveData.PovistkasUsed;

	public event Action<EConsumable, int> UpdatedCount;

	public event Action<EConsumable, int> ReceivedWithHint;

	public event Action<EConsumable, int> GivenAwayWithHint;

	public ConsumablesController(IDataModelService dataModelService, IOtherGameSODataProvider otherGameSoDataProvider)
	{
		_dataModelService = dataModelService;
		_otherGameSoData = otherGameSoDataProvider.OtherGameData;
		foreach (object value in Enum.GetValues(typeof(EConsumable)))
		{
			_saveData.Storage.Add((EConsumable)value, 0);
		}
		if (!dataModelService.GameSaveDataHandler.NeedToLoadGame)
		{
			Add(EConsumable.Bobeer, 8);
			Add(EConsumable.Kombucha);
			Add(EConsumable.Cockroach);
		}
		_saveData.DeficitItemDay = _otherGameSoData.GetDeficitRandomDaysCount;
		LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
	}

	public void InitializeGetDay(Func<int> func)
	{
		_getDay = func;
	}

	public bool IsNeedToGetDeficitItem()
	{
		int num = _getDay();
		int deficitItemDay = _saveData.DeficitItemDay;
		if (num < deficitItemDay)
		{
			return false;
		}
		return _otherGameSoData.DeficitConsumables.Count((KeyValuePair<EConsumable, int> x) => x.Value >= _saveData.Storage[x.Key]) > 0;
	}

	public EConsumable GetDeficitItem()
	{
		return (from x in _otherGameSoData.DeficitConsumables
			where x.Value >= _saveData.Storage[x.Key]
			select x.Key).GetRandomElement();
	}

	public void UpdateDeficitDay()
	{
		_saveData.DeficitItemDay = _getDay() + _otherGameSoData.GetDeficitRandomDaysCount;
	}

	public void BeginNewDay()
	{
		_saveData.ConsumablesTodayUsed.Clear();
	}

	public void Add(EConsumable consumable, int count = 1, bool isShowHint = false)
	{
		_saveData.Storage[consumable] += count;
		UpdatedCount?.Invoke(consumable, _saveData.Storage[consumable]);
		if (isShowHint)
		{
			ReceivedWithHint?.Invoke(consumable, count);
		}
	}

	public bool TryRemove(EConsumable consumable, int count = 1, bool isShowHint = false)
	{
		if (_saveData.Storage[consumable] < count)
		{
			return false;
		}
		_saveData.Storage[consumable] -= count;
		UpdatedCount?.Invoke(consumable, _saveData.Storage[consumable]);
		if (isShowHint)
		{
			GivenAwayWithHint?.Invoke(consumable, count);
		}
		if (!_dataModelService.MetaPrefsDataHandler.IsEverUsedConsumable(consumable))
		{
			_dataModelService.MetaPrefsDataHandler.UseConsumable(consumable);
		}
		if (!_saveData.ConsumablesEverUsed.Contains(consumable))
		{
			_saveData.ConsumablesEverUsed.Add(consumable);
		}
		_saveData.ConsumablesTodayUsed.Add(consumable);
		if (_saveData.ConsumablesTodayUsed.Contains(EConsumable.Cigarette) && _saveData.ConsumablesTodayUsed.Contains(EConsumable.Coffee))
		{
			SteamAchievements.Unlock("CONSUM_COFFEE_CIG");
		}
		if (consumable == EConsumable.Povistka)
		{
			_saveData.PovistkasUsed++;
		}
		return true;
	}

	public int Count(EConsumable consumable)
	{
		return _saveData.Storage[consumable];
	}

	protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
	{
		_saveData = saver.LoadKey(this);
		foreach (KeyValuePair<EConsumable, int> item in _saveData.Storage)
		{
			UpdatedCount?.Invoke(item.Key, item.Value);
		}
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}

	public bool EverUsedConsumableThisRun(EConsumable consumable)
	{
		return _saveData.ConsumablesEverUsed.Contains(consumable);
	}

	public void Tick()
	{
	}
}
