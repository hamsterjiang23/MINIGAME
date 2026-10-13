using UnityEngine;

namespace _Code.Infrastructure.StateObjects;

public sealed class StateObjectsViewProvider : MonoBehaviour, IStateObjectViewProvider
{
	[field: SerializeField]
	public StateObjet[] StateObjets { get; private set; }
}
