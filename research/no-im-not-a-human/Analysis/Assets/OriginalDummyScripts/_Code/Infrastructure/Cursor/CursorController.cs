using System;
using System.Runtime.CompilerServices;
using Zenject;

namespace _Code.Infrastructure.Cursor
{
	public sealed class CursorController : ICursorController, ITickable
	{
		private CursorSOData _data;

		private ECursorType _currentType;

		private bool _isLocked;

		private bool _canGoThrough;

		[CompilerGenerated]
		private Action m_Locked;

		[CompilerGenerated]
		private Action<bool> m_Unlocked;

		public event Action Locked
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<bool> Unlocked
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public CursorController(ICursorDataProvider dataProvider)
		{
		}

		public void Lock()
		{
		}

		public void Unlock()
		{
		}

		public void SetThrough(bool isThrough)
		{
		}

		public void SetType(ECursorType type)
		{
		}

		public void Reset()
		{
		}

		public void Tick()
		{
		}
	}
}
