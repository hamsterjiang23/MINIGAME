using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using _Code.Characters;
using _Code.Events;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;

namespace _Code.Infrastructure._NINAH__DayNight
{
	public sealed class MockDayNightController : IDayNightController
	{
		[CompilerGenerated]
		private Action m_WatchedTV;

		[CompilerGenerated]
		private Action<ETimeOfDay> m_Changed;

		[CompilerGenerated]
		private Action<int> m_DayChanged;

		[CompilerGenerated]
		private Action<List<ChangePoseData>> m_PosesChanged;

		[CompilerGenerated]
		private Action m_BodyEaterAppeared;

		[CompilerGenerated]
		private Action m_BodyEaterDisappeared;

		[CompilerGenerated]
		private Action m_WentToBed;

		[CompilerGenerated]
		private Action m_WokeUp;

		public int MaxDayActions { get; }

		public int DayActions { get; }

		public int Day { get; }

		public ETimeOfDay CurrentTimeOfDay { get; }

		public float LastChange { get; }

		public bool CanLeaveRooms { get; }

		public bool IsEndingDay { get; }

		public event Action WatchedTV
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

		public event Action<ETimeOfDay> Changed
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

		public event Action<int> DayChanged
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

		public event Action<List<ChangePoseData>> PosesChanged
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

		public event Action BodyEaterAppeared
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

		public event Action BodyEaterDisappeared
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

		public event Action WentToBed
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

		public event Action WokeUp
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

		public void Act()
		{
		}

		public void UpdateTimeOfDayStartedOrDialogEndedTime()
		{
		}

		public bool HasCompletedDaytimeGoal()
		{
			return false;
		}

		public void ActAll()
		{
		}

		public void Change()
		{
		}

		public UniTaskVoid SetDayForEnding(EEnding ending)
		{
			return default;
		}

		public void AddEnergySlot()
		{
		}

		public void AddEnergy()
		{
		}

		public void RefillEnergy()
		{
		}

		public void RemoveExtraEnergySlotsForTomorrow(int energyCount)
		{
		}

		public void AddChangePosTomorrow(ECharacterType character, ERoomPeopleState pose)
		{
		}
	}
}
