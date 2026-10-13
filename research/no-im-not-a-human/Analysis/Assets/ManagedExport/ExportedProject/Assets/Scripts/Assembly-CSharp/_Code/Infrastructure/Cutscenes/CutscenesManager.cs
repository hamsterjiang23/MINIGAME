using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.Timeline;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Cutscenes
{
	public sealed class CutscenesManager : ICutscenesManager
	{
		private readonly CutsceneData[] _cutscenes;

		private readonly UnityEvent _onEnd = new UnityEvent();

		private CutsceneData _activeCutscene;

		private readonly INotAHumanSoundService _soundService;

		public bool IsShowingCutscene { get; private set; }

		public event Action<ECutscene> CutsceneEnded;

		public event Action<ECutscene> CutsceneStarted;

		public CutscenesManager(ICutscenesDataProvider cutscenesDataProvider, INotAHumanSoundService soundService)
		{
			_soundService = soundService;
			_cutscenes = cutscenesDataProvider.Cutscenes;
			SignalReceiver signalReceiver = cutscenesDataProvider.SignalReceiver;
			SignalAsset endSignal = cutscenesDataProvider.EndSignal;
			signalReceiver.AddReaction(endSignal, _onEnd);
			_onEnd.AddListener(OnEnd);
		}

		public void ShowCutscene(ECutscene cutscene)
		{
			CutsceneData cutsceneData = _cutscenes.FirstOrDefault((CutsceneData x) => x.Name == cutscene);
			if (cutsceneData != null && _activeCutscene != cutsceneData)
			{
				InvokeExtraActions(cutscene);
				CutsceneStarted?.Invoke(cutscene);
				IsShowingCutscene = true;
				_activeCutscene = cutsceneData;
				_activeCutscene.Playable.Play();
			}
		}

		private void InvokeExtraActions(ECutscene cutscene)
		{
			if (cutscene == ECutscene.Fek3d)
			{
				_soundService.FadeOut(ESoundSource.s1_GameScene_KnockSource, 0f);
			}
		}

		private void OnEnd()
		{
			_activeCutscene.Playable.Stop();
			CutsceneEnded?.Invoke(_activeCutscene.Name);
		}
	}
}
