using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPropertyMap : MonoBehaviour
{
    public string DropDownType;
    public Dropdown RedMap;
    public Dropdown BlueMap;
    public Dropdown GreenMap;
    public Dropdown AlphaMap;


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
                setChannels(1, 0, 4, 2);
                break;
            case 2:
                setChannels(2, 4, 1, 0);
                break;
            default:
                break;
        }
    }
    private void setChannels(int red, int blue, int green, int alpha)
    {

        RedMap.value = red;
        BlueMap.value = blue;
        GreenMap.value = green;
        AlphaMap.value = alpha;
        //RedMap.SetPropertyMapSelection(red);
        //BlueMap.SetPropertyMapSelection(blue);
        //GreenMap.SetPropertyMapSelection(green);
        //AlphaMap.SetPropertyMapSelection(alpha);

    }
}
