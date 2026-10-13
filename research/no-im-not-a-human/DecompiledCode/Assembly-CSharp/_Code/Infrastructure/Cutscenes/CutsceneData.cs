using System;
using UnityEngine;
using UnityEngine.Playables;

namespace _Code.Infrastructure.Cutscenes;

[Serializable]
public sealed class CutsceneData
{
	[field: SerializeField]
	public ECutscene Name { get; private set; }

	[field: SerializeField]
	public PlayableDirector Playable { get; private set; }
}
