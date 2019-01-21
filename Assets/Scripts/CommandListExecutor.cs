using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public struct CommandSettings
{
    public bool normalMapMaxStyle;
    public bool normalMapMayaStyle;

    public bool postProcessEnabled;

    public PropChannelMap propRed;
    public PropChannelMap propGreen;
    public PropChannelMap propBlue;
}

public struct CommandOpen
{
    public string filePath;
    public MapType mapType;
}

public struct CommandSave
{
    public string filePath;
    public MapType mapType;
}

public struct CommandAOFromNormal
{
    public string settings;
}

public struct CommandEdgeFromNormal
{
    public string settings;
}

public struct CommandFlipNormalMapY
{
    //public bool flipNormalMapY;
}

public struct CommandFileFormat
{
    public FileFormat fileFormat;
}

public enum CommandType
{
    Settings,
    Open,
    Save,
    HeightFromDiffuse,
    NormalFromHeight,
    Metallic,
    Smoothness,
    AOFromNormal,
    EdgeFromNormal,
    QuickSave,
    FlipNormalMapY,
    FileFormat,
    Wait
}

public struct Command
{
    //public string xmlCommand;
    public CommandType commandType;
    public string extension;
    public string filePath;
    public MapType mapType;
    public string settings;

    public Settings projectSettings;
}

public class CommandList
{
    public List<Command> commands;
}

public class CommandListExecutor : MonoBehaviour
{
    private AOFromNormalGui aoFromNormalGui;

    public GameObject aoFromNormalGuiObject;
    private EdgeFromNormalGui edgeFromNormalGui;

    public GameObject edgeFromNormalGuiObject;
    private HeightFromDiffuseGui heightFromDiffuseGui;

    public GameObject heightFromDiffuseGuiObject;
    private MainGui mainGui;

    public GameObject mainGuiObject;
    private MaterialGui materialGui;

    public GameObject materialGuiObject;
    private MetallicGui metallicGui;

    public GameObject metallicGuiObject;
    private NormalFromHeightGui normalFromHeightGui;

    public GameObject normalFromHeightGuiObject;
    private SaveLoadProject saveLoad;

    public GameObject saveLoadProjectObject;

    public SettingsGui settingsGui;
    private SmoothnessGui smoothnessGui;

    public GameObject smoothnessGuiObject;

    // Use this for initialization
    private void Start()
    {
        //string[] arguments = Environment.GetCommandLineArgs(); 
        mainGui = mainGuiObject.GetComponent<MainGui>();
        saveLoad = saveLoadProjectObject.GetComponent<SaveLoadProject>();

        heightFromDiffuseGui = heightFromDiffuseGuiObject.GetComponent<HeightFromDiffuseGui>();
        normalFromHeightGui = normalFromHeightGuiObject.GetComponent<NormalFromHeightGui>();
        metallicGui = metallicGuiObject.GetComponent<MetallicGui>();
        smoothnessGui = smoothnessGuiObject.GetComponent<SmoothnessGui>();
        aoFromNormalGui = aoFromNormalGuiObject.GetComponent<AOFromNormalGui>();
        edgeFromNormalGui = edgeFromNormalGuiObject.GetComponent<EdgeFromNormalGui>();

        materialGui = materialGuiObject.GetComponent<MaterialGui>();

        StartCoroutine(StartCommandString());
    }

    /*
    int wait = 20;
    void Update(){
        wait -= 1;
        if (wait == 0) {
            SaveTestString ();
        }
    }
    */

