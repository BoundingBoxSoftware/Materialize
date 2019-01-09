using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public enum MapType
{
    Height,
    Diffuse,
    DiffuseOriginal,
    Metallic,
    Smoothness,
    Normal,
    Edge,
    AO,
    Property
}

public enum FileFormat
{
    png,
    jpg,
    tga,
    exr
}

public class ProjectObject
{
    public HeightFromDiffuseSettings HFDS;
    public string heightMapPath;

    public EditDiffuseSettings EDS;
    public string diffuseMapPath;
    public string diffuseMapOriginalPath;

    public NormalFromHeightSettings NFHS;
    public string normalMapPath;

    public MetallicSettings MS;
    public string metallicMapPath;

    public SmoothnessSettings SS;
    public string smoothnessMapPath;

    public EdgeSettings ES;
    public string edgeMapPath;

    public AOSettings AOS;
    public string aoMapPath;

    public MaterialSettings MatS;
}

public class SaveLoadProject : MonoBehaviour
{
    public MainGui mainGui;
    public HeightFromDiffuseGui heightFromDiffuseGui;
    public EditDiffuseGui editDiffuseGui;
    public NormalFromHeightGui normalFromHeightGui;
    public MetallicGui metallicGui;
    public SmoothnessGui SmoothnessGui;
    public EdgeFromNormalGui edgeFromNormalGui;
    public AOFromNormalGui aoFromNormalGui;
    public MaterialGui materailGui;

    ProjectObject thisProject;

    char pathChar;

    public bool busy;

    // Use this for initialization
    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer)
        {
            pathChar = '\\';
        }
        else
        {
            pathChar = '/';
        }

        thisProject = new ProjectObject();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private string SwitchFormats(FileFormat selectedFormat)
    {
        switch (selectedFormat)
        {
            case FileFormat.png:
                return "png";
                
            case FileFormat.jpg:
                return "jpg";
                
            case FileFormat.tga:
                return "tga";
                
            case FileFormat.exr:
                return "exr";
            default:
                throw new ArgumentOutOfRangeException(nameof(selectedFormat), selectedFormat, null);
        }
    }

    public void LoadProject(string pathToFile)
    {
        UnityEngine.Debug.Log("Loading Project: " + pathToFile);

        var serializer = new XmlSerializer(typeof(ProjectObject));
        var stream = new FileStream(pathToFile, FileMode.Open);
        thisProject = serializer.Deserialize(stream) as ProjectObject;
        stream.Close();

        heightFromDiffuseGui.SetValues(thisProject);
        editDiffuseGui.SetValues(thisProject);
        normalFromHeightGui.SetValues(thisProject);
        metallicGui.SetValues(thisProject);
        SmoothnessGui.SetValues(thisProject);
        edgeFromNormalGui.SetValues(thisProject);
        aoFromNormalGui.SetValues(thisProject);
        materailGui.SetValues(thisProject);

        mainGui.ClearAllTextures();

        StartCoroutine(LoadAllTextures(pathToFile));
    }

    public void SaveProject(string pathToFile, FileFormat selectedFormat)
    {
        if (pathToFile.Contains("."))
        {
            pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf("."));
        }

        UnityEngine.Debug.Log("Saving Project: " + pathToFile);

        var extension = mainGui.SelectedFormat.ToString();

        heightFromDiffuseGui.GetValues(thisProject);
        if (mainGui._HeightMap != null)
        {
            thisProject.heightMapPath = pathToFile + "_height." + extension;
        }
        else
        {
            thisProject.heightMapPath = "null";
        }

        editDiffuseGui.GetValues(thisProject);
        if (mainGui._DiffuseMap != null)
        {
            thisProject.diffuseMapPath = pathToFile + "_diffuse." + extension;
        }
        else
        {
            thisProject.diffuseMapPath = "null";
        }

        if (mainGui._DiffuseMapOriginal != null)
        {
            thisProject.diffuseMapOriginalPath = pathToFile + "_diffuseOriginal." + extension;
        }
        else
        {
            thisProject.diffuseMapOriginalPath = "null";
        }

        normalFromHeightGui.GetValues(thisProject);
        if (mainGui._NormalMap != null)
        {
            thisProject.normalMapPath = pathToFile + "_normal." + extension;
        }
        else
        {
            thisProject.normalMapPath = "null";
        }

        metallicGui.GetValues(thisProject);
        if (mainGui._MetallicMap != null)
        {
            thisProject.metallicMapPath = pathToFile + "_metallic." + extension;
        }
        else
        {
            thisProject.metallicMapPath = "null";
        }

        SmoothnessGui.GetValues(thisProject);
        if (mainGui._SmoothnessMap != null)
        {
            thisProject.smoothnessMapPath = pathToFile + "_smoothness." + extension;
        }
        else
        {
            thisProject.smoothnessMapPath = "null";
        }

        edgeFromNormalGui.GetValues(thisProject);
        if (mainGui._EdgeMap != null)
        {
            thisProject.edgeMapPath = pathToFile + "_edge." + extension;
        }
        else
        {
            thisProject.edgeMapPath = "null";
        }

        aoFromNormalGui.GetValues(thisProject);
        if (mainGui._AOMap != null)
        {
            thisProject.aoMapPath = pathToFile + "_ao." + extension;
        }
        else
        {
            thisProject.aoMapPath = "null";
        }

        materailGui.GetValues(thisProject);

        var serializer = new XmlSerializer(typeof(ProjectObject));
        var stream = new FileStream(pathToFile + ".mtz", FileMode.Create);
        serializer.Serialize(stream, thisProject);
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

