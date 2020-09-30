#region

using System;
using System.Collections.Generic;

#endregion

public class CountLocker
{
    private readonly List<object> _lockCallers = new List<object>();
    public bool IsLocked;

    public void Lock(object sender)
    {
        if (_lockCallers.Contains(sender)) return;
        _lockCallers.Add(sender);

        if (IsLocked) return;
        IsLocked = true;
        OnLock();
    }

    public void Unlock(object sender)
    {
        if (!_lockCallers.Contains(sender)) return;
        _lockCallers.Remove(sender);

        if (_lockCallers.Count != 0) return;

        IsLocked = false;
        OnLockEmpty();
    }

    public event EventHandler LockEmpty;

    // ReSharper disable once EventNeverSubscribedTo.Global
    public event EventHandler Locked;

    private void OnLockEmpty()
    {
        LockEmpty?.Invoke(this, EventArgs.Empty);
    }

    private void OnLock()
    {
        Locked?.Invoke(this, EventArgs.Empty);
    }
}