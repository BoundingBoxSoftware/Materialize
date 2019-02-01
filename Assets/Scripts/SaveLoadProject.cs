using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Enumerable = System.Linq.Enumerable;

public enum MapType
{
    Height,
    Diffuse,
    DiffuseOriginal,
    Metallic,
    Smoothness,
    Normal,
    Edge,
    Ao,
    Property
}

public enum FileFormat
{
    Png,
    Jpg,
    Tga,
    Exr
}

public class ProjectObject
{
    public string AoMapPath;

    public AOSettings AoSettings;
    public string DiffuseMapOriginalPath;
    public string DiffuseMapPath;
    public string EdgeMapPath;

    public EditDiffuseSettings EditDiffuseSettings;

    public EdgeSettings EdgeSettings;
    public string HeightMapPath;
    public HeightFromDiffuseSettings HeightFromDiffuseSettings;

    public MaterialSettings MaterialSettings;
    public string MetallicMapPath;

    public MetallicSettings MetallicSettings;

    public NormalFromHeightSettings NormalFromHeightSettings;
    public string NormalMapPath;
    public string SmoothnessMapPath;

    public SmoothnessSettings SmoothnessSettings;
}

public class SaveLoadProject : MonoBehaviour
{
    private MainGui _mainGui;
    private ProjectObject _thisProject;

    private char _pathChar;

    public bool Busy;

    // Use this for initialization
    private void Start()
    {
        _mainGui = FindObjectOfType<MainGui>();
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer)
            _pathChar = '\\';
        else
            _pathChar = '/';

