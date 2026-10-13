using System;

namespace _Code.Infrastructure.Cursor
{
	public sealed class MockCursorController : ICursorController
	{
		public event Action Locked;

		public event Action<bool> Unlocked;

		public void SetLockedState(bool isLocked)
		{
			throw new NotImplementedException();
		}

		public void Lock(ECursorLockerCause cause)
		{
			throw new NotImplementedException();
		}

		public void Unlock(ECursorLockerCause cause)
		{
			throw new NotImplementedException();
		}

		public void MakeVisible(ECursorLockerCause cause)
		{
			throw new NotImplementedException();
		}

		public void MakeInvisible(ECursorLockerCause cause)
		{
			throw new NotImplementedException();
		}

		public void SetVisibleState(bool isVisible)
		{
			throw new NotImplementedException();
		}

		public void Lock()
		{
			throw new NotImplementedException();
		}

		public void Unlock()
		{
			throw new NotImplementedException();
		}

		public void SetType(ECursorType type)
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void SetThrough(bool isThrough)
		{
			throw new NotImplementedException();
		}
	}
}
