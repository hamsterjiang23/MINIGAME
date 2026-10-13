using UnityEngine;
using _Code.Infrastructure._NINAH__Endings.View;

namespace _Code.Infrastructure.Endings.View;

public sealed class EndingViewProvider : MonoBehaviour, IEndingViewProvider
{
	[field: SerializeField]
	public EndingView EndingView { get; private set; }
}
