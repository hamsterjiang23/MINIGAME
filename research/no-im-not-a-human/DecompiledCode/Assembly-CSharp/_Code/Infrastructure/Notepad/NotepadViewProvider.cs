using UnityEngine;

namespace _Code.Infrastructure.Notepad;

public sealed class NotepadViewProvider : MonoBehaviour, INotepadViewProvider
{
	[field: SerializeField]
	public NotepadView NotepadView { get; private set; }
}
