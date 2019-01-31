using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class AOSettings
{
    [DefaultValue(1.0f)] public float Blend;

    [DefaultValue(1.0f)] public float BlendAmount;

    [DefaultValue("1")] public string BlendText;

    [DefaultValue(100.0f)] public float Depth;

    [DefaultValue("100")] public string DepthText;

    [DefaultValue(0.0f)] public float FinalBias;

    [DefaultValue("0")] public string FinalBiasText;

    [DefaultValue(1.0f)] public float FinalContrast;

    [DefaultValue("1")] public string FinalContrastText;

    [DefaultValue(5.0f)] public float Spread;

    [DefaultValue("50")] public string SpreadText;

    public AOSettings()
    {
        Spread = 50.0f;
        SpreadText = "50";

        Depth = 100.0f;
        DepthText = "100";

        FinalBias = 0.0f;
        FinalBiasText = "0";

        FinalContrast = 1.0f;
        FinalContrastText = "1";

        Blend = 1.0f;
        BlendText = "1";

        BlendAmount = 1.0f;
    }
}

public class AOFromNormalGui : MonoBehaviour
{
    private Texture2D _AOMap;
    private RenderTexture _BlendedAOMap;
    private RenderTexture _TempAOMap;
    private RenderTexture _WorkingAOMap;

    private AOSettings AOS;
    private Material blitMaterial;

    public bool busy;
    public Texture2D defaultHeight;

    public Texture2D defaultNormal;
    private bool doStuff;

    private int imageSizeX = 1024;
    private int imageSizeY = 1024;

    public MainGui MainGuiScript;
    private bool newTexture;

    private bool settingsInitialized;

    public GameObject testObject;

    public Material thisMaterial;

