using System.Collections.Generic;
using UnityEngine;

namespace LuminaryLabs.Tools;

public class MissingDependencyEntry
{
	public GameObject GameObject;

	public Dictionary<Component, List<MissingReferenceItem>> ComponentMissingItems = new Dictionary<Component, List<MissingReferenceItem>>();
}
