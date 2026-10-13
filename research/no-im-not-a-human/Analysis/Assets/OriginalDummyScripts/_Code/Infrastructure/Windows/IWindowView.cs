using System;
using _Code.DialogSystem;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Sound;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Windows
{
	public interface IWindowView
	{
		bool CanLeave { get; }

		event Action<WindowView> StartedOpening;

		event Action<WindowView> Opened;

		event Action<WindowView> Closed;

		void InitImages(int day);

		void StartOpen();

		void Close();

		void RenewDialogs();

		void Init(IDayNightController dayNightController, IDialogManager dialogManager, IWindowsSODataProvider dataProvider, INotAHumanSoundService soundService, IDataModelService dataModelService);

		void EnableSound(ESound sound);

		void DisableSound();
	}
}
