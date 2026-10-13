using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using _Code.DialogSystem;
using _Scripts.Services.DataModel;

namespace _Code.Rooms
{
	public sealed class NarrativeRoomObject : MonoBehaviour
	{
		[SerializeField]
		private ENarrativeObject _objectType;

		[SerializeField]
		private UIButton _uiButton;

		[SerializeField]
		private string _nodeName;

		[SerializeField]
		private Vector2 _position;

		private IDialogManager _dialogManager;

		[CompilerGenerated]
		private Action m_Ended;

		[CompilerGenerated]
		private Action m_Started;

		public event Action Ended
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action Started
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public void Init(IDialogManager dialogManager, IDataModelService dataModelService)
		{
		}

		private void OnEnded(bool isDialog, bool isSubtitle)
		{
		}

		[Button("Copy Position")]
		public void CopyPosition()
		{
		}
	}
}
