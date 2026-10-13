using UnityEngine;
using _Code.Infrastructure._NINAH__InteractableObjects.Objects;

namespace _Code.Infrastructure;

public sealed class StrangeMorningInteractablesViewProvider : MonoBehaviour, IStrangeMorningInteracablesViewProvider
{
	[field: SerializeField]
	public CloseSceneInteractable CloseSceneInteractable { get; private set; }
}
