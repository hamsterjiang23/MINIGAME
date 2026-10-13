using UnityEngine;
using UnityEngine.UI;

namespace _Code.DialogSystem;

public abstract class ADialogSignElementView : MonoBehaviour
{
	[field: SerializeField]
	public Image Content { get; private set; }
}