#if UNITY_STANDALONE_WIN
    public void PasteFile(MapType mapTypeToLoad)
    {
        string tempImagePath = Application.temporaryCachePath + "/temp.png";
        //string tempImagePath = Application.persistentDataPath + "/temp.png";
        UnityEngine.Debug.Log(tempImagePath);

        try
        {
            Process myProcess = new Process();
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = Application.streamingAssetsPath.Replace("/", "\\") + "\\c2i.exe";
            myProcess.StartInfo.Arguments = tempImagePath.Replace("/", "\\");
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.WaitForExit();

            StartCoroutine(LoadTexture(mapTypeToLoad, tempImagePath));
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
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
#endif

    //==============================================//
    //			Texture Saving Coroutines			//
    //==============================================//


    private IEnumerator SaveAllTextures(string pathToFile)
    {
        yield return StartCoroutine(SaveTexture(mainGui._HeightMap, thisProject.heightMapPath));

        yield return StartCoroutine(SaveTexture(mainGui._DiffuseMap, thisProject.diffuseMapPath));

        yield return StartCoroutine(SaveTexture(mainGui._DiffuseMapOriginal, thisProject.diffuseMapOriginalPath));

        yield return StartCoroutine(SaveTexture(mainGui._NormalMap, thisProject.normalMapPath));

        yield return StartCoroutine(SaveTexture(mainGui._MetallicMap, thisProject.metallicMapPath));

        yield return StartCoroutine(SaveTexture(mainGui._SmoothnessMap, thisProject.smoothnessMapPath));

        yield return StartCoroutine(SaveTexture(mainGui._EdgeMap, thisProject.edgeMapPath));

        yield return StartCoroutine(SaveTexture(mainGui._AOMap, thisProject.aoMapPath));
    }

    public IEnumerator SaveTexture(string extension, Texture2D textureToSave, string pathToFile)
    {
        yield return StartCoroutine(SaveTexture(textureToSave, pathToFile + "." + extension));
    }

    public IEnumerator SaveTexture(Texture2D textureToSave, string pathToFile)
    {
        if (!textureToSave || pathToFile.IsNullOrEmpty()) yield break;
        Debug.Log($"Salvando {textureToSave} como {pathToFile}");
        if (!pathToFile.Contains("."))
        {
            pathToFile = $"{pathToFile}.{mainGui.SelectedFormat}";
        }

        int fileIndex = pathToFile.LastIndexOf('.');
        string extension = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);

        switch (extension)
        {
            case "png":
                byte[] pngBytes = textureToSave.EncodeToPNG();
                File.WriteAllBytes(pathToFile, pngBytes);
                break;
            case "jpg":
                byte[] jpgBytes = textureToSave.EncodeToJPG();
                File.WriteAllBytes(pathToFile, jpgBytes);
                break;
            case "tga":
                byte[] tgaBytes = textureToSave.EncodeToTGA();
                File.WriteAllBytes(pathToFile, tgaBytes);
                break;
            case "exr":
                byte[] exrBytes = textureToSave.EncodeToEXR();
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
        pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf(pathChar));
        pathToFile += pathChar;

        if (thisProject.heightMapPath != "null")
        {
            StartCoroutine(LoadTexture(MapType.Height, pathToFile + thisProject.heightMapPath));
        }

        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if (thisProject.diffuseMapOriginalPath != "null")
        {
            StartCoroutine(LoadTexture(MapType.DiffuseOriginal, pathToFile + thisProject.diffuseMapOriginalPath));
        }

        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if (thisProject.diffuseMapPath != "null")
        {
            StartCoroutine(LoadTexture(MapType.Diffuse, pathToFile + thisProject.diffuseMapPath));
        }

        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if (thisProject.normalMapPath != "null")
        {
            StartCoroutine(LoadTexture(MapType.Normal, pathToFile + thisProject.normalMapPath));
        }

        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if (thisProject.metallicMapPath != "null")
        {
            StartCoroutine(LoadTexture(MapType.Metallic, pathToFile + thisProject.metallicMapPath));
        }

        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if (thisProject.smoothnessMapPath != "null")
        {
            StartCoroutine(LoadTexture(MapType.Smoothness, pathToFile + thisProject.smoothnessMapPath));
        }

        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if (thisProject.edgeMapPath != "null")
        {
            StartCoroutine(LoadTexture(MapType.Edge, pathToFile + thisProject.edgeMapPath));
        }

        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if (thisProject.aoMapPath != "null")
        {
            StartCoroutine(LoadTexture(MapType.AO, pathToFile + thisProject.aoMapPath));
        }

        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.01f);
    }

    public IEnumerator LoadTexture(MapType textureToLoad, string pathToFile)
    {
        busy = true;

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
                mainGui._HeightMap = newTexture;
                break;
            case MapType.Diffuse:
                mainGui._DiffuseMap = newTexture;
                break;
            case MapType.DiffuseOriginal:
                mainGui._DiffuseMapOriginal = newTexture;
                break;
            case MapType.Normal:
                mainGui._NormalMap = newTexture;
                break;
            case MapType.Metallic:
                mainGui._MetallicMap = newTexture;
                break;
            case MapType.Smoothness:
                mainGui._SmoothnessMap = newTexture;
                break;
            case MapType.Edge:
                mainGui._EdgeMap = newTexture;
                break;
            case MapType.AO:
                mainGui._AOMap = newTexture;
                break;
            default:
                break;
        }

        mainGui.SetLoadedTexture(textureToLoad);

        Resources.UnloadUnusedAssets();


        yield return new WaitForSeconds(0.01f);

        busy = false;
    }
}