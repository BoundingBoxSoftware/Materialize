using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipUIElement : MonoBehaviour
{
    public void FlipUiElement(GameObject Obj)
    {
        Obj.SetActive(!Obj.active);
    }
}
