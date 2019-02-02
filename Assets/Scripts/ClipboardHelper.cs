#region

using UnityEngine;

#endregion

public static class ClipboardHelper
{
    public static string ClipBoard
    {
        get => GUIUtility.systemCopyBuffer;
        set => GUIUtility.systemCopyBuffer = value;
    }
}