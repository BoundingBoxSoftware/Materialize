using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;

public class Settings
{
    public bool normalMapMaxStyle;
    public bool normalMapMayaStyle;

    public bool postProcessEnabled;

    public PropChannelMap propRed;
    public PropChannelMap propGreen;
    public PropChannelMap propBlue;

    public FileFormat fileFormat;
}


public class SettingsGui : MonoBehaviour
{
    public MainGui mainGui;
    public PostProcessGui postProcessGui;

    Rect windowRect = new Rect(Screen.width - 300, Screen.height - 320, 280, 600);
    bool windowOpen = false;
    public Settings settings = new Settings();

    public static SettingsGui instance;

    char pathChar;

    // Use this for initialization
    void Start()
    {
        instance = this;

        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer)
        {
            pathChar = '\\';
        }
        else
        {
            pathChar = '/';
        }

        LoadSettings();
    }

    string GetPathToFile()
    {
        string pathToFile = Application.dataPath;
        //string pathToFile = Application.persistentDataPath;

        if (Application.isEditor)
        {
            pathToFile = pathToFile + "/settings.txt";
        }
        else
        {
            pathToFile = pathToFile.Substring(0, pathToFile.Length - 16) + "settings.txt";
        }

        return pathToFile;
    }

    public void LoadSettings()
    {
        string pathToFile = GetPathToFile();

        Debug.Log(pathToFile);
        //if (File.Exists (pathToFile)) {
        //FileAttributes fileAttributes = File.GetAttributes(pathToFile);
        //if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
        //File.SetAttributes (pathToFile, FileAttributes.Normal);
        //}
        //}

        if (System.IO.File.Exists(pathToFile))
        {
            var serializer = new XmlSerializer(typeof(Settings));
            var stream = new FileStream(pathToFile, FileMode.Open);
            settings = serializer.Deserialize(stream) as Settings;
            stream.Close();
        }
        else
        {
            settings.normalMapMaxStyle = true;
            settings.normalMapMayaStyle = false;
            settings.postProcessEnabled = true;
            settings.propRed = PropChannelMap.None;
            settings.propGreen = PropChannelMap.None;
            settings.propBlue = PropChannelMap.None;
            settings.fileFormat = FileFormat.bmp;
            SaveSettings();
        }

        SetSettings();
    }

    void SaveSettings()
    {
        string pathToFile = GetPathToFile();

        Debug.Log(pathToFile);

        if (File.Exists(pathToFile))
        {
            //FileAttributes fileAttributes = File.GetAttributes(pathToFile);
            //if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
            File.SetAttributes(pathToFile, FileAttributes.Normal);
            //}
        }

        var serializer = new XmlSerializer(typeof(Settings));
        var stream = new FileStream(pathToFile, FileMode.Create);
        serializer.Serialize(stream, settings);
        stream.Close();
    }

    void SetNormalMode()
    {
        int flipNormalY = 0;
        if (settings.normalMapMayaStyle)
        {
            flipNormalY = 1;
        }

        Shader.SetGlobalInt("_FlipNormalY", flipNormalY);
    }

    public void SetSettings()
    {
        SetNormalMode();

        if (settings.postProcessEnabled)
        {
            postProcessGui.PostProcessOn();
        }
        else
        {
            postProcessGui.PostProcessOff();
        }

        mainGui.propRed = settings.propRed;
        mainGui.propGreen = settings.propGreen;
        mainGui.propBlue = settings.propBlue;

        mainGui.SetFormat(settings.fileFormat);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void DoMyWindow(int windowID)
    {
        int offsetX = 10;
        int offsetY = 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Normal Map Style");

        offsetY += 30;

        settings.normalMapMaxStyle =
            GUI.Toggle(new Rect(offsetX, offsetY, 100, 30), settings.normalMapMaxStyle, " Max Style");
        if (settings.normalMapMaxStyle)
        {
            settings.normalMapMayaStyle = false;
        }
        else
        {
            settings.normalMapMayaStyle = true;
        }


        settings.normalMapMayaStyle = GUI.Toggle(new Rect(offsetX + 100, offsetY, 100, 30), settings.normalMapMayaStyle,
            " Maya Style");
        if (settings.normalMapMayaStyle)
        {
            settings.normalMapMaxStyle = false;
        }
        else
        {
            settings.normalMapMaxStyle = true;
        }

        offsetY += 30;

        settings.postProcessEnabled = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), settings.postProcessEnabled,
            " Enable Post Process By Default");

        offsetY += 30;

        if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default Property Map Channels"))
        {
            settings.propRed = mainGui.propRed;
            settings.propGreen = mainGui.propGreen;
            settings.propBlue = mainGui.propBlue;
        }

        offsetY += 30;

        if (GUI.Button(new Rect(offsetX, offsetY, 260, 25), "Set Default File Format"))
        {
            settings.fileFormat = mainGui.selectedFormat;
        }

        offsetY += 40;

        if (GUI.Button(new Rect(offsetX + 140, offsetY, 120, 30), "Save and Close"))
        {
            SaveSettings();
            SetNormalMode();
            windowOpen = false;
        }

        GUI.DragWindow();
    }

    void OnGUI()
    {
        windowRect = new Rect(Screen.width - 300, Screen.height - 320, 280, 230);

        if (windowOpen)
        {
            windowRect = GUI.Window(20, windowRect, DoMyWindow, "Setting and Preferences");
        }

        if (GUI.Button(new Rect(Screen.width - 280, Screen.height - 40, 80, 30), "Settings"))
        {
            if (windowOpen == true)
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