#region

using System.Collections;
using UnityEngine;

#endregion

public class EdgeFromNormalGui : MonoBehaviour
{
    private static readonly int Blur0Weight = Shader.PropertyToID("_Blur0Weight");
    private static readonly int Blur1Weight = Shader.PropertyToID("_Blur1Weight");
    private static readonly int Blur2Weight = Shader.PropertyToID("_Blur2Weight");
    private static readonly int Blur3Weight = Shader.PropertyToID("_Blur3Weight");
    private static readonly int Blur4Weight = Shader.PropertyToID("_Blur4Weight");
    private static readonly int Blur5Weight = Shader.PropertyToID("_Blur5Weight");
    private static readonly int Blur6Weight = Shader.PropertyToID("_Blur6Weight");
    private static readonly int EdgeAmount = Shader.PropertyToID("_EdgeAmount");
    private static readonly int CreviceAmount = Shader.PropertyToID("_CreviceAmount");
    private static readonly int Pinch = Shader.PropertyToID("_Pinch");
    private static readonly int Pillow = Shader.PropertyToID("_Pillow");
    private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
    private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
    private static readonly int ImageSize = Shader.PropertyToID("_ImageSize");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int BlurTex0 = Shader.PropertyToID("_BlurTex0");
    private static readonly int BlurTex1 = Shader.PropertyToID("_BlurTex1");
    private static readonly int BlurTex2 = Shader.PropertyToID("_BlurTex2");
    private static readonly int BlurTex3 = Shader.PropertyToID("_BlurTex3");
    private static readonly int BlurTex4 = Shader.PropertyToID("_BlurTex4");
    private static readonly int BlurTex5 = Shader.PropertyToID("_BlurTex5");
    private static readonly int BlurTex6 = Shader.PropertyToID("_BlurTex6");
    private static readonly int BlurContrast = Shader.PropertyToID("_BlurContrast");
    private static readonly int BlurSamples = Shader.PropertyToID("_BlurSamples");
    private static readonly int BlurSpread = Shader.PropertyToID("_BlurSpread");
    private static readonly int BlurDirection = Shader.PropertyToID("_BlurDirection");
    private Material _blitMaterial;
    private RenderTexture _blurMap0;
    private RenderTexture _blurMap1;
    private RenderTexture _blurMap2;
    private RenderTexture _blurMap3;
    private RenderTexture _blurMap4;
    private RenderTexture _blurMap5;
    private RenderTexture _blurMap6;
    private bool _doStuff = true;

    private Texture2D _edgeMap;

    private int _imageSizeX = 1024;
    private int _imageSizeY = 1024;
    private bool _newTexture;
    private Texture2D _normalMap;

    private EdgeSettings _settings;

    private bool _settingsInitialized;
    private RenderTexture _tempBlurMap;
    private RenderTexture _tempEdgeMap;

    private Rect _windowRect = new Rect(30, 300, 300, 600);

    [HideInInspector] public bool Busy;

    public MainGui MainGuiScript;

    public GameObject TestObject;

