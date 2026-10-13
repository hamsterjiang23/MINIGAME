using UnityEngine;

namespace _Code.Infrastructure.Updatable;

public sealed class UpdaterInstanceProvider : MonoBehaviour, IUpdaterInstanceProvider
{
	[field: SerializeField]
	public UpdaterInstance UpdaterInstance { get; private set; }
}