    private IEnumerator StartCommandString()
    {
        yield return new WaitForSeconds(0.1f);
        var commandString = ClipboardHelper.clipBoard;
        if (commandString.Contains(
            "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">")
        ) ProcessCommands(commandString);
    }

    public void SaveTestString()
    {
        var commandList = new CommandList();
        commandList.commands = new List<Command>();

        var command = new Command();
        command.commandType = CommandType.Settings;
        command.projectSettings = settingsGui.settings;
        commandList.commands.Add(command);

        command = new Command();
        command.commandType = CommandType.Open;
        command.filePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_diffuse.bmp";
        command.mapType = MapType.DiffuseOriginal;
        commandList.commands.Add(command);

        command = new Command();
        command.commandType = CommandType.Open;
        command.filePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_normal.bmp";
        command.mapType = MapType.Normal;
        commandList.commands.Add(command);

        command = new Command();
        command.commandType = CommandType.FlipNormalMapY;
        commandList.commands.Add(command);

        command = new Command();
        command.commandType = CommandType.AOFromNormal;
        commandList.commands.Add(command);

        command = new Command();
        command.commandType = CommandType.EdgeFromNormal;
        commandList.commands.Add(command);

        command = new Command();
        command.commandType = CommandType.FileFormat;
        command.extension = "tga";
        commandList.commands.Add(command);

        command = new Command();
        command.commandType = CommandType.QuickSave;
        command.filePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_property.bmp";
        commandList.commands.Add(command);


        var sb = new StringBuilder();
        var serializer = new XmlSerializer(typeof(CommandList));
        var stream = new StringWriter(sb);
        serializer.Serialize(stream, commandList);
        ClipboardHelper.clipBoard = stream.ToString();

        Debug.Log(stream.ToString());
    }

    private void OnApplicationFocus(bool focusStatus)
    {
        if (focusStatus)
        {
            var commandString = ClipboardHelper.clipBoard;
            if (commandString.Contains(
                "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">")
            ) ProcessCommands(commandString);
        }
    }

    public void ProcessCommands()
    {
        var commandString = ClipboardHelper.clipBoard;
        if (commandString.Contains(
            "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">")
        ) StartCoroutine(ProcessCommandsCoroutine(commandString));
    }

    public void ProcessCommands(string commandString)
    {
        StartCoroutine(ProcessCommandsCoroutine(commandString));
    }

    private IEnumerator ProcessCommandsCoroutine(string commandString)
    {
        //string commandString = ClipboardHelper.clipBoard;

        var serializer = new XmlSerializer(typeof(CommandList));
        var stream = new StringReader(commandString);
        var commandList = serializer.Deserialize(stream) as CommandList;

        for (var i = 0; i < commandList.commands.Count; i++)
        {
            var thisCommand = commandList.commands[i];
            if (thisCommand.commandType == CommandType.Settings)
            {
                settingsGui.settings = thisCommand.projectSettings;
                settingsGui.SetSettings();
            }
            else if (thisCommand.commandType == CommandType.Open)
            {
                StartCoroutine(saveLoad.LoadTexture(thisCommand.mapType, thisCommand.filePath));
                while (saveLoad.busy) yield return new WaitForSeconds(0.1f);
            }
            else if (thisCommand.commandType == CommandType.Save)
            {
                switch (thisCommand.mapType)
                {
                    case MapType.Height:
                        StartCoroutine(saveLoad.SaveTexture(thisCommand.extension, mainGui.HeightMap,
                            thisCommand.filePath));
                        break;
                    case MapType.Diffuse:
                        StartCoroutine(saveLoad.SaveTexture(thisCommand.extension, mainGui.DiffuseMapOriginal,
                            thisCommand.filePath));
                        break;
                    case MapType.Metallic:
                        StartCoroutine(saveLoad.SaveTexture(thisCommand.extension, mainGui.MetallicMap,
                            thisCommand.filePath));
                        break;
                    case MapType.Smoothness:
                        StartCoroutine(saveLoad.SaveTexture(thisCommand.extension, mainGui.SmoothnessMap,
                            thisCommand.filePath));
                        break;
                    case MapType.Edge:
                        StartCoroutine(saveLoad.SaveTexture(thisCommand.extension, mainGui.EdgeMap,
                            thisCommand.filePath));
                        break;
                    case MapType.AO:
                        StartCoroutine(
                            saveLoad.SaveTexture(thisCommand.extension, mainGui.AoMap, thisCommand.filePath));
                        break;
                    case MapType.Property:
                        mainGui.ProcessPropertyMap();
                        StartCoroutine(saveLoad.SaveTexture(thisCommand.extension, mainGui.PropertyMap,
                            thisCommand.filePath));
                        break;
                }

                while (saveLoad.busy) yield return new WaitForSeconds(0.1f);
            }
            else if (thisCommand.commandType == CommandType.FlipNormalMapY)
            {
                mainGui.FlipNormalMapY();
            }
            else if (thisCommand.commandType == CommandType.FileFormat)
            {
                mainGui.SetFormat(thisCommand.extension);
            }
            else if (thisCommand.commandType == CommandType.HeightFromDiffuse)
            {
                mainGui.CloseWindows();
                heightFromDiffuseGuiObject.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                heightFromDiffuseGui.InitializeTextures();
                yield return new WaitForSeconds(0.1f);
                StartCoroutine(heightFromDiffuseGui.ProcessDiffuse());
                while (heightFromDiffuseGui.busy) yield return new WaitForSeconds(0.1f);
                StartCoroutine(heightFromDiffuseGui.ProcessHeight());
                while (heightFromDiffuseGui.busy) yield return new WaitForSeconds(0.1f);
                heightFromDiffuseGui.Close();
            }
            else if (thisCommand.commandType == CommandType.NormalFromHeight)
            {
                mainGui.CloseWindows();
                normalFromHeightGuiObject.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                normalFromHeightGui.InitializeTextures();
                yield return new WaitForSeconds(0.1f);
                StartCoroutine(normalFromHeightGui.ProcessHeight());
                while (normalFromHeightGui.busy) yield return new WaitForSeconds(0.1f);
                StartCoroutine(normalFromHeightGui.ProcessNormal());
                while (normalFromHeightGui.busy) yield return new WaitForSeconds(0.1f);
                normalFromHeightGui.Close();
            }
            else if (thisCommand.commandType == CommandType.Metallic)
            {
                mainGui.CloseWindows();
                metallicGuiObject.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                metallicGui.InitializeTextures();
                yield return new WaitForSeconds(0.1f);
                StartCoroutine(metallicGui.ProcessBlur());
                while (metallicGui.busy) yield return new WaitForSeconds(0.1f);
                StartCoroutine(metallicGui.ProcessMetallic());
                while (metallicGui.busy) yield return new WaitForSeconds(0.1f);
                metallicGui.Close();
            }
            else if (thisCommand.commandType == CommandType.Smoothness)
            {
                mainGui.CloseWindows();
                smoothnessGuiObject.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                smoothnessGui.InitializeTextures();
                yield return new WaitForSeconds(0.1f);
                StartCoroutine(smoothnessGui.ProcessBlur());
                while (smoothnessGui.busy) yield return new WaitForSeconds(0.1f);
                StartCoroutine(smoothnessGui.ProcessSmoothness());
                while (smoothnessGui.busy) yield return new WaitForSeconds(0.1f);
                smoothnessGui.Close();
            }
            else if (thisCommand.commandType == CommandType.AOFromNormal)
            {
                mainGui.CloseWindows();
                aoFromNormalGuiObject.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                aoFromNormalGui.InitializeTextures();
                yield return new WaitForSeconds(0.1f);
                StartCoroutine(aoFromNormalGui.ProcessNormalDepth());
                while (aoFromNormalGui.busy) yield return new WaitForSeconds(0.1f);
                StartCoroutine(aoFromNormalGui.ProcessAO());
                while (aoFromNormalGui.busy) yield return new WaitForSeconds(0.1f);
                aoFromNormalGui.Close();
            }
            else if (thisCommand.commandType == CommandType.EdgeFromNormal)
            {
                mainGui.CloseWindows();
                edgeFromNormalGuiObject.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                edgeFromNormalGui.InitializeTextures();
                yield return new WaitForSeconds(0.1f);
                StartCoroutine(edgeFromNormalGui.ProcessNormal());
                while (edgeFromNormalGui.busy) yield return new WaitForSeconds(0.1f);
                StartCoroutine(edgeFromNormalGui.ProcessEdge());
                while (edgeFromNormalGui.busy) yield return new WaitForSeconds(0.1f);
                edgeFromNormalGui.Close();
            }
            else if (thisCommand.commandType == CommandType.QuickSave)
            {
                switch (thisCommand.mapType)
                {
                    case MapType.Height:
                        mainGui.QuicksavePathHeight = thisCommand.filePath;
                        break;
                    case MapType.Diffuse:
                        mainGui.QuicksavePathDiffuse = thisCommand.filePath;
                        break;
                    case MapType.Normal:
                        mainGui.QuicksavePathNormal = thisCommand.filePath;
                        break;
                    case MapType.Metallic:
                        mainGui.QuicksavePathMetallic = thisCommand.filePath;
                        break;
                    case MapType.Smoothness:
                        mainGui.QuicksavePathSmoothness = thisCommand.filePath;
                        break;
                    case MapType.Edge:
                        mainGui.QuicksavePathEdge = thisCommand.filePath;
                        break;
                    case MapType.AO:
                        mainGui.QuicksavePathAo = thisCommand.filePath;
                        break;
                    case MapType.Property:
                        mainGui.QuicksavePathProperty = thisCommand.filePath;
                        break;
                }
            }

            yield return new WaitForSeconds(0.1f);

            ClipboardHelper.clipBoard = "";
        }

        yield return new WaitForSeconds(0.1f);

        mainGui.CloseWindows();
        mainGui.FixSize();
        materialGuiObject.SetActive(true);
        materialGui.Initialize();
    }
}