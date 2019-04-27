#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

#endregion

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

    public AoSettings AoSettings;
    public string DiffuseMapOriginalPath;
    public string DiffuseMapPath;
    public string EdgeMapPath;

    public EdgeSettings EdgeSettings;

    public EditDiffuseSettings EditDiffuseSettings;
    public HeightFromDiffuseSettings HeightFromDiffuseSettings;
    public string HeightMapPath;

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
    private char _pathChar;
    private ProjectObject _thisProject;

    [HideInInspector] public bool Busy;

    // Use this for initialization
    private void Start()
    {
       
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
        MainGui.Instance.HeightFromDiffuseGuiScript.SetValues(_thisProject);
        MainGui.Instance.EditDiffuseGuiScript.SetValues(_thisProject);
        MainGui.Instance.NormalFromHeightGuiScript.SetValues(_thisProject);
        MainGui.Instance.MetallicGuiScript.SetValues(_thisProject);
        MainGui.Instance.SmoothnessGuiScript.SetValues(_thisProject);
        MainGui.Instance.EdgeFromNormalGuiScript.SetValues(_thisProject);
        MainGui.Instance.AoFromNormalGuiScript.SetValues(_thisProject);
        MainGui.Instance.MaterialGuiScript.SetValues(_thisProject);

        MainGui.Instance.ClearAllTextures();

        StartCoroutine(LoadAllTextures(pathToFile));
    }

    public void SaveProject(string pathToFile)
    {
        if (pathToFile.Contains("."))
            pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf(".", StringComparison.Ordinal));

        List<string> extensions = new List<string>();
        Debug.Log("Saving Project: " + pathToFile);

        var extension = MainGui.Instance.SelectedFormat.ToString().ToLower();
        var projectName = pathToFile.Substring(pathToFile.LastIndexOf(_pathChar) + 1);
        Debug.Log("Project Name " + projectName);

        Debug.Log("test");
        MainGui.Instance.HeightFromDiffuseGuiScript.GetValues(_thisProject);
        if (MainGui.Instance.HeightMap != null)
            _thisProject.HeightMapPath = projectName + "_height." + extension;
        else
            _thisProject.HeightMapPath = "null";

        MainGui.Instance.EditDiffuseGuiScript.GetValues(_thisProject);
        if (MainGui.Instance.DiffuseMap != null)
            _thisProject.DiffuseMapPath = projectName + "_diffuse." + extension;
        else
            _thisProject.DiffuseMapPath = "null";

        if (MainGui.Instance.DiffuseMapOriginal != null)
            _thisProject.DiffuseMapOriginalPath = projectName + "_diffuseOriginal." + extension;
        else
            _thisProject.DiffuseMapOriginalPath = "null";

        MainGui.Instance.NormalFromHeightGuiScript.GetValues(_thisProject);
        if (MainGui.Instance.NormalMap != null)
            _thisProject.NormalMapPath = projectName + "_normal." + extension;
        else
            _thisProject.NormalMapPath = "null";

        MainGui.Instance.MetallicGuiScript.GetValues(_thisProject);
        if (MainGui.Instance.MetallicMap != null)
            _thisProject.MetallicMapPath = projectName + "_metallic." + extension;
        else
            _thisProject.MetallicMapPath = "null";

        MainGui.Instance.SmoothnessGuiScript.GetValues(_thisProject);
        if (MainGui.Instance.SmoothnessMap != null)
            _thisProject.SmoothnessMapPath = projectName + "_smoothness." + extension;
        else
            _thisProject.SmoothnessMapPath = "null";

        MainGui.Instance.EdgeFromNormalGuiScript.GetValues(_thisProject);
        if (MainGui.Instance.EdgeMap != null)
            _thisProject.EdgeMapPath = projectName + "_edge." + extension;
        else
            _thisProject.EdgeMapPath = "null";

        MainGui.Instance.AoFromNormalGuiScript.GetValues(_thisProject);
        if (MainGui.Instance.AoMap != null)
            _thisProject.AoMapPath = projectName + "_ao." + extension;
        else
            _thisProject.AoMapPath = "null";

        MainGui.Instance.MaterialGuiScript.GetValues(_thisProject);

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
        SaveTexture(textureToSave, pathToFile);
    }

    public void PasteFile(MapType mapTypeToLoad)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;
        const string filePrefix = "file:///";
        string pathToFile;

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
            BashRunner.Run($"xclip -selection clipboard -o > {pathToTextFile}");
            bashOut = File.ReadAllText(pathToTextFile);

            if (!bashOut.Contains(filePrefix)) return;
            var supported = MainGui.LoadFormats.Any(format => bashOut.Contains(format));
            if (!supported) return;

            var firstIndex = bashOut.IndexOf("file:///", StringComparison.Ordinal);
            var lastIndex = bashOut.IndexOf("\n", firstIndex, StringComparison.Ordinal);
            var length = lastIndex - firstIndex;
            pathToFile = bashOut.Substring(firstIndex, length);
            pathToFile = pathToFile.Replace("file:///", "/");
            Debug.Log("Path " + pathToFile);
        }

        File.Delete(pathToTextFile);


        StartCoroutine(LoadTexture(mapTypeToLoad, pathToFile));
    }

    public void CopyFile(Texture2D textureToSave)
    {
    }

    //==============================================//
    //			Texture Saving Coroutines			//
    //==============================================//


    private IEnumerator SaveAllTextures(string pathToFile)
    {
        var path = pathToFile.Substring(0, pathToFile.LastIndexOf(_pathChar) + 1);
        //yield return StartCoroutine(SaveTexture(MainGui.Instance.HeightMap, path + _thisProject.HeightMapPath));

        //yield return StartCoroutine(SaveTexture(MainGui.Instance.DiffuseMap, path + _thisProject.DiffuseMapPath));

        //yield return StartCoroutine(SaveTexture(MainGui.Instance.DiffuseMapOriginal,
        //    path + _thisProject.DiffuseMapOriginalPath));

        //yield return StartCoroutine(SaveTexture(MainGui.Instance.NormalMap, path + _thisProject.NormalMapPath));

        //yield return StartCoroutine(SaveTexture(MainGui.Instance.MetallicMap, path + _thisProject.MetallicMapPath));

        //yield return StartCoroutine(SaveTexture(MainGui.Instance.SmoothnessMap, path + _thisProject.SmoothnessMapPath));

        //yield return StartCoroutine(SaveTexture(MainGui.Instance.EdgeMap, path + _thisProject.EdgeMapPath));

        //yield return StartCoroutine(SaveTexture(MainGui.Instance.AoMap, path + _thisProject.AoMapPath));

        yield return SaveTexture(MainGui.Instance.HeightMap, path + _thisProject.HeightMapPath);

        yield return SaveTexture(MainGui.Instance.DiffuseMap, path + _thisProject.DiffuseMapPath);

        yield return SaveTexture(MainGui.Instance.DiffuseMapOriginal,
            path + _thisProject.DiffuseMapOriginalPath);

        yield return SaveTexture(MainGui.Instance.NormalMap, path + _thisProject.NormalMapPath);

        yield return SaveTexture(MainGui.Instance.MetallicMap, path + _thisProject.MetallicMapPath);

        yield return SaveTexture(MainGui.Instance.SmoothnessMap, path + _thisProject.SmoothnessMapPath);

        yield return SaveTexture(MainGui.Instance.EdgeMap, path + _thisProject.EdgeMapPath);

        yield return SaveTexture(MainGui.Instance.AoMap, path + _thisProject.AoMapPath);

        MainGui.Instance.Modle.SetActive(true);
    }

    public async Task SaveTexture(string extension, Texture2D textureToSave, string pathToFile)
    {
        await (SaveTexture(textureToSave, pathToFile + "." + extension));
    }

    private async Task SaveTexture(Texture2D textureToSave, string pathToFile)
    {
        if (!textureToSave || pathToFile.IsNullOrEmpty()) return;
        if (MainGui.Instance.ScaleTexture)
        {
            //TextureScale.BilinearScale(_textureToSave);
            textureToSave = TextureScale.Bilinear(textureToSave, int.Parse(MainGui.Instance.XSize), int.Parse(MainGui.Instance.YSize));
        }

        Debug.Log($"Saved {textureToSave} To {pathToFile}");
        if (!pathToFile.Contains(".")) pathToFile = $"{pathToFile}.{MainGui.Instance.SelectedFormat}";

        var fileIndex = pathToFile.LastIndexOf('.');
        var extension = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);

        switch (extension)
        {
            case "png":
                {
                    var pngBytes = textureToSave.EncodeToPNG();
                    File.WriteAllBytes(pathToFile, pngBytes);
                    break;
                }
            case "jpg":
                {
                    var jpgBytes = textureToSave.EncodeToJPG();
                    File.WriteAllBytes(pathToFile, jpgBytes);
                    break;
                }
            case "tga":
                {
                    var tgaBytes = textureToSave.EncodeToTGA();
                    File.WriteAllBytes(pathToFile, tgaBytes);
                    break;
                }
            case "exr":
                {
                    var exrBytes = textureToSave.EncodeToEXR();
                    File.WriteAllBytes(pathToFile, exrBytes);
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
        }

        Resources.UnloadUnusedAssets();


        //yield return new WaitForSeconds(0.1f);
    }

    //==============================================//
    //			Texture Loading Coroutines			//
    //==============================================//

    private IEnumerator LoadAllTextures(string pathToFile)
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
        pathToFile = Uri.UnescapeDataString(pathToFile);

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
                MainGui.Instance.HeightMap = newTexture;
                break;
            case MapType.Diffuse:
                MainGui.Instance.DiffuseMap = newTexture;
                break;
            case MapType.DiffuseOriginal:
                MainGui.Instance.DiffuseMapOriginal = newTexture;
                break;
            case MapType.Normal:
                MainGui.Instance.NormalMap = newTexture;
                break;
            case MapType.Metallic:
                MainGui.Instance.MetallicMap = newTexture;
                break;
            case MapType.Smoothness:
                MainGui.Instance.SmoothnessMap = newTexture;
                break;
            case MapType.Edge:
                MainGui.Instance.EdgeMap = newTexture;
                break;
            case MapType.Ao:
                MainGui.Instance.AoMap = newTexture;
                break;
            case MapType.Property:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(textureToLoad), textureToLoad, null);
        }

        MainGui.Instance.SetLoadedTexture(textureToLoad);

        Resources.UnloadUnusedAssets();


        yield return new WaitForSeconds(0.01f);

        Busy = false;
    }
}