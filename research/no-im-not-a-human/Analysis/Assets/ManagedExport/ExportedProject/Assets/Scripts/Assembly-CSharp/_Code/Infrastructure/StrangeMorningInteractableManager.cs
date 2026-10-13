using System;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__InteractableObjects.Objects;
using _Code.Menues.HUD;
using _Code.Player;

namespace _Code.Infrastructure
{
	public sealed class StrangeMorningInteractableManager : IInteractablesManager
	{
		private CloseSceneInteractable _closeSceneInteractable;

		public IUpdateable[] Updateables => new IUpdateable[0];

		public void DisableAll()
		{
			throw new NotImplementedException();
		}

		public StrangeMorningInteractableManager(IStrangeMorningInteracablesViewProvider viewProvider, IHUDPresenter hudPresenter, IPauseController pauseController, IInputHandlerProvider inputHandlerProvider, CommonEnumEventus commonEnumEventus)
		{
			_closeSceneInteractable = viewProvider.CloseSceneInteractable;
			_closeSceneInteractable.PreInit(inputHandlerProvider);
			_closeSceneInteractable.Init(hudPresenter, pauseController, commonEnumEventus);
		}
	}
}
