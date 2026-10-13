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

		public IUpdateable[] Updateables => null;

		public void DisableAll()
		{
		}

		public StrangeMorningInteractableManager(IStrangeMorningInteracablesViewProvider viewProvider, IHUDPresenter hudPresenter, IPauseController pauseController, IInputHandlerProvider inputHandlerProvider, CommonEnumEventus commonEnumEventus)
		{
		}
	}
}
