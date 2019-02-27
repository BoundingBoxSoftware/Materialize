#region

using System.Collections;
using UnityEngine;

#endregion

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

public class MetallicGui : MonoBehaviour
{
    private static readonly int MetalColor = Shader.PropertyToID("_MetalColor");
    private static readonly int SampleUv = Shader.PropertyToID("_SampleUV");
    private static readonly int HueWeight = Shader.PropertyToID("_HueWeight");
    private static readonly int SatWeight = Shader.PropertyToID("_SatWeight");
    private static readonly int LumWeight = Shader.PropertyToID("_LumWeight");
    private static readonly int MaskLow = Shader.PropertyToID("_MaskLow");
    private static readonly int MaskHigh = Shader.PropertyToID("_MaskHigh");
    private static readonly int Slider = Shader.PropertyToID("_Slider");
    private static readonly int BlurOverlay = Shader.PropertyToID("_BlurOverlay");
    private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
    private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private Material _blitMaterial;
    private RenderTexture _blurMap;
    private Camera _camera;

    private Texture2D _diffuseMap;
    private Texture2D _diffuseMapOriginal;
    private bool _doStuff;

    private int _imageSizeX;
    private int _imageSizeY;

    private bool _lastUseAdjustedDiffuse;
    private Texture2D _metalColorMap;
    private Material _metallicBlitMaterial;

    private Texture2D _metallicMap;

    private MetallicSettings _metallicSettings;
    private bool _mouseButtonDown;
    private bool _newTexture;
    private RenderTexture _overlayBlurMap;
    private bool _selectingColor;

    private bool _settingsInitialized;

    private float _slider = 0.5f;

    private RenderTexture _tempMap;

    private Rect _windowRect = new Rect(30, 300, 300, 530);

    [HideInInspector] public bool Busy = true;

    public MainGui MainGuiScript;

    public GameObject TestObject;

