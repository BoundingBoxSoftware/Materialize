#region

using System.Collections;
using System.ComponentModel;
using UnityEngine;

#endregion

public class EditDiffuseSettings
{
    [DefaultValue(50)] public int AvgColorBlurSize;

    [DefaultValue("50")] public string AvgColorBlurSizeText;

    [DefaultValue(0.0f)] public float BlurContrast;

    [DefaultValue("0")] public string BlurContrastText;

    [DefaultValue(20)] public int BlurSize;

    [DefaultValue("20")] public string BlurSizeText;

    [DefaultValue(0.5f)] public float ColorLerp;

    [DefaultValue("0.5")] public string ColorLerpText;

    [DefaultValue(0.5f)] public float DarkMaskPow;

    [DefaultValue("0.5")] public string DarkMaskPowText;

    [DefaultValue(0.0f)] public float DarkPow;

    [DefaultValue("0")] public string DarkPowText;

    [DefaultValue(0.0f)] public float DarkSpot;

    [DefaultValue("0")] public string DarkSpotText;

    [DefaultValue(0.0f)] public float FinalBias;

    [DefaultValue("0")] public string FinalBiasText;

    [DefaultValue(1.0f)] public float FinalContrast;

    [DefaultValue("1")] public string FinalContrastText;

    [DefaultValue(0.0f)] public float HotSpot;

    [DefaultValue("0")] public string HotSpotText;

    [DefaultValue(0.5f)] public float LightMaskPow;

    [DefaultValue("0.5")] public string LightMaskPowText;

    [DefaultValue(0f)] public float LightPow;

    [DefaultValue("0")] public string LightPowText;

    [DefaultValue(1.0f)] public float Saturation;

    [DefaultValue("1")] public string SaturationText;


    public EditDiffuseSettings()
    {
        AvgColorBlurSize = 50;
        AvgColorBlurSizeText = "50";

        BlurSize = 20;
        BlurSizeText = "20";

        BlurContrast = 0.0f;
        BlurContrastText = "0";

        LightMaskPow = 0.5f;
        LightMaskPowText = "0";

        LightPow = 0.0f;
        LightPowText = "0";

        DarkMaskPow = 0.5f;
        DarkMaskPowText = "0.5";

        DarkPow = 0.0f;
        DarkPowText = "0";

        HotSpot = 0.0f;
        HotSpotText = "0";

        DarkSpot = 0.0f;
        DarkSpotText = "0";

        FinalContrast = 1.0f;
        FinalContrastText = "1";

        FinalBias = 0.0f;
        FinalBiasText = "0";

        ColorLerp = 0.5f;
        ColorLerpText = "0.5";

        Saturation = 1.0f;
        SaturationText = "1";
    }
}

public class EditDiffuseGui : MonoBehaviour
{
    private static readonly int Slider = Shader.PropertyToID("_Slider");
    private static readonly int BlurContrast = Shader.PropertyToID("_BlurContrast");
    private static readonly int LightMaskPow = Shader.PropertyToID("_LightMaskPow");
    private static readonly int LightPow = Shader.PropertyToID("_LightPow");
    private static readonly int DarkMaskPow = Shader.PropertyToID("_DarkMaskPow");
    private static readonly int DarkPow = Shader.PropertyToID("_DarkPow");
    private static readonly int HotSpot = Shader.PropertyToID("_HotSpot");
    private static readonly int DarkSpot = Shader.PropertyToID("_DarkSpot");
    private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
    private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
    private static readonly int ColorLerp = Shader.PropertyToID("_ColorLerp");
    private static readonly int Saturation = Shader.PropertyToID("_Saturation");
    private static readonly int ImageSize = Shader.PropertyToID("_ImageSize");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int BlurTex = Shader.PropertyToID("_BlurTex");
    private static readonly int AvgTex = Shader.PropertyToID("_AvgTex");
    private static readonly int BlurSpread = Shader.PropertyToID("_BlurSpread");
    private static readonly int BlurSamples = Shader.PropertyToID("_BlurSamples");
    private static readonly int BlurDirection = Shader.PropertyToID("_BlurDirection");
    private readonly RenderTexture _avgTempMap;
    private RenderTexture _avgMap;
    private Material _blitMaterial;
    private RenderTexture _blurMap;

    private Texture2D _diffuseMap;
    private Texture2D _diffuseMapOriginal;
    private bool _doStuff;

    private EditDiffuseSettings _eds;

    private int _imageSizeX;
    private int _imageSizeY;
    private bool _newTexture;
    private bool _settingsInitialized;

    private float _slider = 0.5f;

    private RenderTexture _tempMap;

    private Rect _windowRect = new Rect(30, 300, 300, 450);

