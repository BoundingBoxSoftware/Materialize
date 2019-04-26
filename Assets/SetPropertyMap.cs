using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPropertyMap : MonoBehaviour
{
    public string DropDownType;   
    public void SetPropertyMapSelection(int selection)
    {
        MainGui.Instance.MapSelection(selection, DropDownType);
    }
}