    private Rect windowRect = new Rect(30, 300, 300, 230);

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.AoSettings = AOS;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.AoSettings != null)
        {
            AOS = projectObject.AoSettings;
        }
        else
        {
            settingsInitialized = false;
            InitializeSettings();
        }

        doStuff = true;
    }

    private void InitializeSettings()
    {
        if (settingsInitialized == false)
        {
            AOS = new AOSettings();

            if (MainGuiScript.HeightMap != null)
            {
                AOS.Blend = 1.0f;
                AOS.BlendText = "1.0";
            }
            else
            {
                AOS.Blend = 0.0f;
                AOS.BlendText = "0.0";
            }

            settingsInitialized = true;
        }
    }

    // Use this for initialization
    private void Start()
    {
        testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;

        blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));

        //blitMaterial = new Material (Shader.Find ("Hidden/Blit_Height_From_Normal"));

        InitializeSettings();
    }

    public void DoStuff()
    {
        doStuff = true;
    }

    public void NewTexture()
    {
        newTexture = true;
    }


    // Update is called once per frame
    private void Update()
    {
        if (newTexture)
        {
            InitializeTextures();
            newTexture = false;
        }

        if (doStuff)
        {
            StopAllCoroutines();

            StartCoroutine(ProcessNormalDepth());
            doStuff = false;
        }

        thisMaterial.SetFloat("_FinalContrast", AOS.FinalContrast);
        thisMaterial.SetFloat("_FinalBias", AOS.FinalBias);
        thisMaterial.SetFloat("_AOBlend", AOS.Blend);
    }

    private void DoMyWindow(int windowID)
    {
        var spacingX = 0;
        var spacingY = 50;

        var offsetX = 10;
        var offsetY = 30;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO pixel Spread", AOS.Spread, AOS.SpreadText,
            out AOS.Spread, out AOS.SpreadText, 10.0f, 100.0f)) doStuff = true;
        offsetY += 40;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Pixel Depth", AOS.Depth, AOS.DepthText,
            out AOS.Depth, out AOS.DepthText, 0.0f, 256.0f)) doStuff = true;
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Blend Normal AO and Depth AO", AOS.Blend, AOS.BlendText,
            out AOS.Blend, out AOS.BlendText, 0.0f, 1.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO Power", AOS.FinalContrast, AOS.FinalContrastText,
            out AOS.FinalContrast, out AOS.FinalContrastText, 0.1f, 10.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "AO Bias", AOS.FinalBias, AOS.FinalBiasText,
            out AOS.FinalBias, out AOS.FinalBiasText, -1.0f, 1.0f);
        offsetY += 50;

        if (busy)
            GUI.enabled = false;
        else
            GUI.enabled = true;
        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as AO Map")) StartCoroutine(ProcessAO());
        GUI.enabled = true;
        GUI.DragWindow();
    }

    private void OnGUI()
    {
        windowRect.width = 300;
        windowRect.height = 280;

        windowRect = GUI.Window(10, windowRect, DoMyWindow, "Normal + Depth to AO");
    }

    public void InitializeTextures()
    {
        testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;

        CleanupTextures();

        if (MainGuiScript.NormalMap != null)
        {
            imageSizeX = MainGuiScript.NormalMap.width;
            imageSizeY = MainGuiScript.NormalMap.height;
        }
        else
        {
            imageSizeX = MainGuiScript.HeightMap.width;
            imageSizeY = MainGuiScript.HeightMap.height;
        }

        Debug.Log("Initializing Textures of size: " + imageSizeX + "x" + imageSizeY);

        _WorkingAOMap = new RenderTexture(imageSizeX, imageSizeY, 0, RenderTextureFormat.RGHalf,
            RenderTextureReadWrite.Linear);
        _WorkingAOMap.wrapMode = TextureWrapMode.Repeat;
        _BlendedAOMap = new RenderTexture(imageSizeX, imageSizeY, 0, RenderTextureFormat.RGHalf,
            RenderTextureReadWrite.Linear);
        _BlendedAOMap.wrapMode = TextureWrapMode.Repeat;
    }

    public void Close()
    {
        CleanupTextures();
        gameObject.SetActive(false);
    }

    private void CleanupTexture(RenderTexture _Texture)
    {
        if (_Texture != null)
        {
            _Texture.Release();
            _Texture = null;
        }
    }

    private void CleanupTextures()
    {
        Debug.Log("Cleaning Up Textures");

        CleanupTexture(_WorkingAOMap);
        CleanupTexture(_BlendedAOMap);
        CleanupTexture(_TempAOMap);
    }

    public IEnumerator ProcessAO()
    {
        busy = true;

        Debug.Log("Processing AO Map");

        CleanupTexture(_TempAOMap);
        _TempAOMap = new RenderTexture(imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);
        _TempAOMap.wrapMode = TextureWrapMode.Repeat;

        blitMaterial.SetFloat("_FinalBias", AOS.FinalBias);
        blitMaterial.SetFloat("_FinalContrast", AOS.FinalContrast);
        blitMaterial.SetTexture("_MainTex", _BlendedAOMap);
        blitMaterial.SetFloat("_AOBlend", AOS.Blend);

        Graphics.Blit(_BlendedAOMap, _TempAOMap, blitMaterial, 8);

        if (MainGuiScript.AoMap != null) Destroy(MainGuiScript.AoMap);

        RenderTexture.active = _TempAOMap;
        MainGuiScript.AoMap = new Texture2D(_TempAOMap.width, _TempAOMap.height, TextureFormat.ARGB32, true, true);
        MainGuiScript.AoMap.ReadPixels(new Rect(0, 0, _TempAOMap.width, _TempAOMap.height), 0, 0);
        MainGuiScript.AoMap.Apply();

        yield return new WaitForSeconds(0.1f);

        CleanupTexture(_TempAOMap);

        busy = false;
    }

    public IEnumerator ProcessNormalDepth()
    {
        busy = true;

        Debug.Log("Processing Normal Depth to AO");

        blitMaterial.SetVector("_ImageSize", new Vector4(imageSizeX, imageSizeY, 0, 0));
        blitMaterial.SetFloat("_Spread", AOS.Spread);

        if (MainGuiScript.NormalMap != null)
            blitMaterial.SetTexture("_MainTex", MainGuiScript.NormalMap);
        else
            blitMaterial.SetTexture("_MainTex", defaultNormal);

        if (MainGuiScript.HdHeightMap != null)
            blitMaterial.SetTexture("_HeightTex", MainGuiScript.HdHeightMap);
        else if (MainGuiScript.HeightMap != null)
            blitMaterial.SetTexture("_HeightTex", MainGuiScript.HeightMap);
        else
            blitMaterial.SetTexture("_HeightTex", defaultHeight);

        blitMaterial.SetTexture("_BlendTex", _BlendedAOMap);
        blitMaterial.SetFloat("_Depth", AOS.Depth);
        thisMaterial.SetTexture("_MainTex", _BlendedAOMap);

        var yieldCountDown = 5;

        for (var i = 1; i < 100; i++)
        {
            blitMaterial.SetFloat("_BlendAmount", 1.0f / i);
            blitMaterial.SetFloat("_Progress", i / 100.0f);

            Graphics.Blit(MainGuiScript.NormalMap, _WorkingAOMap, blitMaterial, 7);
            Graphics.Blit(_WorkingAOMap, _BlendedAOMap);


            yieldCountDown -= 1;
            if (yieldCountDown <= 0)
            {
                yieldCountDown = 5;
                yield return new WaitForSeconds(0.01f);
            }
        }

        yield return new WaitForSeconds(0.01f);

        busy = false;
    }
}