    public MainGui MainGuiScript;

    public GameObject TestObject;

    public Material ThisMaterial;

    public EditDiffuseGui(RenderTexture avgTempMap)
    {
        _avgTempMap = avgTempMap;
    }

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.EditDiffuseSettings = _eds;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.EditDiffuseSettings != null)
        {
            _eds = projectObject.EditDiffuseSettings;
        }
        else
        {
            _settingsInitialized = false;
            InitializeSettings();
        }

        _doStuff = true;
    }

    private void InitializeSettings()
    {
        if (_settingsInitialized) return;
        Debug.Log("Initializing Edit Diffuse Settings");
        _eds = new EditDiffuseSettings();
        _settingsInitialized = true;
    }


    // Use this for initialization
    private void Start()
    {
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        _blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));

        InitializeSettings();
    }

    public void DoStuff()
    {
        _doStuff = true;
    }

    public void NewTexture()
    {
        _newTexture = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_newTexture)
        {
            InitializeTextures();
            _newTexture = false;
        }

        if (_doStuff)
        {
            StartCoroutine(ProcessBlur());
            _doStuff = false;
        }


        ThisMaterial.SetFloat(Slider, _slider);

        ThisMaterial.SetFloat(BlurContrast, _eds.BlurContrast);

        ThisMaterial.SetFloat(LightMaskPow, _eds.LightMaskPow);
        ThisMaterial.SetFloat(LightPow, _eds.LightPow);

        ThisMaterial.SetFloat(DarkMaskPow, _eds.DarkMaskPow);
        ThisMaterial.SetFloat(DarkPow, _eds.DarkPow);

        ThisMaterial.SetFloat(HotSpot, _eds.HotSpot);
        ThisMaterial.SetFloat(DarkSpot, _eds.DarkSpot);

        ThisMaterial.SetFloat(FinalContrast, _eds.FinalContrast);
        ThisMaterial.SetFloat(FinalBias, _eds.FinalBias);

        ThisMaterial.SetFloat(ColorLerp, _eds.ColorLerp);

        ThisMaterial.SetFloat(Saturation, _eds.Saturation);
    }

    private void DoMyWindow(int windowId)
    {
        const int offsetX = 10;
        var offsetY = 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Diffuse Reveal Slider");
        _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
        offsetY += 50;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Average Color Blur Size", _eds.AvgColorBlurSize,
            _eds.AvgColorBlurSizeText, out _eds.AvgColorBlurSize, out _eds.AvgColorBlurSizeText, 5, 100))
            _doStuff = true;
        offsetY += 50;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Size", _eds.BlurSize, _eds.BlurSizeText,
            out _eds.BlurSize, out _eds.BlurSizeText, 5, 100)) _doStuff = true;
        offsetY += 30;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Contrast", _eds.BlurContrast,
            _eds.BlurContrastText, out _eds.BlurContrast, out _eds.BlurContrastText, -1.0f, 1.0f);
        offsetY += 50;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Light Mask Power", _eds.LightMaskPow,
            _eds.LightMaskPowText, out _eds.LightMaskPow, out _eds.LightMaskPowText, 0.0f, 1.0f);
        offsetY += 30;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Remove Light", _eds.LightPow, _eds.LightPowText,
            out _eds.LightPow, out _eds.LightPowText, 0.0f, 1.0f);
        offsetY += 50;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Shadow Mask Power", _eds.DarkMaskPow,
            _eds.DarkMaskPowText,
            out _eds.DarkMaskPow, out _eds.DarkMaskPowText, 0.0f, 1.0f);
        offsetY += 30;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Remove Shadow", _eds.DarkPow, _eds.DarkPowText,
            out _eds.DarkPow, out _eds.DarkPowText, 0.0f, 1.0f);
        offsetY += 50;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Hot Spot Removal", _eds.HotSpot, _eds.HotSpotText,
            out _eds.HotSpot, out _eds.HotSpotText, 0.0f, 1.0f);
        offsetY += 30;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Dark Spot Removal", _eds.DarkSpot, _eds.DarkSpotText,
            out _eds.DarkSpot, out _eds.DarkSpotText, 0.0f, 1.0f);
        offsetY += 50;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _eds.FinalContrast,
            _eds.FinalContrastText, out _eds.FinalContrast, out _eds.FinalContrastText, -2.0f, 2.0f);
        offsetY += 30;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _eds.FinalBias, _eds.FinalBiasText,
            out _eds.FinalBias, out _eds.FinalBiasText, -0.5f, 0.5f);
        offsetY += 50;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Keep Original Color", _eds.ColorLerp, _eds.ColorLerpText,
            out _eds.ColorLerp, out _eds.ColorLerpText, 0.0f, 1.0f);
        offsetY += 30;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Saturation", _eds.Saturation, _eds.SaturationText,
            out _eds.Saturation, out _eds.SaturationText, 0.0f, 1.0f);
        offsetY += 50;

        if (GUI.Button(new Rect(offsetX + 10, offsetY, 130, 30), "Reset to Defaults"))
        {
            //_settingsInitialized = false;
            SetValues(new ProjectObject());
            //StartCoroutine(ProcessDiffuse());
        }

        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as Diffuse"))
            StartCoroutine(ProcessDiffuse());

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = 650;

        _windowRect = GUI.Window(12, _windowRect, DoMyWindow, "Edit Diffuse");
    }

    public void Close()
    {
        CleanupTextures();
        gameObject.SetActive(false);
    }

    private static void CleanupTexture(RenderTexture texture)
    {
        if (!texture) return;
        texture.Release();
        // ReSharper disable once RedundantAssignment
        texture = null;
    }

    private void CleanupTextures()
    {
        CleanupTexture(_blurMap);
        CleanupTexture(_tempMap);
        CleanupTexture(_avgMap);
        CleanupTexture(_avgTempMap);
    }

    private void InitializeTextures()
    {
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        CleanupTextures();

        _diffuseMapOriginal = MainGuiScript.DiffuseMapOriginal;

        ThisMaterial.SetTexture(MainTex, _diffuseMapOriginal);

        _imageSizeX = _diffuseMapOriginal.width;
        _imageSizeY = _diffuseMapOriginal.height;

        Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

        _blurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _avgMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
    }

    private IEnumerator ProcessDiffuse()
    {
        Debug.Log("Processing Diffuse");

        _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        _blitMaterial.SetTexture(MainTex, _diffuseMapOriginal);

        _blitMaterial.SetTexture(BlurTex, _blurMap);
        _blitMaterial.SetFloat(BlurContrast, _eds.BlurContrast);

        _blitMaterial.SetTexture(AvgTex, _avgMap);

        _blitMaterial.SetFloat(LightMaskPow, _eds.LightMaskPow);
        _blitMaterial.SetFloat(LightPow, _eds.LightPow);

        _blitMaterial.SetFloat(DarkMaskPow, _eds.DarkMaskPow);
        _blitMaterial.SetFloat(DarkPow, _eds.DarkPow);

        _blitMaterial.SetFloat(HotSpot, _eds.HotSpot);
        _blitMaterial.SetFloat(DarkSpot, _eds.DarkSpot);

        _blitMaterial.SetFloat(FinalContrast, _eds.FinalContrast);

        _blitMaterial.SetFloat(FinalBias, _eds.FinalBias);

        _blitMaterial.SetFloat(ColorLerp, _eds.ColorLerp);

        _blitMaterial.SetFloat(Saturation, _eds.Saturation);

        CleanupTexture(_tempMap);
        _tempMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};

        Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 11);

        RenderTexture.active = _tempMap;

        if (MainGuiScript.DiffuseMap)
        {
            Destroy(MainGuiScript.DiffuseMap);
            MainGuiScript.DiffuseMap = null;
        }

        MainGuiScript.DiffuseMap = new Texture2D(_tempMap.width, _tempMap.height, TextureFormat.ARGB32, true, true);
        MainGuiScript.DiffuseMap.ReadPixels(new Rect(0, 0, _tempMap.width, _tempMap.height), 0, 0);
        MainGuiScript.DiffuseMap.Apply();

        yield return new WaitForSeconds(0.1f);

        CleanupTexture(_tempMap);
    }

    private IEnumerator ProcessBlur()
    {
        Debug.Log("Processing Blur");

        CleanupTexture(_tempMap);
        _tempMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};

        _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));
        _blitMaterial.SetFloat(BlurContrast, 1.0f);
        _blitMaterial.SetFloat(BlurSpread, 1.0f);

        // Blur the image 1
        _blitMaterial.SetInt(BlurSamples, _eds.BlurSize);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempMap, _blurMap, _blitMaterial, 1);
        ThisMaterial.SetTexture(BlurTex, _blurMap);


        _blitMaterial.SetTexture(MainTex, _diffuseMapOriginal);
        _blitMaterial.SetInt(BlurSamples, _eds.AvgColorBlurSize);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempMap, _avgMap, _blitMaterial, 1);

        _blitMaterial.SetFloat(BlurSpread, _eds.AvgColorBlurSize / 5.0f);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_avgMap, _tempMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempMap, _avgMap, _blitMaterial, 1);

        ThisMaterial.SetTexture(AvgTex, _avgMap);

        CleanupTexture(_tempMap);
        CleanupTexture(_avgTempMap);

        yield return new WaitForSeconds(0.01f);
    }
}