using System;
using System.Runtime.CompilerServices;

namespace _Code.Infrastructure.Cursor
{
	public sealed class MockCursorController : ICursorController
	{
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

		public void SetLockedState(bool isLocked)
		{
		}

		public void Lock(ECursorLockerCause cause)
		{
		}

		public void Unlock(ECursorLockerCause cause)
		{
		}

		public void MakeVisible(ECursorLockerCause cause)
		{
		}

		public void MakeInvisible(ECursorLockerCause cause)
		{
		}

		public void SetVisibleState(bool isVisible)
		{
		}

		public void Lock()
		{
		}

		public void Unlock()
		{
		}

		public void SetType(ECursorType type)
		{
		}

		public void Reset()
		{
		}

		public void SetThrough(bool isThrough)
		{
		}
	}
}
