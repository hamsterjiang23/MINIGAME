using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure._NINAH__Dream
{
	[Serializable]
	public sealed class DreamSaveData : ASavableData
	{
		[field: SerializeField]
		public List<EDream> SeenDreams { get; set; } = new List<EDream>();
	}
}
