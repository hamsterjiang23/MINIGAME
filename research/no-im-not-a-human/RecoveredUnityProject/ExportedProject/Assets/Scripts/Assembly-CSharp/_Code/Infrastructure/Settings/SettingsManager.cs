using Zenject;
using _Code.Infrastructure.Cursor;
using _Code.Player;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Settings
{
	public sealed class SettingsManager : ISettingsManager, IInitializable
	{
		private readonly SettingsInstance _settingsInstance;

		public SettingsManager(ISettingsInstanceProvider settingsInstanceProvider, IDataModelService dataModelService, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController)
		{
			_settingsInstance = settingsInstanceProvider.SettingsInstance;
			_settingsInstance.Init(dataModelService, inputHandlerProvider, cursorController);
		}

		public void Initialize()
		{
			_settingsInstance.Initialize();
		}
	}
}
