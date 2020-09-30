#region

using System;
using UnityEngine;

#endregion

public static class GuiHelper
{
    private static string FloatToString(float num, int length)
    {
        var numString = num.ToString();
        var numStringLength = numString.Length;
        var lastIndex = Mathf.FloorToInt(Mathf.Min(numStringLength, (float) length));

        return numString.Substring(0, lastIndex);
    }

    public static bool Slider(Rect rect, string title, float value, string textValue, out float outValue,
        out string outTextValue, float minValue, float maxValue)
    {
        
        if (textValue == null) textValue = value.ToString();

        var offsetX = (int) rect.x;
        var offsetY = (int) rect.y;

        GUI.Label(new Rect(rect.x, rect.y, 250, 30), title);
        offsetY += 20;

        var isChanged = false;

        var tempValue = value;
        value = GUI.HorizontalSlider(new Rect(offsetX, offsetY, rect.width - 60, 10), value, minValue, maxValue);
        if (Math.Abs(value - tempValue) > 0.001f)
        {
            textValue = FloatToString(value, 6);
            isChanged = true;
        }

        var handler = Time.time.ToString();
        GUI.SetNextControlName(handler);
        textValue = GUI.TextField(new Rect(offsetX + rect.width - 50, offsetY - 5, 50, 20), textValue);
        if (Event.current.type == EventType.KeyDown && Event.current.character == '\n' &&
            GUI.GetNameOfFocusedControl() == handler)
        {
            if (textValue.Contains("."))
            {
                textValue = textValue.Replace(".", ",");
            }

            float.TryParse(textValue, out value);
            value = Mathf.Clamp(value, minValue, maxValue);
            textValue = FloatToString(value, 6);

            if (Math.Abs(value - tempValue) > 0.0001f) isChanged = true;
        }

        float floatValue = 0.0f;
        float.TryParse(textValue, out floatValue);
        if(floatValue != value)
        {
            value = floatValue;
        }
        outValue = value;
        outTextValue = textValue;

        return isChanged;
    }

    // Value is an int
    public static bool Slider(Rect rect, string title, int value, string textValue, out int outValue,
        out string outTextValue, int minValue, int maxValue)
    {
        if (textValue == null) textValue = value.ToString();

        var offsetX = (int) rect.x;
        var offsetY = (int) rect.y;

        GUI.Label(new Rect(rect.x, rect.y, 250, 30), title);
        offsetY += 20;

        var isChanged = false;

        float tempValue = value;
        value = (int) GUI.HorizontalSlider(new Rect(offsetX, offsetY, rect.width - 60, 10), value, minValue, maxValue);
        if (Math.Abs(value - tempValue) > 0.0001f)
        {
            textValue = FloatToString(value, 6);
            isChanged = true;
        }

        var handler = Time.time.ToString();
        GUI.SetNextControlName(handler);
        textValue = GUI.TextField(new Rect(offsetX + rect.width - 50, offsetY - 5, 50, 20), textValue);
        if (Event.current.type == EventType.KeyDown && Event.current.character == '\n' &&
            GUI.GetNameOfFocusedControl() == handler)
        {
            if (textValue.Contains("."))
            {
                textValue = textValue.Replace(".", ",");
            }

            float.TryParse(textValue, out var preValue);
            value = (int) preValue;
            value = Mathf.Clamp(value, minValue, maxValue);
            textValue = value.ToString();

            if (Math.Abs(value - tempValue) > 0.001f) isChanged = true;
        }

        outValue = value;
        outTextValue = textValue;

        return isChanged;
    }

    // No Title, Value is a float
    public static bool Slider(Rect rect, float value, string textValue, out float outValue,
        out string outTextValue,
        float minValue, float maxValue)
    {
        if (textValue == null) textValue = value.ToString();

        var offsetX = (int) rect.x;
        var offsetY = (int) rect.y;

        var isChanged = false;

        var tempValue = value;
        value = GUI.HorizontalSlider(new Rect(offsetX, offsetY, rect.width - 60, 10), value, minValue, maxValue);
        if (Math.Abs(value - tempValue) > 0.001f)
        {
            textValue = FloatToString(value, 6);
            isChanged = true;
        }

        var handler = Time.time.ToString();
        GUI.SetNextControlName(handler);
        textValue = GUI.TextField(new Rect(offsetX + rect.width - 50, offsetY - 5, 50, 20), textValue);
        if (Event.current.type == EventType.KeyDown && Event.current.character == '\n' &&
            GUI.GetNameOfFocusedControl() == handler)
        {
            if (textValue.Contains("."))
            {
                textValue = textValue.Replace(".", ",");
            }

            float.TryParse(textValue, out value);
            value = Mathf.Clamp(value, minValue, maxValue);
            textValue = FloatToString(value, 6);

            if (Math.Abs(value - tempValue) > 0.0001f) isChanged = true;
        }

        outValue = value;
        outTextValue = textValue;

        return isChanged;
    }

    // No Title, Value is an int
    public static bool Slider(Rect rect, int value, string textValue, out int outValue, out string outTextValue,
        int minValue, int maxValue)
    {
        if (textValue == null) textValue = value.ToString();

        var offsetX = (int) rect.x;
        var offsetY = (int) rect.y;

        var isChanged = false;

        float tempValue = value;
        value = (int) GUI.HorizontalSlider(new Rect(offsetX, offsetY, rect.width - 60, 10), value, minValue, maxValue);
        if (Math.Abs(value - tempValue) > 0.0001f)
        {
            textValue = FloatToString(value, 6);
            isChanged = true;
        }

        var handler = Time.time.ToString();
        GUI.SetNextControlName(handler);
        textValue = GUI.TextField(new Rect(offsetX + rect.width - 50, offsetY - 5, 50, 20), textValue);
        if (Event.current.type == EventType.KeyDown && Event.current.character == '\n' &&
            GUI.GetNameOfFocusedControl() == handler)
        {
            if (textValue.Contains("."))
            {
                textValue = textValue.Replace(".", ",");
            }

            float.TryParse(textValue, out var preValue);
            value = (int) preValue;
            value = (int) Mathf.Clamp(value, minValue, (float) maxValue);
            textValue = value.ToString();

            if (Math.Abs(value - tempValue) > 0.0001f) isChanged = true;
        }

        outValue = value;
        outTextValue = textValue;

        return isChanged;
    }


    public static bool VerticalSlider(Rect rect, float value, out float outValue, float minValue, float maxValue,
        bool doStuff)
    {
        var isChanged = false;

        var tempValue = value;
        value = GUI.VerticalSlider(rect, value, minValue, maxValue);
        if (Math.Abs(value - tempValue) > 0.0001f || doStuff) isChanged = true;

        outValue = value;

        return isChanged;
    }

    public static bool Toggle(Rect rect, bool value, out bool outValue, string text, bool doStuff)
    {
        var isChanged = false;

        var tempValue = value;
        value = GUI.Toggle(rect, value, text);
        if (value != tempValue || doStuff) isChanged = true;

        outValue = value;

        return isChanged;
    }
}