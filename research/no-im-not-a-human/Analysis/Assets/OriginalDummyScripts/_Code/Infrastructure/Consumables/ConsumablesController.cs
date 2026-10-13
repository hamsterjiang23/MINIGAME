using System;
using System.Runtime.CompilerServices;
using Zenject;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.OtherGameData;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Consumables
{
	public sealed class ConsumablesController : ASavableClass<ConsumablesSaveData>, IConsumablesController, IDisposable, ITickable
	{
		private ConsumablesSaveData _saveData;

		private Func<int> _getDay;

		private readonly OtherGameSOData _otherGameSoData;

		private readonly IDataModelService _dataModelService;

		[CompilerGenerated]
		private Action<EConsumable, int> m_UpdatedCount;

		[CompilerGenerated]
		private Action<EConsumable, int> m_ReceivedWithHint;

		[CompilerGenerated]
		private Action<EConsumable, int> m_GivenAwayWithHint;

		public int PovistaskUsedCount => 0;

		public event Action<EConsumable, int> UpdatedCount
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EConsumable, int> ReceivedWithHint
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EConsumable, int> GivenAwayWithHint
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public ConsumablesController(IDataModelService dataModelService, IOtherGameSODataProvider otherGameSoDataProvider)
		{
		}

		public void InitializeGetDay(Func<int> func)
		{
		}

		public bool IsNeedToGetDeficitItem()
		{
			return false;
		}

		public EConsumable GetDeficitItem()
		{
			return EConsumable.Bobeer;
		}

		public void UpdateDeficitDay()
		{
		}

		public void BeginNewDay()
		{
		}

		public void Add(EConsumable consumable, int count = 1, bool isShowHint = false)
		{
		}

		public bool TryRemove(EConsumable consumable, int count = 1, bool isShowHint = false)
		{
			return false;
		}

		public int Count(EConsumable consumable)
		{
			return 0;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}

		public bool EverUsedConsumableThisRun(EConsumable consumable)
		{
			return false;
		}

		public void Tick()
		{
		}
	}
}