        _thisProject = new ProjectObject();
    }

    public void LoadProject(string pathToFile)
    {
        Debug.Log("Loading Project: " + pathToFile);

        var serializer = new XmlSerializer(typeof(ProjectObject));
        var stream = new FileStream(pathToFile, FileMode.Open);
        _thisProject = serializer.Deserialize(stream) as ProjectObject;
        stream.Close();
        _mainGui.HeightFromDiffuseGuiScript.SetValues(_thisProject);
        _mainGui.EditDiffuseGuiScript.SetValues(_thisProject);
        _mainGui.NormalFromHeightGuiScript.SetValues(_thisProject);
        _mainGui.MetallicGuiScript.SetValues(_thisProject);
        _mainGui.SmoothnessGuiScript.SetValues(_thisProject);
        _mainGui.EdgeFromNormalGuiScript.SetValues(_thisProject);
        _mainGui.AoFromNormalGuiScript.SetValues(_thisProject);
        _mainGui.MaterialGuiScript.SetValues(_thisProject);

        _mainGui.ClearAllTextures();

        StartCoroutine(LoadAllTextures(pathToFile));
    }

    public void SaveProject(string pathToFile)
    {
        if (pathToFile.Contains("."))
            pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf(".", StringComparison.Ordinal));

        Debug.Log("Saving Project: " + pathToFile);

        var extension = _mainGui.SelectedFormat.ToString().ToLower();
        var projectName = pathToFile.Substring(pathToFile.LastIndexOf(_pathChar) + 1);
        Debug.Log("Project Name " + projectName);

        _mainGui.HeightFromDiffuseGuiScript.GetValues(_thisProject);
        if (_mainGui.HeightMap != null)
            _thisProject.HeightMapPath = projectName + "_height." + extension;
        else
            _thisProject.HeightMapPath = "null";

        _mainGui.EditDiffuseGuiScript.GetValues(_thisProject);
        if (_mainGui.DiffuseMap != null)
            _thisProject.DiffuseMapPath = projectName + "_diffuse." + extension;
        else
            _thisProject.DiffuseMapPath = "null";

        if (_mainGui.DiffuseMapOriginal != null)
            _thisProject.DiffuseMapOriginalPath = projectName + "_diffuseOriginal." + extension;
        else
            _thisProject.DiffuseMapOriginalPath = "null";

        _mainGui.NormalFromHeightGuiScript.GetValues(_thisProject);
        if (_mainGui.NormalMap != null)
            _thisProject.NormalMapPath = projectName + "_normal." + extension;
        else
            _thisProject.NormalMapPath = "null";

        _mainGui.MetallicGuiScript.GetValues(_thisProject);
        if (_mainGui.MetallicMap != null)
            _thisProject.MetallicMapPath = projectName + "_metallic." + extension;
        else
            _thisProject.MetallicMapPath = "null";

        _mainGui.SmoothnessGuiScript.GetValues(_thisProject);
        if (_mainGui.SmoothnessMap != null)
            _thisProject.SmoothnessMapPath = projectName + "_smoothness." + extension;
        else
            _thisProject.SmoothnessMapPath = "null";

        _mainGui.EdgeFromNormalGuiScript.GetValues(_thisProject);
        if (_mainGui.EdgeMap != null)
            _thisProject.EdgeMapPath = projectName + "_edge." + extension;
        else
            _thisProject.EdgeMapPath = "null";

        _mainGui.AoFromNormalGuiScript.GetValues(_thisProject);
        if (_mainGui.AoMap != null)
            _thisProject.AoMapPath = projectName + "_ao." + extension;
        else
            _thisProject.AoMapPath = "null";

        _mainGui.MaterialGuiScript.GetValues(_thisProject);

        var serializer = new XmlSerializer(typeof(ProjectObject));
        var stream = new FileStream(pathToFile + ".mtz", FileMode.Create);
        serializer.Serialize(stream, _thisProject);
        stream.Close();

        SaveAllFiles(pathToFile);
    }

    public void SaveAllFiles(string pathToFile)
    {
        StartCoroutine(SaveAllTextures(pathToFile));
    }

    public void SaveFile(string pathToFile, Texture2D textureToSave)
    {
        StartCoroutine(SaveTexture(textureToSave, pathToFile));
    }

    public void PasteFile(MapType mapTypeToLoad)
    {
        var clipBoard = ClipboardHelper.clipBoard;
        string pathToFile;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            const string filePrefix = "file:///";
            if (clipBoard.IsNullOrEmpty())
            {
                var pathToTextFile = Path.GetTempFileName();
                BashRunner.Run($"xclip -selection clipboard -t TARGETS -o > {pathToTextFile}");
                var bashOut = File.ReadAllText(pathToTextFile);
                Debug.Log($"Out : {bashOut}");
                File.Delete(pathToTextFile);

                if (bashOut.Contains("image/png"))
                {
                    pathToFile = Path.GetTempFileName() + ".png";
                    BashRunner.Run($"xclip -selection clipboard -t image/png -o > {pathToFile}");
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (!clipBoard.Contains(filePrefix)) return;
                var supported = Enumerable.Any(MainGui.LoadFormats, format => clipBoard.Contains(format));
                if (!supported) return;

                var firstIndex = clipBoard.IndexOf("file:///", StringComparison.Ordinal);
                var lastIndex = clipBoard.IndexOf("\n", firstIndex, StringComparison.Ordinal);
                var length = lastIndex - firstIndex;
                pathToFile = clipBoard.Substring(firstIndex, length);
                pathToFile = pathToFile.Replace("file:///", "/");
                Debug.Log(clipBoard);
            }


            Debug.Log(pathToFile);
            StartCoroutine(LoadTexture(mapTypeToLoad, pathToFile));
        }
    }

    public void CopyFile(Texture2D textureToSave)
    {
        SaveFile(Application.dataPath + "/temp.png", textureToSave);
        //SaveFile(Application.persistentDataPath + "/temp.png",FileFormat.png,textureToSave, "" );

        try
        {
            Process myProcess = new Process();
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = Application.streamingAssetsPath.Replace("/", "\\") + "\\i2c.exe";
            myProcess.StartInfo.Arguments = Application.temporaryCachePath.Replace("/", "\\") + "\\temp.png";
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.WaitForExit();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }
    }

    //==============================================//
    //			Texture Saving Coroutines			//
    //==============================================//


    private IEnumerator SaveAllTextures(string pathToFile)
    {
        var path = pathToFile.Substring(0, pathToFile.LastIndexOf(_pathChar) + 1);
        yield return StartCoroutine(SaveTexture(_mainGui.HeightMap, path + _thisProject.HeightMapPath));

        yield return StartCoroutine(SaveTexture(_mainGui.DiffuseMap, path + _thisProject.DiffuseMapPath));

        yield return StartCoroutine(SaveTexture(_mainGui.DiffuseMapOriginal,
            path + _thisProject.DiffuseMapOriginalPath));

        yield return StartCoroutine(SaveTexture(_mainGui.NormalMap, path + _thisProject.NormalMapPath));

        yield return StartCoroutine(SaveTexture(_mainGui.MetallicMap, path + _thisProject.MetallicMapPath));

        yield return StartCoroutine(SaveTexture(_mainGui.SmoothnessMap, path + _thisProject.SmoothnessMapPath));

        yield return StartCoroutine(SaveTexture(_mainGui.EdgeMap, path + _thisProject.EdgeMapPath));

        yield return StartCoroutine(SaveTexture(_mainGui.AoMap, path + _thisProject.AoMapPath));
    }

    public IEnumerator SaveTexture(string extension, Texture2D textureToSave, string pathToFile)
    {
        yield return StartCoroutine(SaveTexture(textureToSave, pathToFile + "." + extension));
    }

    public IEnumerator SaveTexture(Texture2D textureToSave, string pathToFile)
    {
        if (!textureToSave || pathToFile.IsNullOrEmpty()) yield break;
        Debug.Log($"Salvando {textureToSave} como {pathToFile}");
        if (!pathToFile.Contains(".")) pathToFile = $"{pathToFile}.{_mainGui.SelectedFormat}";

        var fileIndex = pathToFile.LastIndexOf('.');
        var extension = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);

        switch (extension)
        {
            case "png":
                var pngBytes = textureToSave.EncodeToPNG();
                File.WriteAllBytes(pathToFile, pngBytes);
                break;
            case "jpg":
                var jpgBytes = textureToSave.EncodeToJPG();
                File.WriteAllBytes(pathToFile, jpgBytes);
                break;
            case "tga":
                var tgaBytes = textureToSave.EncodeToTGA();
                File.WriteAllBytes(pathToFile, tgaBytes);
                break;
            case "exr":
                var exrBytes = textureToSave.EncodeToEXR();
                File.WriteAllBytes(pathToFile, exrBytes);
                break;
        }

        Resources.UnloadUnusedAssets();


        yield return new WaitForSeconds(0.1f);
    }

    //==============================================//
    //			Texture Loading Coroutines			//
    //==============================================//

    public IEnumerator LoadAllTextures(string pathToFile)
    {
        pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf(_pathChar) + 1);

        if (_thisProject.HeightMapPath != "null")
            StartCoroutine(LoadTexture(MapType.Height, pathToFile + _thisProject.HeightMapPath));

        while (Busy) yield return new WaitForSeconds(0.01f);

        if (_thisProject.DiffuseMapOriginalPath != "null")
            StartCoroutine(LoadTexture(MapType.DiffuseOriginal, pathToFile + _thisProject.DiffuseMapOriginalPath));

        while (Busy) yield return new WaitForSeconds(0.01f);

        if (_thisProject.DiffuseMapPath != "null")
            StartCoroutine(LoadTexture(MapType.Diffuse, pathToFile + _thisProject.DiffuseMapPath));

        while (Busy) yield return new WaitForSeconds(0.01f);

        if (_thisProject.NormalMapPath != "null")
            StartCoroutine(LoadTexture(MapType.Normal, pathToFile + _thisProject.NormalMapPath));

        while (Busy) yield return new WaitForSeconds(0.01f);

        if (_thisProject.MetallicMapPath != "null")
            StartCoroutine(LoadTexture(MapType.Metallic, pathToFile + _thisProject.MetallicMapPath));

        while (Busy) yield return new WaitForSeconds(0.01f);

        if (_thisProject.SmoothnessMapPath != "null")
            StartCoroutine(LoadTexture(MapType.Smoothness, pathToFile + _thisProject.SmoothnessMapPath));

        while (Busy) yield return new WaitForSeconds(0.01f);

        if (_thisProject.EdgeMapPath != "null")
            StartCoroutine(LoadTexture(MapType.Edge, pathToFile + _thisProject.EdgeMapPath));

        while (Busy) yield return new WaitForSeconds(0.01f);

        if (_thisProject.AoMapPath != "null")
            StartCoroutine(LoadTexture(MapType.Ao, pathToFile + _thisProject.AoMapPath));

        while (Busy) yield return new WaitForSeconds(0.01f);

        yield return new WaitForSeconds(0.01f);
    }

    public IEnumerator LoadTexture(MapType textureToLoad, string pathToFile)
    {
        Busy = true;

        Texture2D newTexture = null;

        if (File.Exists(pathToFile))
        {
            var fileData = File.ReadAllBytes(pathToFile);
            newTexture = new Texture2D(2, 2);
            newTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        if (!newTexture) yield break;
        newTexture.anisoLevel = 9;


        switch (textureToLoad)
        {
            case MapType.Height:
                _mainGui.HeightMap = newTexture;
                break;
            case MapType.Diffuse:
                _mainGui.DiffuseMap = newTexture;
                break;
            case MapType.DiffuseOriginal:
                _mainGui.DiffuseMapOriginal = newTexture;
                break;
            case MapType.Normal:
                _mainGui.NormalMap = newTexture;
                break;
            case MapType.Metallic:
                _mainGui.MetallicMap = newTexture;
                break;
            case MapType.Smoothness:
                _mainGui.SmoothnessMap = newTexture;
                break;
            case MapType.Edge:
                _mainGui.EdgeMap = newTexture;
                break;
            case MapType.Ao:
                _mainGui.AoMap = newTexture;
                break;
        }

        _mainGui.SetLoadedTexture(textureToLoad);

        Resources.UnloadUnusedAssets();


        yield return new WaitForSeconds(0.01f);

        Busy = false;
    }
}