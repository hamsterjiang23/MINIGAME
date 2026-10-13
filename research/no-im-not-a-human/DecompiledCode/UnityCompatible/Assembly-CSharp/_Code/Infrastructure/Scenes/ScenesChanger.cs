using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace _Code.Infrastructure.Scenes
{
	public static class ScenesChanger
	{
		public static async UniTask OpenScene(int index)
		{
			await SceneManager.LoadSceneAsync(2, LoadSceneMode.Single).ToUniTask();
			await SceneManager.LoadSceneAsync(index, LoadSceneMode.Single).ToUniTask();
		}
	}
}
