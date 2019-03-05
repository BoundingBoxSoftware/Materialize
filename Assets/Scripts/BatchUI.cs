using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SFB;
using System.Linq;
using System;

public class BatchUI : MonoBehaviour
{
    public MainGui MainGui;
   // public HeightFromDiffuseGui HeightmapCreator;
    public bool UseInitalLocation;
    bool PathIsSet;
    string path = null;
    public bool ProcessPropertyMap;
    // Start is called before the first frame update
    void Start()
    {
        MainGui = FindObjectOfType<MainGui>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UseInitalLocationToggle(bool value) { UseInitalLocation = value; }

    public void BatchLoadTextures()
    {
        StartCoroutine(BatchProcessTextures());
    }
    public IEnumerator BatchProcessTextures()
    {
        var path = StandaloneFileBrowser.OpenFolderPanel("Texture Files Location", "", false);
        //var path = StandaloneFileBrowser.SaveFilePanel("Texture Directory", "", "","");
        var s = Directory.GetFiles(path[0], "*.*").Where(g => g.EndsWith(".jpg") || g.EndsWith(".png"));
        foreach (string f in s)
        {
            //BatchProcessTextures(f);

            byte[] Data = System.IO.File.ReadAllBytes(f);
            Texture2D Tex = new Texture2D(2, 2);
            if (Tex.LoadImage(Data))
            {
                yield return StartCoroutine(BatchTextures(Tex, f));

                //MainGui.HeightFromDiffuseGuiScript.StartCoroutine(ProcessHeight());
                //return null;
            }
        }
        //return null;
    }
    IEnumerator BatchTextures(Texture2D T, string name)
    {
        MainGui.DiffuseMapOriginal = T;
        MainGui.HeightFromDiffuseGuiObject.SetActive(true);
        MainGui.HeightFromDiffuseGuiScript.NewTexture();
        MainGui.HeightFromDiffuseGuiScript.DoStuff();
        //yield return MainGui.HeightFromDiffuseGuiScript.StartCoroutine(MainGui.HeightFromDiffuseGuiScript.ProcessDiffuse());
        yield return new WaitForSeconds(.1f);
        MainGui.HeightFromDiffuseGuiScript.StartProcessHeight();
        MainGui.CloseWindows();
        MainGui.FixSize();
        MainGui.NormalFromHeightGuiObject.SetActive(true);
        MainGui.NormalFromHeightGuiScript.NewTexture();
        MainGui.NormalFromHeightGuiScript.DoStuff();
        //yield return MainGui.NormalFromHeightGuiScript.StartCoroutine(MainGui.NormalFromHeightGuiScript.ProcessHeight());
        yield return new WaitForSeconds(.1f);
        MainGui.NormalFromHeightGuiScript.StartProcessNormal();
        yield return new WaitForEndOfFrame();
        MainGui.CloseWindows();
        MainGui.FixSize();
        MainGui.MetallicMap = new Texture2D(MainGui.HeightMap.width, MainGui.HeightMap.height);
        Color theColor = new Color();
        for (int x = 0; x < MainGui.MetallicMap.width; x++)
        {
            for (int y = 0; y < MainGui.MetallicMap.height; y++)
            {
                theColor.r = 0;
                theColor.g = 0;
                theColor.b = 0;
                theColor.a = 255;
                MainGui.MetallicMap.SetPixel(x, y, theColor);
            }
        }
        // MainGui.MetallicGuiObject.SetActive(true);
        //MainGui.MetallicGuiScript.NewTexture();
        //MainGui.MetallicGuiScript.DoStuff();
        //yield return new WaitForSeconds(.1f);
        //MainGui.MetallicGuiScript.StartCoroutine(MainGui.MetallicGuiScript.ProcessMetallic());
        MainGui.MetallicMap.Apply();
        MainGui.CloseWindows();
        MainGui.FixSize();
        MainGui.SmoothnessGuiObject.SetActive(true);
        MainGui.SmoothnessGuiScript.NewTexture();
        MainGui.SmoothnessGuiScript.DoStuff();
        yield return new WaitForSeconds(.1f);
        MainGui.SmoothnessGuiScript.StartCoroutine(MainGui.SmoothnessGuiScript.ProcessSmoothness());
        MainGui.CloseWindows();
        MainGui.FixSize();
        MainGui.EdgeFromNormalGuiObject.SetActive(true);
        MainGui.EdgeFromNormalGuiScript.NewTexture();
        MainGui.EdgeFromNormalGuiScript.DoStuff();
        yield return new WaitForSeconds(.1f);
        MainGui.EdgeFromNormalGuiScript.StartCoroutine(MainGui.EdgeFromNormalGuiScript.ProcessEdge());
        MainGui.CloseWindows();
        MainGui.FixSize();
        MainGui.AoFromNormalGuiObject.SetActive(true);
        MainGui.AoFromNormalGuiScript.NewTexture();
        MainGui.AoFromNormalGuiScript.DoStuff();
        yield return new WaitForSeconds(.1f);
        MainGui.AoFromNormalGuiScript.StartCoroutine(MainGui.AoFromNormalGuiScript.ProcessAo());

        yield return new WaitForSeconds(.3f);
        
        List<string> names = name.Split( new string[] { "/", "\\" }, StringSplitOptions.None).ToList<string>();
        //Debug.Log(names);
        foreach(var s in names)
        {
            Debug.Log(s);
        }
        string defaultName = names[names.Count - 1];
        Debug.Log(defaultName);
        names = defaultName.Split('.').ToList<string>();
        defaultName = names[0];
        string NameWithOutExtension = defaultName;
        defaultName = defaultName + ".mtz";
        
        if (UseInitalLocation)
        {
            if (!PathIsSet)
            {
                path = StandaloneFileBrowser.SaveFilePanel("Save Project", MainGui._lastDirectory, defaultName, "mtz");
                //if (path.IsNullOrEmpty()) return;
                PathIsSet = true;
                var lastBar = path.LastIndexOf(MainGui._pathChar);
                MainGui._lastDirectory = path.Substring(0, lastBar + 1);

            }
            else
            {
                List<string> PathSplit = path.Split(new string[] { "/", "\\" }, StringSplitOptions.None).ToList<string>();
                //PathSplit[PathSplit.Length - 1]
                PathSplit.RemoveAt(PathSplit.Count - 1);
                //Debug.Log(PathSplit);
                path = string.Join("/" , PathSplit.ToArray());
                path = path+ "/" + defaultName;
                Debug.Log(defaultName);
                //var lastBar = path.LastIndexOf(MainGui._pathChar);
                //MainGui._lastDirectory = path.Substring(0, lastBar + 1);
            }
        }
        else
        {
            path = StandaloneFileBrowser.SaveFilePanel("Save Project", MainGui._lastDirectory, defaultName, "mtz");
            var lastBar = path.LastIndexOf(MainGui._pathChar);
            MainGui._lastDirectory = path.Substring(0, lastBar + 1);
        }
        Debug.Log(path);
        MainGui._saveLoadProjectScript.SaveProject(path);
        yield return new WaitForSeconds(1f);
        if (ProcessPropertyMap)
        {
            MainGui.ProcessPropertyMap();
            MainGui.SaveTextureFile(MapType.Property, path, NameWithOutExtension);
        }
        //return null;
    }
}
