#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

#endregion

public enum CommandType
{
    Settings,
    Open,
    Save,
    HeightFromDiffuse,
    NormalFromHeight,
    Metallic,
    Smoothness,
    AoFromNormal,
    EdgeFromNormal,
    QuickSave,
    FlipNormalMapY,
    FileFormat
}

public struct Command
{
    //public string xmlCommand;
    public CommandType CommandType;
    public string Extension;
    public string FilePath;
    public MapType MapType;

    public Settings ProjectSettings;
}

public class CommandList
{
    public List<Command> Commands;
}

public class CommandListExecutor : MonoBehaviour
{
    private MainGui _mainGui;
    private SaveLoadProject _saveLoad;

    public GameObject SaveLoadProjectObject;

    public SettingsGui SettingsGui;

    // Use this for initialization
    private void Start()
    {
        _mainGui = MainGui.Instance.GetComponent<MainGui>();
        _saveLoad = SaveLoadProjectObject.GetComponent<SaveLoadProject>();

        StartCoroutine(StartCommandString());
    }

    private IEnumerator StartCommandString()
    {
        yield return new WaitForSeconds(0.1f);
        var commandString = ClipboardHelper.ClipBoard;
        if (commandString.Contains(
            "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">")
        ) ProcessCommands(commandString);
    }

    public void SaveTestString()
    {
        var commandList = new CommandList {Commands = new List<Command>()};

        var command = new Command {CommandType = CommandType.Settings, ProjectSettings = SettingsGui.Settings};
        commandList.Commands.Add(command);

        command = new Command
        {
            CommandType = CommandType.Open,
            FilePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_diffuse.bmp",
            MapType = MapType.DiffuseOriginal
        };
        commandList.Commands.Add(command);

        command = new Command
        {
            CommandType = CommandType.Open,
            FilePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_normal.bmp",
            MapType = MapType.Normal
        };
        commandList.Commands.Add(command);

        command = new Command {CommandType = CommandType.FlipNormalMapY};
        commandList.Commands.Add(command);

        command = new Command {CommandType = CommandType.AoFromNormal};
        commandList.Commands.Add(command);

        command = new Command {CommandType = CommandType.EdgeFromNormal};
        commandList.Commands.Add(command);

        command = new Command {CommandType = CommandType.FileFormat, Extension = "tga"};
        commandList.Commands.Add(command);

        command = new Command
        {
            CommandType = CommandType.QuickSave,
            FilePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_property.bmp"
        };
        commandList.Commands.Add(command);


        var sb = new StringBuilder();
        var serializer = new XmlSerializer(typeof(CommandList));
        var stream = new StringWriter(sb);
        serializer.Serialize(stream, commandList);
        ClipboardHelper.ClipBoard = stream.ToString();

        Debug.Log(stream.ToString());
    }

    private void OnApplicationFocus(bool focusStatus)
    {
        if (!focusStatus) return;
        var commandString = ClipboardHelper.ClipBoard;
        if (commandString.Contains(
            "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">")
        ) ProcessCommands(commandString);
    }

    public void ProcessCommands()
    {
        var commandString = ClipboardHelper.ClipBoard;
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

        if (serializer.Deserialize(stream) is CommandList commandList)
            foreach (var thisCommand in commandList.Commands)
            {
                switch (thisCommand.CommandType)
                {
                    case CommandType.Settings:
                        SettingsGui.Settings = thisCommand.ProjectSettings;
                        SettingsGui.SetSettings();
                        break;
                    case CommandType.Open:
                    {
                        StartCoroutine(_saveLoad.LoadTexture(thisCommand.MapType, thisCommand.FilePath));
                        while (_saveLoad.Busy) yield return new WaitForSeconds(0.1f);
                        break;
                    }
                    case CommandType.Save:
                    {
                        switch (thisCommand.MapType)
                        {
                            case MapType.Height:
                                _saveLoad.SaveTexture(thisCommand.Extension, _mainGui.HeightMap,
                                    thisCommand.FilePath);
                                break;
                            case MapType.Diffuse:
                                _saveLoad.SaveTexture(thisCommand.Extension, _mainGui.DiffuseMapOriginal,
                                    thisCommand.FilePath);
                                break;
                            case MapType.Metallic:
                                _saveLoad.SaveTexture(thisCommand.Extension, _mainGui.MetallicMap,
                                    thisCommand.FilePath);
                                break;
                            case MapType.Smoothness:
                                _saveLoad.SaveTexture(thisCommand.Extension, _mainGui.SmoothnessMap,
                                    thisCommand.FilePath);
                                break;
                            case MapType.Edge:
                                _saveLoad.SaveTexture(thisCommand.Extension, _mainGui.EdgeMap,
                                    thisCommand.FilePath);
                                break;
                            case MapType.Ao:
                                    _saveLoad.SaveTexture(thisCommand.Extension, _mainGui.AoMap, thisCommand.FilePath);
                                break;
                            case MapType.Property:
                                _mainGui.ProcessPropertyMap();
                                _saveLoad.SaveTexture(thisCommand.Extension, _mainGui.PropertyMap,
                                    thisCommand.FilePath);
                                break;
                            case MapType.DiffuseOriginal:
                                break;
                            case MapType.Normal:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        while (_saveLoad.Busy) yield return new WaitForSeconds(0.1f);
                        break;
                    }
                    case CommandType.FlipNormalMapY:
                        _mainGui.FlipNormalMapY();
                        break;
                    case CommandType.FileFormat:
                        _mainGui.SetFormat(thisCommand.Extension);
                        break;
                    case CommandType.HeightFromDiffuse:
                    {
                        _mainGui.CloseWindows();
                        _mainGui.HeightFromDiffuseGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        _mainGui.HeightFromDiffuseGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.HeightFromDiffuseGuiScript.ProcessDiffuse());
                        while (_mainGui.HeightFromDiffuseGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.HeightFromDiffuseGuiScript.ProcessHeight());
                        while (_mainGui.HeightFromDiffuseGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        _mainGui.HeightFromDiffuseGuiScript.Close();
                        break;
                    }
                    case CommandType.NormalFromHeight:
                    {
                        _mainGui.CloseWindows();
                        _mainGui.NormalFromHeightGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        _mainGui.NormalFromHeightGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.NormalFromHeightGuiScript.ProcessHeight());
                        while (_mainGui.NormalFromHeightGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.NormalFromHeightGuiScript.ProcessNormal());
                        while (_mainGui.NormalFromHeightGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        _mainGui.NormalFromHeightGuiScript.Close();
                        break;
                    }
                    case CommandType.Metallic:
                    {
                        _mainGui.CloseWindows();
                        _mainGui.MetallicGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        _mainGui.MetallicGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.MetallicGuiScript.ProcessBlur());
                        while (_mainGui.MetallicGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.MetallicGuiScript.ProcessMetallic());
                        while (_mainGui.MetallicGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        _mainGui.MetallicGuiScript.Close();
                        break;
                    }
                    case CommandType.Smoothness:
                    {
                        _mainGui.CloseWindows();
                        _mainGui.SmoothnessGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        _mainGui.SmoothnessGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.SmoothnessGuiScript.ProcessBlur());
                        while (_mainGui.SmoothnessGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.SmoothnessGuiScript.ProcessSmoothness());
                        while (_mainGui.SmoothnessGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        _mainGui.SmoothnessGuiScript.Close();
                        break;
                    }
                    case CommandType.AoFromNormal:
                    {
                        _mainGui.CloseWindows();
                        _mainGui.AoFromNormalGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        _mainGui.AoFromNormalGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.AoFromNormalGuiScript.ProcessNormalDepth());
                        while (_mainGui.AoFromNormalGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.AoFromNormalGuiScript.ProcessAo());
                        while (_mainGui.AoFromNormalGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        _mainGui.AoFromNormalGuiScript.Close();
                        break;
                    }
                    case CommandType.EdgeFromNormal:
                    {
                        _mainGui.CloseWindows();
                        _mainGui.EdgeFromNormalGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        _mainGui.EdgeFromNormalGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.EdgeFromNormalGuiScript.ProcessNormal());
                        while (_mainGui.EdgeFromNormalGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        StartCoroutine(_mainGui.EdgeFromNormalGuiScript.ProcessEdge());
                        while (_mainGui.EdgeFromNormalGuiScript.Busy) yield return new WaitForSeconds(0.1f);
                        _mainGui.EdgeFromNormalGuiScript.Close();
                        break;
                    }
                    case CommandType.QuickSave:
                        switch (thisCommand.MapType)
                        {
                            case MapType.Height:
                                _mainGui.QuicksavePathHeight = thisCommand.FilePath;
                                break;
                            case MapType.Diffuse:
                                _mainGui.QuicksavePathDiffuse = thisCommand.FilePath;
                                break;
                            case MapType.Normal:
                                _mainGui.QuicksavePathNormal = thisCommand.FilePath;
                                break;
                            case MapType.Metallic:
                                _mainGui.QuicksavePathMetallic = thisCommand.FilePath;
                                break;
                            case MapType.Smoothness:
                                _mainGui.QuicksavePathSmoothness = thisCommand.FilePath;
                                break;
                            case MapType.Edge:
                                _mainGui.QuicksavePathEdge = thisCommand.FilePath;
                                break;
                            case MapType.Ao:
                                _mainGui.QuicksavePathAo = thisCommand.FilePath;
                                break;
                            case MapType.Property:
                                _mainGui.QuicksavePathProperty = thisCommand.FilePath;
                                break;
                            case MapType.DiffuseOriginal:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return new WaitForSeconds(0.1f);

                ClipboardHelper.ClipBoard = "";
            }

        yield return new WaitForSeconds(0.1f);

        _mainGui.CloseWindows();
        _mainGui.FixSize();
        _mainGui.MaterialGuiObject.SetActive(true);
        _mainGui.MaterialGuiScript.Initialize();
    }
}