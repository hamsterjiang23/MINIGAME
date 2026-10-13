using System;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Cutscenes
{
	public sealed class CutscenesManager : ICutscenesManager
	{
		private readonly CutsceneData[] _cutscenes;

		private readonly UnityEvent _onEnd;

		private CutsceneData _activeCutscene;

		private readonly INotAHumanSoundService _soundService;

		[CompilerGenerated]
		private Action<ECutscene> m_CutsceneEnded;

		[CompilerGenerated]
		private Action<ECutscene> m_CutsceneStarted;

		public bool IsShowingCutscene { get; private set; }

		public event Action<ECutscene> CutsceneEnded
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

		public event Action<ECutscene> CutsceneStarted
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

		public CutscenesManager(ICutscenesDataProvider cutscenesDataProvider, INotAHumanSoundService soundService)
		{
		}

		public void ShowCutscene(ECutscene cutscene)
		{
		}

		private void InvokeExtraActions(ECutscene cutscene)
		{
		}

		private void OnEnd()
		{
		}
	}
}
