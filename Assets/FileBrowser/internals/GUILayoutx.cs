//------------------------------------------------------------------------------
// Based on ImprovedSelectionList, by Daniel Brauer:
// http://wiki.unity3d.com/index.php/ImprovedSelectionList
// Licensed under a Creative Commons Attribution-ShareAlike 3.0 Unported License.
//
// Modifications by Petrucio:
// -Added a default List Item style when one is not present in the current GUI style
// -Added callback for item selection (single click)
// -Added support for touches / mobile
// -Refactored duplicated code into a single function
// -Added Pablo Bollansée (The Oddler)'s double click fix
//------------------------------------------------------------------------------
using UnityEngine;

public class GUILayoutx {

	private static GUIStyle defaultListItemStyle = new GUIStyle(GUI.skin.label);

	public delegate void DoubleClickCallback(int index);
	public delegate void SingleClickCallback(int index);
	
	public static float maxListWidth = 10000;

	private static GUIStyle getListItemStyle(string styleStr) {
		GUIStyle style = GUI.skin.FindStyle(styleStr);
		if (style != null) return style;

		defaultListItemStyle.fixedHeight = 20;
		return defaultListItemStyle;
	}

	public static int SelectionList(int selected, GUIContent[] list) {
		return SelectionList(selected, list, getListItemStyle("List Item"), null, null);
	}
	public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle) {
		return SelectionList(selected, list, elementStyle, null, null);
	}
	public static int SelectionList(int selected, GUIContent[] list, DoubleClickCallback callback) {
		return SelectionList(selected, list, getListItemStyle("List Item"), callback, null);
	}
	public static int SelectionList(int selected, GUIContent[] list, DoubleClickCallback callback, SingleClickCallback selCallback) {
		return SelectionList(selected, list, getListItemStyle("List Item"), callback, selCallback);
	}

	public static int SelectionList(int selected, string[] list) {
		return SelectionList(selected, list, getListItemStyle("List Item"), null, null);
	}
	public static int SelectionList(int selected, string[] list, GUIStyle elementStyle) {
		return SelectionList(selected, list, elementStyle, null, null);
	}
	public static int SelectionList(int selected, string[] list, DoubleClickCallback callback) {
		return SelectionList(selected, list, getListItemStyle("List Item"), callback, null);
	}
	public static int SelectionList(int selected, string[] list, DoubleClickCallback callback, SingleClickCallback selCallback) {
		return SelectionList(selected, list, getListItemStyle("List Item"), callback, selCallback);
	}

	public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle, DoubleClickCallback callback, SingleClickCallback selCallback) {
		for (int i = 0; i < list.Length; ++i) {
			Rect elementRect = GUILayoutUtility.GetRect(list[i], elementStyle);
			if (elementRect.width > maxListWidth) elementRect.width = maxListWidth;
			
			bool hover = elementRect.Contains(Event.current.mousePosition);
			if (hover && Event.current.type == EventType.MouseDown && Event.current.clickCount == 1) {
				selected = i;
				Event.current.Use();
				if (selCallback != null) {
					selCallback(i);
				}
			} else if (hover && callback != null && Event.current.type == EventType.MouseDown && Event.current.clickCount == 2) {
				callback(i);
				Event.current.Use();
			} else if (Event.current.type == EventType.Repaint) {
				elementStyle.Draw(elementRect, list[i], hover, false, i == selected, false);
			}
		}
		return selected;
	}

	public static int SelectionList(int selected, string[] list, GUIStyle elementStyle, DoubleClickCallback callback, SingleClickCallback selCallback) {
		GUIContent[] contentList = new GUIContent[list.Length];
		for (int i = 0; i < list.Length; ++i) {
			contentList[i] = new GUIContent(list[i]);
		}		
		return SelectionList(selected, contentList, elementStyle, callback, selCallback);
	}

}

