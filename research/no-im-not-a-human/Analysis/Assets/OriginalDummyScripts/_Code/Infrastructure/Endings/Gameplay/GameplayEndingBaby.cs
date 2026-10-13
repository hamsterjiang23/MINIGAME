using System;
using System.Runtime.CompilerServices;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Windows;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Rooms;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingBaby : AGameplayEnding
	{
		private IWindowsManager _windowsManager;

		private IDayNightController _dayNightController;

		private IStateObjectController _stateObjectController;

		private IInteractablesManager _interactablesManager;

		private IEffectsController _effectsController;

		private INotAHumanSoundService _soundService;

		private GameplayEndingManagerSaveData _saveData;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _babyFoundSounds;

		[CompilerGenerated]
		private Action<int> m_FoundBaby;

		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		public bool IsBathroomBabyFound => false;

		public bool IsOfficeBabyFound => false;

		public bool IsBedroomBabyFound => false;

		public event Action<int> FoundBaby
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

		protected override void TriggerInner()
		{
		}

		public void InitModules(IDayNightController dayNightController, IWindowsManager windowsManager, IStateObjectController stateObjectController, GameplayEndingManagerSaveData saveData, IEffectsController effectsController, INotAHumanSoundService soundService)
		{
		}

		public void FindBaby(ERoom room)
		{
		}

		public void ReinitSaveData(GameplayEndingManagerSaveData saveData)
		{
		}
	}
}
