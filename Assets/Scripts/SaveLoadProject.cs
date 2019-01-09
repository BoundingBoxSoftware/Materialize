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
    bmp,
    jpg,
    png,
    tga
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

    string SwitchFormats(FileFormat selectedFormat)
    {
        string extension = "bmp";
        switch (selectedFormat)
        {
            case FileFormat.bmp:
                extension = "bmp";
                break;
            case FileFormat.jpg:
                extension = "jpg";
                break;
            case FileFormat.png:
                extension = "png";
                break;
            case FileFormat.tga:
                extension = "tga";
                break;
        }

        return extension;
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
        UnityEngine.Debug.Log("Saving Project: " + pathToFile);

        string extension = SwitchFormats(selectedFormat);

        if (pathToFile.Contains("."))
        {
            pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf("."));
        }

        int fileIndex = pathToFile.LastIndexOf(pathChar);
        string projectName = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);

        heightFromDiffuseGui.GetValues(thisProject);
        if (mainGui._HeightMap != null)
        {
            thisProject.heightMapPath = projectName + "_height." + extension;
        }
        else
        {
            thisProject.heightMapPath = "null";
        }

        editDiffuseGui.GetValues(thisProject);
        if (mainGui._DiffuseMap != null)
        {
            thisProject.diffuseMapPath = projectName + "_diffuse." + extension;
        }
        else
        {
            thisProject.diffuseMapPath = "null";
        }

        if (mainGui._DiffuseMapOriginal != null)
        {
            thisProject.diffuseMapOriginalPath = projectName + "_diffuseOriginal." + extension;
        }
        else
        {
            thisProject.diffuseMapOriginalPath = "null";
        }

        normalFromHeightGui.GetValues(thisProject);
        if (mainGui._NormalMap != null)
        {
            thisProject.normalMapPath = projectName + "_normal." + extension;
        }
        else
        {
            thisProject.normalMapPath = "null";
        }

        metallicGui.GetValues(thisProject);
        if (mainGui._MetallicMap != null)
        {
            thisProject.metallicMapPath = projectName + "_metallic." + extension;
        }
        else
        {
            thisProject.metallicMapPath = "null";
        }

        SmoothnessGui.GetValues(thisProject);
        if (mainGui._SmoothnessMap != null)
        {
            thisProject.smoothnessMapPath = projectName + "_smoothness." + extension;
        }
        else
        {
            thisProject.smoothnessMapPath = "null";
        }

        edgeFromNormalGui.GetValues(thisProject);
        if (mainGui._EdgeMap != null)
        {
            thisProject.edgeMapPath = projectName + "_edge." + extension;
        }
        else
        {
            thisProject.edgeMapPath = "null";
        }

        aoFromNormalGui.GetValues(thisProject);
        if (mainGui._AOMap != null)
        {
            thisProject.aoMapPath = projectName + "_ao." + extension;
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

        SaveAllFiles(pathToFile, selectedFormat);
    }

    public void SaveAllFiles(string pathToFile, FileFormat selectedFormat)
    {
        string extension = SwitchFormats(selectedFormat);
        if (pathToFile.Contains("."))
        {
            pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf("."));
        }

        StartCoroutine(SaveAllTextures(extension, pathToFile));
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


    public IEnumerator SaveAllTextures(string extension, string pathToFile)
    {
        StartCoroutine(SaveTexture(mainGui._HeightMap, pathToFile + "_height"));
        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(SaveTexture(mainGui._DiffuseMap, pathToFile + "_diffuse"));
        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(SaveTexture(mainGui._DiffuseMapOriginal, pathToFile + "_diffuseOriginal"));
        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(SaveTexture(mainGui._NormalMap, pathToFile + "_normal"));
        while (busy)
            if (!pathToFile.Contains("."))
            {
                pathToFile = $"{pathToFile}.{mainGui.SelectedFormat}";
            }

        {
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(SaveTexture(mainGui._MetallicMap, pathToFile + "_metallic"));
        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(SaveTexture(mainGui._SmoothnessMap, pathToFile + "_smoothness"));
        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(SaveTexture(mainGui._EdgeMap, pathToFile + "_edge"));
        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(SaveTexture(mainGui._AOMap, pathToFile + "_ao"));
        while (busy)
        {
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.01f);
    }

    public IEnumerator SaveTexture(string extension, Texture2D textureToSave, string pathToFile)
    {
        yield return StartCoroutine(SaveTexture(textureToSave, pathToFile + "." + extension));
    }

    public IEnumerator SaveTexture(Texture2D textureToSave, string pathToFile)
    {
        busy = true;

        if (!pathToFile.Contains("."))
        {
            pathToFile = $"{pathToFile}.{mainGui.SelectedFormat}";
        }

        if (textureToSave)
        {
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
        }

        yield return new WaitForSeconds(0.01f);
        busy = false;
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