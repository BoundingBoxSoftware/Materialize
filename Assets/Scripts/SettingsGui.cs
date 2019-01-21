using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class Settings
{
    public FileFormat fileFormat;
    public bool normalMapMaxStyle;
    public bool normalMapMayaStyle;

    public bool postProcessEnabled;
    public PropChannelMap propBlue;
    public PropChannelMap propGreen;

    public PropChannelMap propRed;
}


public class SettingsGui : MonoBehaviour
{
    public static SettingsGui instance;
    public MainGui mainGui;

    private char pathChar;
    public PostProcessGui postProcessGui;
    public Settings settings = new Settings();
    private readonly string SettingsKey = "Settings";
    private bool windowOpen;

    private Rect windowRect = new Rect(Screen.width - 300, Screen.height - 320, 280, 600);

    // Use this for initialization
    private void Start()
    {
        instance = this;

        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer)
            pathChar = '\\';
        else
            pathChar = '/';

        LoadSettings();
    }

    private string GetPathToFile()
    {
        var pathToFile = Application.dataPath;
        //string pathToFile = Application.persistentDataPath;

        if (Application.isEditor)
            pathToFile = pathToFile + "/settings.txt";
        else
            pathToFile = pathToFile.Substring(0, pathToFile.Length - 16) + "settings.txt";

        return pathToFile;
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(SettingsKey))
        {
            var set = PlayerPrefs.GetString(SettingsKey);
            var serializer = new XmlSerializer(typeof(Settings));
            using (TextReader sr = new StringReader(set))
            {
                settings = serializer.Deserialize(sr) as Settings;
            }
        }
        else
        {
            settings.normalMapMaxStyle = true;
            settings.normalMapMayaStyle = false;
            settings.postProcessEnabled = true;
            settings.propRed = PropChannelMap.None;
            settings.propGreen = PropChannelMap.None;
            settings.propBlue = PropChannelMap.None;
            settings.fileFormat = FileFormat.png;
            SaveSettings();
        }

        SetSettings();
    }

    private void SaveSettings()
    {
        var serializer = new XmlSerializer(typeof(Settings));
        var set = "";
        using (TextWriter sw = new StringWriter())
        {
            serializer.Serialize(sw, settings);
            PlayerPrefs.SetString(SettingsKey, sw.ToString());
        }
    }

    private void SetNormalMode()
    {
        var flipNormalY = 0;
        if (settings.normalMapMayaStyle) flipNormalY = 1;

        Shader.SetGlobalInt("_FlipNormalY", flipNormalY);
    }

    public void SetSettings()
    {
        SetNormalMode();

        if (settings.postProcessEnabled)
            postProcessGui.PostProcessOn();
        else
            postProcessGui.PostProcessOff();

        mainGui.PropRed = settings.propRed;
        mainGui.PropGreen = settings.propGreen;
        mainGui.PropBlue = settings.propBlue;

        mainGui.SetFormat(settings.fileFormat);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void DoMyWindow(int windowID)
    {
        var offsetX = 10;
        var offsetY = 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Normal Map Style");

        offsetY += 30;

        settings.normalMapMaxStyle =
            GUI.Toggle(new Rect(offsetX, offsetY, 100, 30), settings.normalMapMaxStyle, " Max Style");
        if (settings.normalMapMaxStyle)
            settings.normalMapMayaStyle = false;
        else
            settings.normalMapMayaStyle = true;


        settings.normalMapMayaStyle = GUI.Toggle(new Rect(offsetX + 100, offsetY, 100, 30), settings.normalMapMayaStyle,
            " Maya Style");
        if (settings.normalMapMayaStyle)
            settings.normalMapMaxStyle = false;
        else
            settings.normalMapMaxStyle = true;

        offsetY += 30;

        settings.postProcessEnabled = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), settings.postProcessEnabled,
            " Enable Post Process By Default");

        offsetY += 30;

        if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default Property Map Channels"))
        {
            settings.propRed = mainGui.PropRed;
            settings.propGreen = mainGui.PropGreen;
            settings.propBlue = mainGui.PropBlue;
        }

        offsetY += 30;

        if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default File Format"))
            settings.fileFormat = FileFormat.png;

        offsetY += 40;

        if (GUI.Button(new Rect(offsetX + 140, offsetY, 120, 30), "Save and Close"))
        {
            SaveSettings();
            SetNormalMode();
            windowOpen = false;
        }

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        windowRect = new Rect(Screen.width - 300, Screen.height - 320, 280, 230);

        if (windowOpen) windowRect = GUI.Window(20, windowRect, DoMyWindow, "Setting and Preferences");

        if (GUI.Button(new Rect(Screen.width - 280, Screen.height - 40, 80, 30), "Settings"))
        {
            if (windowOpen)
            {
                SaveSettings();
                windowOpen = false;
            }
            else
            {
                windowOpen = true;
            }
        }
    }
}