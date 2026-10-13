using UnityEngine.Timeline;

namespace _Code.Infrastructure.Cutscenes;

public interface ICutscenesDataProvider
{
	SignalAsset EndSignal { get; }

	SignalReceiver SignalReceiver { get; }

	CutsceneData[] Cutscenes { get; }
}
