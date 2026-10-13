using System;

namespace _Code.Infrastructure.Cursor;

public interface ICursorController
{
	event Action Locked;

	event Action<bool> Unlocked;

	void Lock();

	void Unlock();

	void SetType(ECursorType type);

	void Reset();

	void SetThrough(bool isThrough);
}
