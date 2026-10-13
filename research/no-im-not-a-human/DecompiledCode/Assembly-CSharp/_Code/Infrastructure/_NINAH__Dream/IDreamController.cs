using Cysharp.Threading.Tasks;

namespace _Code.Infrastructure._NINAH__Dream;

public interface IDreamController
{
	bool NeedToShowDream { get; }

	UniTask ShowDream();

	void WantToShowDream(EDream dream);

	bool HasSeen(EDream dream);
}
