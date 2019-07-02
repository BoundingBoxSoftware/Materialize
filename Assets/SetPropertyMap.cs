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

    public void SetPreset(int selection)
    {
        switch (selection)
        {
            case 0:

                break;
            case 1:

                break;
            case 2:

                break;
            default:
                break;
        }
    }
    private void setChannels(int red, int blue, int green, int alpha)
    {

    }
}