    public Material ThisMaterial;

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.MetallicSettings = _metallicSettings;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.MetallicSettings != null)
        {
            _metallicSettings = projectObject.MetallicSettings;
        }
        else
        {
            _settingsInitialized = false;
            InitializeSettings();
        }

        _metalColorMap.SetPixel(1, 1, _metallicSettings.MetalColor);
        _metalColorMap.Apply();

        _doStuff = true;
    }

    private void InitializeSettings()
    {
        if (_settingsInitialized) return;
        Debug.Log("Initializing Metallic Settings");
        _metallicSettings = new MetallicSettings();

        _metalColorMap = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        _metalColorMap.SetPixel(1, 1, _metallicSettings.MetalColor);
        _metalColorMap.Apply();

        _settingsInitialized = true;
    }

    // Use this for initialization
    private void Start()
    {
        _camera = Camera.main;
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        _blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));

        _metallicBlitMaterial = new Material(Shader.Find("Hidden/Blit_Metallic"));

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

    private void SelectColor()
    {
        if (Input.GetMouseButton(0))
        {
            _mouseButtonDown = true;

            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
                return;

            var rend = hit.transform.GetComponent<Renderer>();
            var meshCollider = hit.collider as MeshCollider;
            if (!rend || !rend.sharedMaterial || !rend.sharedMaterial.mainTexture || !meshCollider)
                return;

            var pixelUv = hit.textureCoord;

            _metallicSettings.SampleUv = pixelUv;

            _metallicSettings.MetalColor = _metallicSettings.UseAdjustedDiffuse
                ? _diffuseMap.GetPixelBilinear(pixelUv.x, pixelUv.y)
                : _diffuseMapOriginal.GetPixelBilinear(pixelUv.x, pixelUv.y);

            _metalColorMap.SetPixel(1, 1, _metallicSettings.MetalColor);
            _metalColorMap.Apply();
        }

        if (!Input.GetMouseButtonUp(0) || !_mouseButtonDown) return;

        _mouseButtonDown = false;
        _selectingColor = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_selectingColor) SelectColor();

        if (_newTexture)
        {
            InitializeTextures();
            _newTexture = false;
        }

        if (_metallicSettings.UseAdjustedDiffuse != _lastUseAdjustedDiffuse)
        {
            _lastUseAdjustedDiffuse = _metallicSettings.UseAdjustedDiffuse;
            _doStuff = true;
        }

        if (_doStuff)
        {
            StartCoroutine(ProcessBlur());
            _doStuff = false;
        }

        //thisMaterial.SetFloat ("_BlurWeight", BlurWeight);

        ThisMaterial.SetVector(MetalColor, _metallicSettings.MetalColor);
        ThisMaterial.SetVector(SampleUv,
            new Vector4(_metallicSettings.SampleUv.x, _metallicSettings.SampleUv.y, 0, 0));

        ThisMaterial.SetFloat(HueWeight, _metallicSettings.HueWeight);
        ThisMaterial.SetFloat(SatWeight, _metallicSettings.SatWeight);
        ThisMaterial.SetFloat(LumWeight, _metallicSettings.LumWeight);
        ThisMaterial.SetFloat(MaskLow, _metallicSettings.MaskLow);
        ThisMaterial.SetFloat(MaskHigh, _metallicSettings.MaskHigh);

        ThisMaterial.SetFloat(Slider, _slider);
        ThisMaterial.SetFloat(BlurOverlay, _metallicSettings.BlurOverlay);
        ThisMaterial.SetFloat(FinalContrast, _metallicSettings.FinalContrast);
        ThisMaterial.SetFloat(FinalBias, _metallicSettings.FinalBias);

        ThisMaterial.SetTexture(MainTex, _metallicSettings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal);
    }

    private void DoMyWindow(int windowId)
    {
        const int offsetX = 10;
        var offsetY = 30;

        GUI.enabled = _diffuseMap != null;
        if (GUI.Toggle(new Rect(offsetX, offsetY, 140, 30), _metallicSettings.UseAdjustedDiffuse,
            " Use Edited Diffuse"))
        {
            _metallicSettings.UseAdjustedDiffuse = true;
            _metallicSettings.UseOriginalDiffuse = false;
        }

        GUI.enabled = true;
        if (GUI.Toggle(new Rect(offsetX + 150, offsetY, 140, 30), _metallicSettings.UseOriginalDiffuse,
            " Use Original Diffuse"))
        {
            _metallicSettings.UseAdjustedDiffuse = false;
            _metallicSettings.UseOriginalDiffuse = true;
        }

        offsetY += 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Metallic Reveal Slider");
        _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);

        offsetY += 40;

        if (GUI.Button(new Rect(offsetX, offsetY + 10, 80, 30), "Pick Color")) _selectingColor = true;

        GUI.DrawTexture(new Rect(offsetX, offsetY + 50, 80, 80), _metalColorMap);

        GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
        _metallicSettings.HueWeight = GUI.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 100),
            _metallicSettings.HueWeight, 1.0f, 0.0f);

        GUI.Label(new Rect(offsetX + 130, offsetY, 250, 30), "Sat");
        _metallicSettings.SatWeight = GUI.VerticalSlider(new Rect(offsetX + 135, offsetY + 30, 10, 100),
            _metallicSettings.SatWeight, 1.0f, 0.0f);

        GUI.Label(new Rect(offsetX + 170, offsetY, 250, 30), "Lum");
        _metallicSettings.LumWeight = GUI.VerticalSlider(new Rect(offsetX + 175, offsetY + 30, 10, 100),
            _metallicSettings.LumWeight, 1.0f, 0.0f);

        GUI.Label(new Rect(offsetX + 220, offsetY, 250, 30), "Low");
        _metallicSettings.MaskLow = GUI.VerticalSlider(new Rect(offsetX + 225, offsetY + 30, 10, 100),
            _metallicSettings.MaskLow, 1.0f, 0.0f);

        GUI.Label(new Rect(offsetX + 250, offsetY, 250, 30), "High");
        _metallicSettings.MaskHigh = GUI.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 100),
            _metallicSettings.MaskHigh, 1.0f, 0.0f);

        offsetY += 150;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Blur Size", _metallicSettings.BlurSize,
            _metallicSettings.BlurSizeText,
            out _metallicSettings.BlurSize, out _metallicSettings.BlurSizeText, 0, 100)) _doStuff = true;
        offsetY += 40;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Overlay Blur Size",
            _metallicSettings.OverlayBlurSize,
            _metallicSettings.OverlayBlurSizeText, out _metallicSettings.OverlayBlurSize,
            out _metallicSettings.OverlayBlurSizeText, 10, 100)) _doStuff = true;
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "High Pass Overlay", _metallicSettings.BlurOverlay,
            _metallicSettings.BlurOverlayText,
            out _metallicSettings.BlurOverlay, out _metallicSettings.BlurOverlayText, -10.0f, 10.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _metallicSettings.FinalContrast,
            _metallicSettings.FinalContrastText,
            out _metallicSettings.FinalContrast, out _metallicSettings.FinalContrastText, -2.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _metallicSettings.FinalBias,
            _metallicSettings.FinalBiasText,
            out _metallicSettings.FinalBias, out _metallicSettings.FinalBiasText, -0.5f, 0.5f);
        offsetY += 50;
        if (GUI.Button(new Rect(offsetX + 10, offsetY, 130, 30), "Reset to Defaults"))
        {
            //_settingsInitialized = false;
            SetValues(new ProjectObject());
            //StartCoroutine(ProcessDiffuse());
        }
        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as Metallic")) StartCoroutine(ProcessMetallic());


        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = 500;

        _windowRect = GUI.Window(15, _windowRect, DoMyWindow, "Metallic From Diffuse");
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
        CleanupTexture(_overlayBlurMap);
        CleanupTexture(_tempMap);
    }

    public void InitializeTextures()
    {
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        CleanupTextures();

        _diffuseMap = MainGuiScript.DiffuseMap;
        _diffuseMapOriginal = MainGuiScript.DiffuseMapOriginal;

        if (_diffuseMap)
        {
            ThisMaterial.SetTexture(MainTex, _diffuseMap);
            _imageSizeX = _diffuseMap.width;
            _imageSizeY = _diffuseMap.height;
        }
        else
        {
            ThisMaterial.SetTexture(MainTex, _diffuseMapOriginal);
            _imageSizeX = _diffuseMapOriginal.width;
            _imageSizeY = _diffuseMapOriginal.height;

            _metallicSettings.UseAdjustedDiffuse = false;
            _metallicSettings.UseOriginalDiffuse = true;
        }

        Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

        _tempMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _overlayBlurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
    }

    public IEnumerator ProcessMetallic()
    {
        Busy = true;

        Debug.Log("Processing Height");

        _metallicBlitMaterial.SetVector("_ImageSize", new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        _metallicBlitMaterial.SetVector("_MetalColor", _metallicSettings.MetalColor);
        _metallicBlitMaterial.SetVector("_SampleUV",
            new Vector4(_metallicSettings.SampleUv.x, _metallicSettings.SampleUv.y, 0, 0));

        _metallicBlitMaterial.SetFloat("_HueWeight", _metallicSettings.HueWeight);
        _metallicBlitMaterial.SetFloat("_SatWeight", _metallicSettings.SatWeight);
        _metallicBlitMaterial.SetFloat("_LumWeight", _metallicSettings.LumWeight);

        _metallicBlitMaterial.SetFloat("_MaskLow", _metallicSettings.MaskLow);
        _metallicBlitMaterial.SetFloat("_MaskHigh", _metallicSettings.MaskHigh);

        _metallicBlitMaterial.SetFloat("_Slider", _slider);

        _metallicBlitMaterial.SetFloat("_BlurOverlay", _metallicSettings.BlurOverlay);

        _metallicBlitMaterial.SetFloat("_FinalContrast", _metallicSettings.FinalContrast);

        _metallicBlitMaterial.SetFloat("_FinalBias", _metallicSettings.FinalBias);

        _metallicBlitMaterial.SetTexture("_BlurTex", _blurMap);

        _metallicBlitMaterial.SetTexture("_OverlayBlurTex", _overlayBlurMap);

        CleanupTexture(_tempMap);
        _tempMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};

        Graphics.Blit(_metallicSettings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal, _tempMap,
            _metallicBlitMaterial, 0);

        RenderTexture.active = _tempMap;

        if (MainGuiScript.MetallicMap) Destroy(MainGuiScript.MetallicMap);

        MainGuiScript.MetallicMap = new Texture2D(_tempMap.width, _tempMap.height, TextureFormat.ARGB32, true, true);
        MainGuiScript.MetallicMap.ReadPixels(new Rect(0, 0, _tempMap.width, _tempMap.height), 0, 0);
        MainGuiScript.MetallicMap.Apply();

        yield return new WaitForSeconds(0.01f);

        CleanupTexture(_tempMap);

        Busy = false;
    }

    public IEnumerator ProcessBlur()
    {
        Busy = true;

        Debug.Log("Processing Blur");

        _blitMaterial.SetVector("_ImageSize", new Vector4(_imageSizeX, _imageSizeY, 0, 0));
        _blitMaterial.SetFloat("_BlurContrast", 1.0f);
        _blitMaterial.SetFloat("_BlurSpread", 1.0f);

        // Blur the image 1
        _blitMaterial.SetInt("_BlurSamples", _metallicSettings.BlurSize);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        if (_metallicSettings.UseAdjustedDiffuse)
        {
            if (_metallicSettings.BlurSize == 0)
                Graphics.Blit(_diffuseMap, _tempMap);
            else
                Graphics.Blit(_diffuseMap, _tempMap, _blitMaterial, 1);
        }
        else
        {
            if (_metallicSettings.BlurSize == 0)
                Graphics.Blit(_diffuseMapOriginal, _tempMap);
            else
                Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 1);
        }

        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        if (_metallicSettings.BlurSize == 0)
            Graphics.Blit(_tempMap, _blurMap);
        else
            Graphics.Blit(_tempMap, _blurMap, _blitMaterial, 1);
        ThisMaterial.SetTexture("_BlurTex", _blurMap);

        // Blur the image for overlay
        _blitMaterial.SetInt("_BlurSamples", _metallicSettings.OverlayBlurSize);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_metallicSettings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal, _tempMap, _blitMaterial,
            1);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempMap, _overlayBlurMap, _blitMaterial, 1);
        ThisMaterial.SetTexture("_OverlayBlurTex", _overlayBlurMap);

        yield return new WaitForSeconds(0.01f);

        Busy = false;
    }
}