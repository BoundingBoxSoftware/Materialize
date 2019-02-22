#region

using System.IO;
using System.Xml.Serialization;
using UnityEngine;

#endregion

public class Settings
{
    public FileFormat FileFormat;
    public bool NormalMapMaxStyle;
    public bool NormalMapMayaStyle;

    public bool PostProcessEnabled;
    public PropChannelMap PropBlue;
    public PropChannelMap PropGreen;

    public PropChannelMap PropRed;
}


public class SettingsGui : MonoBehaviour
{
    private const string SettingsKey = "Settings";
    public static SettingsGui Instance;
    private static readonly int FlipNormalY = Shader.PropertyToID("_FlipNormalY");
    private bool _windowOpen;

    private Rect _windowRect = new Rect(Screen.width - 300, Screen.height - 320, 280, 600);
    public PostProcessGui PostProcessGui;
    [HideInInspector] public Settings Settings = new Settings();

    private void Start()
    {
        Instance = this;

        LoadSettings();
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(SettingsKey))
        {
            var set = PlayerPrefs.GetString(SettingsKey);
            var serializer = new XmlSerializer(typeof(Settings));
            using (TextReader sr = new StringReader(set))
            {
                Settings = serializer.Deserialize(sr) as Settings;
            }
        }
        else
        {
            Settings.NormalMapMaxStyle = true;
            Settings.NormalMapMayaStyle = false;
            Settings.PostProcessEnabled = true;
            Settings.PropRed = PropChannelMap.None;
            Settings.PropGreen = PropChannelMap.None;
            Settings.PropBlue = PropChannelMap.None;
            Settings.FileFormat = FileFormat.Png;
            SaveSettings();
        }

        SetSettings();
    }

    private void SaveSettings()
    {
        var serializer = new XmlSerializer(typeof(Settings));
        using (TextWriter sw = new StringWriter())
        {
            serializer.Serialize(sw, Settings);
            PlayerPrefs.SetString(SettingsKey, sw.ToString());
        }
    }

    private void SetNormalMode()
    {
        var flipNormalY = 0;
        if (Settings.NormalMapMayaStyle) flipNormalY = 1;

        Shader.SetGlobalInt(FlipNormalY, flipNormalY);
    }

    public void SetSettings()
    {
        SetNormalMode();

        if (Settings.PostProcessEnabled)
            PostProcessGui.PostProcessOn();
        else
            PostProcessGui.PostProcessOff();

        var mainGui = MainGui.Instance;
        mainGui.PropRed = Settings.PropRed;
        mainGui.PropGreen = Settings.PropGreen;
        mainGui.PropBlue = Settings.PropBlue;

        mainGui.SetFormat(Settings.FileFormat);
    }

    private void DoMyWindow(int windowId)
    {
        const int offsetX = 10;
        var offsetY = 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Normal Map Style");

        offsetY += 30;

        Settings.NormalMapMaxStyle =
            GUI.Toggle(new Rect(offsetX, offsetY, 100, 30), Settings.NormalMapMaxStyle, " Max Style");
        Settings.NormalMapMayaStyle = !Settings.NormalMapMaxStyle;


        Settings.NormalMapMayaStyle = GUI.Toggle(new Rect(offsetX + 100, offsetY, 100, 30), Settings.NormalMapMayaStyle,
            " Maya Style");
        Settings.NormalMapMaxStyle = !Settings.NormalMapMayaStyle;

        offsetY += 30;

        Settings.PostProcessEnabled = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), Settings.PostProcessEnabled,
            " Enable Post Process By Default");

        offsetY += 20;
        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Limit Frame Rate");

        offsetY += 20;

        if (GUI.Button(new Rect(offsetX + 40, offsetY, 30, 30), "30"))
        {
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
            PlayerPrefs.SetInt("targetFrameRate", 30);
            PlayerPrefs.SetInt("Vsync", 0);
        }
        if (GUI.Button(new Rect(offsetX + 80, offsetY, 30, 30), "60"))
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            PlayerPrefs.SetInt("targetFrameRate", 60);
            PlayerPrefs.SetInt("Vsync", 0);
        }
        if (GUI.Button(new Rect(offsetX + 120, offsetY, 30, 30), "120"))
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 0;
            PlayerPrefs.SetInt("targetFrameRate", 120);
            PlayerPrefs.SetInt("Vsync", 0);
        }

        if (GUI.Button(new Rect(offsetX + 120, offsetY, 30, 30), "None"))
        {
            //Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;
           // PlayerPrefs.SetInt("targetFrameRate", 30);
            PlayerPrefs.SetInt("Vsync", 1);
        }

        offsetY += 40;


        if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default Property Map Channels"))
        {
            Settings.PropRed = MainGui.Instance.PropRed;
            Settings.PropGreen = MainGui.Instance.PropGreen;
            Settings.PropBlue = MainGui.Instance.PropBlue;
        }

        offsetY += 30;

        if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default File Format"))
            Settings.FileFormat = FileFormat.Png;

        offsetY += 40;

        if (GUI.Button(new Rect(offsetX + 140, offsetY, 120, 30), "Save and Close"))
        {
            SaveSettings();
            SetNormalMode();
            _windowOpen = false;
        }

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect = new Rect(Screen.width - 300, Screen.height - 320, 280, 230);

        if (_windowOpen) _windowRect = GUI.Window(20, _windowRect, DoMyWindow, "Setting and Preferences");

        if (!GUI.Button(new Rect(Screen.width - 280, Screen.height - 40, 80, 30), "Settings")) return;
        if (_windowOpen)
        {
            SaveSettings();
            _windowOpen = false;
        }
        else
        {
            _windowOpen = true;
        }
    }
}