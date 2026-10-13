using System;
using Cysharp.Threading.Tasks;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Mushroomlist;

public sealed class MushroomlistCloseUpView : ACloseUpView
{
	public override IUpdateable[] Updateables => new IUpdateable[1] { this };

	public event Action Closed;

	public override async UniTask Show()
	{
		base.Show();
		HUDPresenter.SetupAndShowControlsView(EControlsList.InUsualCloseUp);
	}

	public override async UniTask Hide()
	{
		base.Hide();
		HUDPresenter.SetupAndShowControlsView(EControlsList.InRoom);
		Closed?.Invoke();
	}
}
