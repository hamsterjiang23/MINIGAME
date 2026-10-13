using System;
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

		public event Action Ended;

		public event Action Started;

		public void Init(IDialogManager dialogManager, IDataModelService dataModelService)
		{
			_dialogManager = dialogManager;
			((RectTransform)base.transform).anchoredPosition = _position;
			_uiButton.SetAction(() =>
			{
				dataModelService.MetaPrefsDataHandler.WatchNarrativeObject(_objectType);
				dialogManager.ShowSubtitle(_nodeName);
				dialogManager.DialogEnded += OnEnded;
				Started?.Invoke();
			});
		}

		private void OnEnded(bool isDialog, bool isSubtitle)
		{
			_dialogManager.DialogEnded -= OnEnded;
			Ended?.Invoke();
		}

		[Button("Copy Position")]
		public void CopyPosition()
		{
			_position = ((RectTransform)base.transform).anchoredPosition;
		}
	}
}
