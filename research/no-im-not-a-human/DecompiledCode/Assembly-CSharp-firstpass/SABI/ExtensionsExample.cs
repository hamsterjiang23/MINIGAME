using UnityEngine;

namespace SABI;

public class ExtensionsExample : MonoBehaviour
{
	private void Start()
	{
		int num = 93434;
		Debug.Log($"[C# Extensions] {num}.RoundToMultipleOf(5) : {num.RoundToMultipleOf(5)}");
		Debug.Log($"[C# Extensions] {num}.ToAbbreviatedString() : {num.ToAbbreviatedString()}");
		this.DelayedExecution(3f, () =>
		{
			Debug.Log("[C# Extensions] 3 seconds delayed execution");
		});
	}
}
