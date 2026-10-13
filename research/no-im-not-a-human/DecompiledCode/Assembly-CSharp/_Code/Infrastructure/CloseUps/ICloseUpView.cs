using Cysharp.Threading.Tasks;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.CloseUps;

public interface ICloseUpView
{
	IUpdateable[] Updateables { get; }

	UniTask Show();

	UniTask Hide();

	void Init();
}
