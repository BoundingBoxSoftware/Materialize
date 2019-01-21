using UnityEngine;

public class ControlsGui : MonoBehaviour
{
    public static SettingsGui instance;
    public Settings settings = new Settings();
    private bool windowOpen;

    private Rect windowRect = new Rect(Screen.width - 520, Screen.height - 320, 300, 600);

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void DoMyWindow(int windowID)
    {
        var offsetX = 10;
        var offsetY = 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Rotate Model");
        offsetY += 20;
        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Right Mouse Button");
        offsetY += 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Move Model");
        offsetY += 20;
        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Middle Mouse Button");
        offsetY += 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Zoom In/Out");
        offsetY += 20;
        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Mouse Scroll Wheel");
        offsetY += 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Rotate Light");
        offsetY += 20;
        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Middle Mouse Button + L");
        offsetY += 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Rotate Background");
        offsetY += 20;
        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Middle Mouse Button + B");
        offsetY += 30;

        if (GUI.Button(new Rect(offsetX + 160, offsetY, 120, 30), "Close")) windowOpen = false;
    }

    private void OnGUI()
    {
        windowRect = new Rect(Screen.width - 480, Screen.height - 370, 170, 280);

        if (windowOpen) windowRect = GUI.Window(22, windowRect, DoMyWindow, "Controls");

        if (GUI.Button(new Rect(Screen.width - 370, Screen.height - 40, 80, 30), "Controls"))
        {
            if (windowOpen)
                windowOpen = false;
            else
                windowOpen = true;
        }
    }
}