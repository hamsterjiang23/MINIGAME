using System;

namespace _Code.Infrastructure.Cutscenes
{
	public interface ICutscenesManager
	{
		bool IsShowingCutscene { get; }

		event Action<ECutscene> CutsceneEnded;

		event Action<ECutscene> CutsceneStarted;

		void ShowCutscene(ECutscene cutscene);
	}
}