    public Material ThisMaterial;

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.EdgeSettings = _settings;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.EdgeSettings != null)
        {
            _settings = projectObject.EdgeSettings;
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
        Debug.Log("Initializing Edge Settings");
        _settings = new EdgeSettings();
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
            StartCoroutine(ProcessNormal());
            _doStuff = false;
        }

        ThisMaterial.SetFloat(Blur0Weight, _settings.Blur0Weight * _settings.Blur0Weight * _settings.Blur0Weight);
        ThisMaterial.SetFloat(Blur1Weight, _settings.Blur1Weight * _settings.Blur1Weight * _settings.Blur1Weight);
        ThisMaterial.SetFloat(Blur2Weight, _settings.Blur2Weight * _settings.Blur2Weight * _settings.Blur2Weight);
        ThisMaterial.SetFloat(Blur3Weight, _settings.Blur3Weight * _settings.Blur3Weight * _settings.Blur3Weight);
        ThisMaterial.SetFloat(Blur4Weight, _settings.Blur4Weight * _settings.Blur4Weight * _settings.Blur4Weight);
        ThisMaterial.SetFloat(Blur5Weight, _settings.Blur5Weight * _settings.Blur5Weight * _settings.Blur5Weight);
        ThisMaterial.SetFloat(Blur6Weight, _settings.Blur6Weight * _settings.Blur6Weight * _settings.Blur6Weight);
        ThisMaterial.SetFloat(EdgeAmount, _settings.EdgeAmount);
        ThisMaterial.SetFloat(CreviceAmount, _settings.CreviceAmount);
        ThisMaterial.SetFloat(Pinch, _settings.Pinch);
        ThisMaterial.SetFloat(Pillow, _settings.Pillow);
        ThisMaterial.SetFloat(FinalContrast, _settings.FinalContrast);
        ThisMaterial.SetFloat(FinalBias, _settings.FinalBias);
    }

    private void SetDefaultSliderValues()
    {
        _settings.Blur0Contrast = 1.0f;
        _settings.Blur0ContrastText = "1";

        _settings.EdgeAmount = 1.0f;
        _settings.EdgeAmountText = "1";

        _settings.CreviceAmount = 1.0f;
        _settings.CreviceAmountText = "1";

        _settings.Pinch = 1.0f;
        _settings.PinchText = "1";

        _settings.Pillow = 1.0f;
        _settings.PillowText = "1";

        _settings.FinalContrast = 2.0f;
        _settings.FinalContrastText = "2";

        _settings.FinalBias = 0.0f;
        _settings.FinalBiasText = "0";
    }

    private void SetWeightEqDefault()
    {
        _settings.Blur0Weight = 1.0f;
        _settings.Blur1Weight = 0.5f;
        _settings.Blur2Weight = 0.3f;
        _settings.Blur3Weight = 0.5f;
        _settings.Blur4Weight = 0.7f;
        _settings.Blur5Weight = 0.7f;
        _settings.Blur6Weight = 0.3f;

        SetDefaultSliderValues();

        _doStuff = true;
    }

    private void SetWeightEqDisplace()
    {
        _settings.Blur0Weight = 0.1f;
        _settings.Blur1Weight = 0.15f;
        _settings.Blur2Weight = 0.25f;
        _settings.Blur3Weight = 0.45f;
        _settings.Blur4Weight = 0.75f;
        _settings.Blur5Weight = 0.95f;
        _settings.Blur6Weight = 1.0f;

        SetDefaultSliderValues();

        _settings.Blur0Contrast = 3.0f;
        _settings.Blur0ContrastText = "3";

        _settings.FinalContrast = 5.0f;
        _settings.FinalContrastText = "5";

        _settings.FinalBias = -0.2f;
        _settings.FinalBiasText = "-0.2";

        _doStuff = true;
    }

    private void SetWeightEqSoft()
    {
        _settings.Blur0Weight = 0.15f;
        _settings.Blur1Weight = 0.4f;
        _settings.Blur2Weight = 0.7f;
        _settings.Blur3Weight = 0.9f;
        _settings.Blur4Weight = 1.0f;
        _settings.Blur5Weight = 0.9f;
        _settings.Blur6Weight = 0.7f;

        SetDefaultSliderValues();

        _settings.FinalContrast = 4.0f;
        _settings.FinalContrastText = "4";

        _doStuff = true;
    }

    private void SetWeightEqTight()
    {
        _settings.Blur0Weight = 1.0f;
        _settings.Blur1Weight = 0.45f;
        _settings.Blur2Weight = 0.25f;
        _settings.Blur3Weight = 0.18f;
        _settings.Blur4Weight = 0.15f;
        _settings.Blur5Weight = 0.13f;
        _settings.Blur6Weight = 0.1f;

        SetDefaultSliderValues();

        _settings.Pinch = 1.5f;
        _settings.PinchText = "1.5";

        _doStuff = true;
    }

    private void DoMyWindow(int windowId)
    {
        var offsetX = 10;
        var offsetY = 30;

        _doStuff = GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Pre Contrast", _settings.Blur0Contrast,
            _settings.Blur0ContrastText, out _settings.Blur0Contrast, out _settings.Blur0ContrastText, 0.0f, 5.0f);
        offsetY += 50;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Equalizer");
        GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
        if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default")) SetWeightEqDefault();
        if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Displace")) SetWeightEqDisplace();
        if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Soft")) SetWeightEqSoft();
        if (GUI.Button(new Rect(offsetX + 215, offsetY + 120, 60, 20), "Tight")) SetWeightEqTight();
        offsetY += 30;
        offsetX += 10;
        _settings.Blur0Weight =
            GUI.VerticalSlider(new Rect(offsetX + 180, offsetY, 10, 100), _settings.Blur0Weight, 1.0f, 0.0f);
        _settings.Blur1Weight =
            GUI.VerticalSlider(new Rect(offsetX + 150, offsetY, 10, 100), _settings.Blur1Weight, 1.0f, 0.0f);
        _settings.Blur2Weight =
            GUI.VerticalSlider(new Rect(offsetX + 120, offsetY, 10, 100), _settings.Blur2Weight, 1.0f, 0.0f);
        _settings.Blur3Weight =
            GUI.VerticalSlider(new Rect(offsetX + 90, offsetY, 10, 100), _settings.Blur3Weight, 1.0f, 0.0f);
        _settings.Blur4Weight =
            GUI.VerticalSlider(new Rect(offsetX + 60, offsetY, 10, 100), _settings.Blur4Weight, 1.0f, 0.0f);
        _settings.Blur5Weight =
            GUI.VerticalSlider(new Rect(offsetX + 30, offsetY, 10, 100), _settings.Blur5Weight, 1.0f, 0.0f);
        _settings.Blur6Weight =
            GUI.VerticalSlider(new Rect(offsetX + 0, offsetY, 10, 100), _settings.Blur6Weight, 1.0f, 0.0f);
        offsetX -= 10;
        offsetY += 120;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Edge Amount", _settings.EdgeAmount,
            _settings.EdgeAmountText,
            out _settings.EdgeAmount, out _settings.EdgeAmountText, 0.0f, 1.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Crevice Amount", _settings.CreviceAmount,
            _settings.CreviceAmountText,
            out _settings.CreviceAmount, out _settings.CreviceAmountText, 0.0f, 1.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Pinch", _settings.Pinch, _settings.PinchText,
            out _settings.Pinch,
            out _settings.PinchText, 0.1f, 10.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Pillow", _settings.Pillow, _settings.PillowText,
            out _settings.Pillow,
            out _settings.PillowText, 0.1f, 5.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _settings.FinalContrast,
            _settings.FinalContrastText,
            out _settings.FinalContrast, out _settings.FinalContrastText, 0.1f, 30.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _settings.FinalBias,
            _settings.FinalBiasText,
            out _settings.FinalBias, out _settings.FinalBiasText, -1.0f, 1.0f);
        offsetY += 50;

        if (GUI.Button(new Rect(offsetX + 10, offsetY, 130, 30), "Reset to Defaults"))
        {
            //_settingsInitialized = false;
            SetValues(new ProjectObject());
            //StartCoroutine(ProcessDiffuse());
        }

        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as Edge Map")) StartCoroutine(ProcessEdge());

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = 520;

        _windowRect = GUI.Window(11, _windowRect, DoMyWindow, "Edge from Normal");
    }

    public void InitializeTextures()
    {
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        CleanupTextures();

        _normalMap = MainGuiScript.NormalMap;

        _imageSizeX = _normalMap.width;
        _imageSizeY = _normalMap.height;

        Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

        _tempBlurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap0 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap1 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap2 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap3 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap4 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap5 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap6 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        StartCoroutine(ProcessNormal());
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
        CleanupTexture(_tempBlurMap);
        CleanupTexture(_blurMap0);
        CleanupTexture(_blurMap1);
        CleanupTexture(_blurMap2);
        CleanupTexture(_blurMap3);
        CleanupTexture(_blurMap4);
        CleanupTexture(_blurMap5);
        CleanupTexture(_blurMap6);
        CleanupTexture(_tempEdgeMap);
    }

    public IEnumerator ProcessEdge()
    {
        Busy = true;

        Debug.Log("Processing Height / Edge");

        _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        _blitMaterial.SetFloat(Blur0Weight, _settings.Blur0Weight * _settings.Blur0Weight * _settings.Blur0Weight);
        _blitMaterial.SetFloat(Blur1Weight, _settings.Blur1Weight * _settings.Blur1Weight * _settings.Blur1Weight);
        _blitMaterial.SetFloat(Blur2Weight, _settings.Blur2Weight * _settings.Blur2Weight * _settings.Blur2Weight);
        _blitMaterial.SetFloat(Blur3Weight, _settings.Blur3Weight * _settings.Blur3Weight * _settings.Blur3Weight);
        _blitMaterial.SetFloat(Blur4Weight, _settings.Blur4Weight * _settings.Blur4Weight * _settings.Blur4Weight);
        _blitMaterial.SetFloat(Blur5Weight, _settings.Blur5Weight * _settings.Blur5Weight * _settings.Blur5Weight);
        _blitMaterial.SetFloat(Blur6Weight, _settings.Blur6Weight * _settings.Blur6Weight * _settings.Blur6Weight);
        _blitMaterial.SetFloat(EdgeAmount, _settings.EdgeAmount);
        _blitMaterial.SetFloat(CreviceAmount, _settings.CreviceAmount);
        _blitMaterial.SetFloat(Pinch, _settings.Pinch);
        _blitMaterial.SetFloat(Pillow, _settings.Pillow);
        _blitMaterial.SetFloat(FinalContrast, _settings.FinalContrast);
        _blitMaterial.SetFloat(FinalBias, _settings.FinalBias);

        _blitMaterial.SetTexture(MainTex, _normalMap);
        _blitMaterial.SetTexture(BlurTex0, _blurMap0);
        _blitMaterial.SetTexture(BlurTex1, _blurMap1);
        _blitMaterial.SetTexture(BlurTex2, _blurMap2);
        _blitMaterial.SetTexture(BlurTex3, _blurMap3);
        _blitMaterial.SetTexture(BlurTex4, _blurMap4);
        _blitMaterial.SetTexture(BlurTex5, _blurMap5);
        _blitMaterial.SetTexture(BlurTex6, _blurMap6);

        // Save low fidelity for texture 2d

        CleanupTexture(_tempEdgeMap);
        _tempEdgeMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        Graphics.Blit(_blurMap0, _tempEdgeMap, _blitMaterial, 6);

        RenderTexture.active = _tempEdgeMap;

        if (MainGuiScript.EdgeMap) Destroy(MainGuiScript.EdgeMap);

        MainGuiScript.EdgeMap =
            new Texture2D(_tempEdgeMap.width, _tempEdgeMap.height, TextureFormat.ARGB32, true, true);
        MainGuiScript.EdgeMap.ReadPixels(new Rect(0, 0, _tempEdgeMap.width, _tempEdgeMap.height), 0, 0);
        MainGuiScript.EdgeMap.Apply();

        yield return new WaitForSeconds(0.1f);

        CleanupTexture(_tempEdgeMap);

        Busy = false;
    }

    public IEnumerator ProcessNormal()
    {
        Busy = true;

        Debug.Log("Processing Normal to Edge");

        _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        // Make normal from height
        _blitMaterial.SetFloat(BlurContrast, _settings.Blur0Contrast);
        _blitMaterial.SetTexture(MainTex, _normalMap);
        Graphics.Blit(_normalMap, _blurMap0, _blitMaterial, 5);

        _blitMaterial.SetFloat(BlurContrast, 1.0f);

        // Blur the image 1
        _blitMaterial.SetTexture(MainTex, _blurMap0);
        _blitMaterial.SetInt(BlurSamples, 4);
        _blitMaterial.SetFloat(BlurSpread, 1.0f);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap0, _tempBlurMap, _blitMaterial, 1);

        _blitMaterial.SetTexture(MainTex, _tempBlurMap);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap1, _blitMaterial, 1);


        // Blur the image 2
        _blitMaterial.SetTexture(MainTex, _blurMap1);
        _blitMaterial.SetFloat(BlurSpread, 2.0f);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap1, _tempBlurMap, _blitMaterial, 1);

        _blitMaterial.SetTexture(MainTex, _tempBlurMap);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap2, _blitMaterial, 1);


        // Blur the image 3
        _blitMaterial.SetTexture(MainTex, _blurMap2);
        _blitMaterial.SetFloat(BlurSpread, 4.0f);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap2, _tempBlurMap, _blitMaterial, 1);

        _blitMaterial.SetTexture(MainTex, _tempBlurMap);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap3, _blitMaterial, 1);


        // Blur the image 4
        _blitMaterial.SetTexture(MainTex, _blurMap3);
        _blitMaterial.SetFloat(BlurSpread, 8.0f);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap3, _tempBlurMap, _blitMaterial, 1);

        _blitMaterial.SetTexture(MainTex, _tempBlurMap);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap4, _blitMaterial, 1);


        // Blur the image 5
        _blitMaterial.SetTexture(MainTex, _blurMap4);
        _blitMaterial.SetFloat(BlurSpread, 16.0f);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap4, _tempBlurMap, _blitMaterial, 1);

        _blitMaterial.SetTexture(MainTex, _tempBlurMap);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap5, _blitMaterial, 1);


        // Blur the image 6
        _blitMaterial.SetTexture(MainTex, _blurMap5);
        _blitMaterial.SetFloat(BlurSpread, 32.0f);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap5, _tempBlurMap, _blitMaterial, 1);

        _blitMaterial.SetTexture(MainTex, _tempBlurMap);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap6, _blitMaterial, 1);


        ThisMaterial.SetTexture(MainTex, _blurMap0);
        ThisMaterial.SetTexture(BlurTex0, _blurMap0);
        ThisMaterial.SetTexture(BlurTex1, _blurMap1);
        ThisMaterial.SetTexture(BlurTex2, _blurMap2);
        ThisMaterial.SetTexture(BlurTex3, _blurMap3);
        ThisMaterial.SetTexture(BlurTex4, _blurMap4);
        ThisMaterial.SetTexture(BlurTex5, _blurMap5);
        ThisMaterial.SetTexture(BlurTex6, _blurMap6);

        yield return new WaitForSeconds(0.1f);

        Busy = false;
    }
}