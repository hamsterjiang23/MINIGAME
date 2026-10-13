using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Showcase
{
	public sealed class ShowcaseManager : IShowcaseManager, IUpdateable
	{
		private const float TIME_TO_RESTART = 600f;

		private float _restartTimer;

		private ICursorController _cursorController;

		public IUpdateable Updateable => this;

		public event Action Restarted;

		public event Action ChangedLanguage;

		public ShowcaseManager(ICursorController cursorController)
		{
			_cursorController = cursorController;
			Init();
		}

		private void Init()
		{
			Restarted -= Restart;
			Restarted += Restart;
		}

		private void Restart()
		{
			_cursorController.Reset();
			SceneManager.LoadScene(0);
		}

		public void OnUpdateAction()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				Restarted?.Invoke();
			}
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				ChangedLanguage?.Invoke();
			}
			ProcessRestartTimer();
		}

		private void ProcessRestartTimer()
		{
			if (Input.anyKeyDown)
			{
				_restartTimer = 0f;
			}
			_restartTimer += Time.deltaTime;
			if (_restartTimer >= 600f)
			{
				Restarted?.Invoke();
			}
		}
	}
}
