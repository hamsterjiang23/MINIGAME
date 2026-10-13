using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure._NINAH__InteractableObjects.Objects.Extra;
using _Code.Menues.HUD;
using _Code.Player;

namespace _Code.Infrastructure._NINAH__InteractableObjects.Objects
{
	public sealed class CalendarInteractable : ZoomInteractable
	{
		[SerializeField]
		private LocalizeStringEvent _holidayName;

		[SerializeField]
		private MeshRenderer _holidayPaperSheet;

		[SerializeField]
		private CalendarListData[] _calendarDays;

		private IDayNightController _dayNightController;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IDayNightController dayNightController, IInputHandlerProvider inputHandlerProvider)
		{
			_dayNightController = dayNightController;
			_hudPresenter = hudPresenter;
			_inputHandler = inputHandlerProvider.InputHandler;
			_dayNightController.DayChanged -= RefreshPage;
			_dayNightController.DayChanged += RefreshPage;
			RefreshPage(_dayNightController.Day);
		}

		private void RefreshPage(int day)
		{
			_holidayPaperSheet.material.mainTexture = _calendarDays[day].HolidayPaperSheetMaterial.texture;
			_holidayName.StringReference = _calendarDays[day].HolidayName;
		}

		public override void OnLoad()
		{
			OnLoadAsync().Forget();
		}

		private async UniTaskVoid OnLoadAsync()
		{
			await UniTask.WaitUntil(() => _dayNightController != null);
			RefreshPage(_dayNightController.Day);
		}
	}
}
