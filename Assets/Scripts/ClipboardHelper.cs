using System;
using System.Reflection;
using UnityEngine;

public class ClipboardHelper
{
    private static PropertyInfo m_systemCopyBufferProperty;

    public static string clipBoard
    {
        get
        {
            //PropertyInfo P = GetSystemCopyBufferProperty();
            //return (string)P.GetValue(null,null);
            return GUIUtility.systemCopyBuffer;
        }
        set
        {
            //PropertyInfo P = GetSystemCopyBufferProperty();
            //P.SetValue(null,value,null);
            GUIUtility.systemCopyBuffer = value;
        }
    }

    private static PropertyInfo GetSystemCopyBufferProperty()
    {
        if (m_systemCopyBufferProperty == null)
        {
            var T = typeof(GUIUtility);
            m_systemCopyBufferProperty =
                T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            if (m_systemCopyBufferProperty == null)
                throw new Exception(
                    "Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
        }

        return m_systemCopyBufferProperty;
    }
}