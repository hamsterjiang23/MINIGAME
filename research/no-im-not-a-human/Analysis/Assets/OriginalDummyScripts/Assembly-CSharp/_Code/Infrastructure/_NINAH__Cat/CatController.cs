using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure._NINAH__Cat;

public sealed class CatController : ASavableClass<CatSaveData>, ICatController, IInitializable, IDisposable
{
	private CatSaveData _saveData = new CatSaveData();

	private readonly ICatViewProvider _catViewProvider;

	private CatInstance _cat;

	private ECatAnimation _animation;

	private IDataModelService _dataModelService;

	public bool IsCatActive
	{
		get
		{
			return _saveData.IsActive;
		}
		private set
		{
			_saveData.IsActive = value;
		}
	}

	public event Action DayStarted;

	public event Action NightStarted;

	public event Action Born;

	public CatController(ICatViewProvider viewProvider, IDataModelService dataModelService)
	{
		_catViewProvider = viewProvider;
		_dataModelService = dataModelService;
		_cat = viewProvider.Cat;
		LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
	}

	public void Initialize()
	{
	}

	public void ActivateCat()
	{
		IsCatActive = true;
		_cat.gameObject.SetActive(value: true);
		ChangePosition(ETimeOfDay.Night);
	}

	private async UniTaskVoid ActivateCatOnLoad()
	{
		await UniTask.WaitUntil(() => _cat != null && (bool)_cat);
		_cat.gameObject.SetActive(value: true);
		ChangePositionTo(_saveData.LastTimeOfDay, _saveData.LastPosIndex);
	}

	private void ChangePositionTo(ETimeOfDay timeOfDay, int posIndex)
	{
		CatPosition catPosition = ((timeOfDay == ETimeOfDay.Day) ? _catViewProvider.DayPositions[posIndex] : _catViewProvider.NightPositions[posIndex]);
		_cat.transform.position = catPosition.Position;
		_cat.transform.eulerAngles = new Vector3(0f, catPosition.Rotation, 0f);
		_cat.SetAnimation(catPosition.Animation);
		_animation = catPosition.Animation;
	}

	public void ChangePosition(ETimeOfDay currentTimeOfDay)
	{
		if (_saveData.IsActive)
		{
			_saveData.LastTimeOfDay = currentTimeOfDay;
			if (currentTimeOfDay == ETimeOfDay.Night)
			{
				NightStarted?.Invoke();
			}
			else
			{
				DayStarted?.Invoke();
			}
			CatPosition catPosition;
			if (currentTimeOfDay == ETimeOfDay.Night)
			{
				int num = UnityEngine.Random.Range(0, _catViewProvider.NightPositions.Length);
				_saveData.LastPosIndex = num;
				catPosition = _catViewProvider.NightPositions[num];
			}
			else
			{
				int num2 = UnityEngine.Random.Range(0, _catViewProvider.DayPositions.Length);
				_saveData.LastPosIndex = num2;
				catPosition = _catViewProvider.DayPositions[num2];
			}
			_cat.transform.position = catPosition.Position;
			_cat.transform.eulerAngles = new Vector3(0f, catPosition.Rotation, 0f);
			_cat.SetAnimation(catPosition.Animation);
			_animation = catPosition.Animation;
		}
	}

	public void PlayAnimation()
	{
		_cat.SetAnimation(_animation);
	}

	protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
	{
		_saveData = saver.LoadKey(this);
		if (_saveData.IsActive)
		{
			ActivateCatOnLoad().Forget();
		}
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}
}
