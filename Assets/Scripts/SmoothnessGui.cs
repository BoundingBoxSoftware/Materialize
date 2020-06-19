#region

using System;
using System.Collections;
using UnityEngine;

#endregion

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

public class SmoothnessGui : MonoBehaviour
{
    private static readonly int MetalSmoothness = Shader.PropertyToID("_MetalSmoothness");
    private static readonly int IsolateSample1 = Shader.PropertyToID("_IsolateSample1");
    private static readonly int UseSample1 = Shader.PropertyToID("_UseSample1");
    private static readonly int SampleColor1 = Shader.PropertyToID("_SampleColor1");
    private static readonly int SampleUv1 = Shader.PropertyToID("_SampleUV1");
    private static readonly int HueWeight1 = Shader.PropertyToID("_HueWeight1");
    private static readonly int SatWeight1 = Shader.PropertyToID("_SatWeight1");
    private static readonly int LumWeight1 = Shader.PropertyToID("_LumWeight1");
    private static readonly int MaskLow1 = Shader.PropertyToID("_MaskLow1");
    private static readonly int MaskHigh1 = Shader.PropertyToID("_MaskHigh1");
    private static readonly int Sample1Smoothness = Shader.PropertyToID("_Sample1Smoothness");
    private static readonly int IsolateSample2 = Shader.PropertyToID("_IsolateSample2");
    private static readonly int UseSample2 = Shader.PropertyToID("_UseSample2");
    private static readonly int SampleColor2 = Shader.PropertyToID("_SampleColor2");
    private static readonly int SampleUv2 = Shader.PropertyToID("_SampleUV2");
    private static readonly int HueWeight2 = Shader.PropertyToID("_HueWeight2");
    private static readonly int SatWeight2 = Shader.PropertyToID("_SatWeight2");
    private static readonly int LumWeight2 = Shader.PropertyToID("_LumWeight2");
    private static readonly int MaskLow2 = Shader.PropertyToID("_MaskLow2");
    private static readonly int MaskHigh2 = Shader.PropertyToID("_MaskHigh2");
    private static readonly int Sample2Smoothness = Shader.PropertyToID("_Sample2Smoothness");
    private static readonly int IsolateSample3 = Shader.PropertyToID("_IsolateSample3");
    private static readonly int UseSample3 = Shader.PropertyToID("_UseSample3");
    private static readonly int SampleColor3 = Shader.PropertyToID("_SampleColor3");
    private static readonly int SampleUv3 = Shader.PropertyToID("_SampleUV3");
    private static readonly int HueWeight3 = Shader.PropertyToID("_HueWeight3");
    private static readonly int SatWeight3 = Shader.PropertyToID("_SatWeight3");
    private static readonly int LumWeight3 = Shader.PropertyToID("_LumWeight3");
    private static readonly int MaskLow3 = Shader.PropertyToID("_MaskLow3");
    private static readonly int MaskHigh3 = Shader.PropertyToID("_MaskHigh3");
    private static readonly int Sample3Smoothness = Shader.PropertyToID("_Sample3Smoothness");
    private static readonly int BaseSmoothness = Shader.PropertyToID("_BaseSmoothness");
    private static readonly int Slider = Shader.PropertyToID("_Slider");
    private static readonly int BlurOverlay = Shader.PropertyToID("_BlurOverlay");
    private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
    private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int MetallicTex = Shader.PropertyToID("_MetallicTex");
    private static readonly int Invert = Shader.PropertyToID("_Invert");
    private Material _blitMaterial;
    private RenderTexture _blurMap;
    private Camera _camera;

    private int _currentSelection;

    private Texture2D _diffuseMap;
    private Texture2D _diffuseMapOriginal;
    private bool _doStuff;

    private int _imageSizeX;
    private int _imageSizeY;
    private bool _lastUseAdjustedDiffuse;

    private Texture2D _metallicMap;
    private bool _mouseButtonDown;
    private bool _newTexture;
    private RenderTexture _overlayBlurMap;

    private Texture2D _sampleColorMap1;
    private Texture2D _sampleColorMap2;
    private Texture2D _sampleColorMap3;

    private bool _selectingColor;
    private bool _invert;

    private SmoothnessSettings _settings;

    private bool _settingsInitialized;

    private float _slider = 0.5f;
    private Texture2D _smoothnessMap;

    private RenderTexture _tempMap;

    private Rect _windowRect = new Rect(30, 300, 300, 530);

    [HideInInspector] public bool Busy;

    public Texture2D DefaultMetallicMap;

    public MainGui MainGuiScript;

    public GameObject TestObject;

    public Material ThisMaterial;
    private void Awake()
    {
        _camera = Camera.main;
       
    }

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.SmoothnessSettings = _settings;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.SmoothnessSettings != null)
        {
            _settings = projectObject.SmoothnessSettings;
        }
        else
        {
            _settingsInitialized = false;
            InitializeSettings();
        }

        _sampleColorMap1.SetPixel(1, 1, _settings.SampleColor1);
        _sampleColorMap1.Apply();

        _sampleColorMap2.SetPixel(1, 1, _settings.SampleColor2);
        _sampleColorMap2.Apply();

        _sampleColorMap3.SetPixel(1, 1, _settings.SampleColor3);
        _sampleColorMap3.Apply();

        _doStuff = true;
    }

    private void InitializeSettings()
    {
        if (_settingsInitialized) return;
        _settings = new SmoothnessSettings();

        _sampleColorMap1 = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        _sampleColorMap1.SetPixel(1, 1, _settings.SampleColor1);
        _sampleColorMap1.Apply();

        _sampleColorMap2 = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        _sampleColorMap2.SetPixel(1, 1, _settings.SampleColor2);
        _sampleColorMap2.Apply();

        _sampleColorMap3 = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        _sampleColorMap3.SetPixel(1, 1, _settings.SampleColor3);
        _sampleColorMap3.Apply();

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
        if (_selectingColor) SelectColor();

        if (_newTexture)
        {
            InitializeTextures();
            _newTexture = false;
        }

        if (_settings.UseAdjustedDiffuse != _lastUseAdjustedDiffuse)
        {
            _lastUseAdjustedDiffuse = _settings.UseAdjustedDiffuse;
            _doStuff = true;
        }

        if (_doStuff)
        {
            StartCoroutine(ProcessBlur());
            _doStuff = false;
        }

        //thisMaterial.SetFloat ("_BlurWeight", BlurWeight);

        ThisMaterial.SetFloat(MetalSmoothness, _settings.MetalSmoothness);

        ThisMaterial.SetInt(IsolateSample1, _settings.IsolateSample1 ? 1 : 0);
        ThisMaterial.SetInt(UseSample1, _settings.UseSample1 ? 1 : 0);
        ThisMaterial.SetColor(SampleColor1, _settings.SampleColor1);
        ThisMaterial.SetVector(SampleUv1, new Vector4(_settings.SampleUv1.x, _settings.SampleUv1.y, 0, 0));
        ThisMaterial.SetFloat(HueWeight1, _settings.HueWeight1);
        ThisMaterial.SetFloat(SatWeight1, _settings.SatWeight1);
        ThisMaterial.SetFloat(LumWeight1, _settings.LumWeight1);
        ThisMaterial.SetFloat(MaskLow1, _settings.MaskLow1);
        ThisMaterial.SetFloat(MaskHigh1, _settings.MaskHigh1);
        ThisMaterial.SetFloat(Sample1Smoothness, _settings.Sample1Smoothness);

        ThisMaterial.SetInt(IsolateSample2, _settings.IsolateSample2 ? 1 : 0);
        ThisMaterial.SetInt(UseSample2, _settings.UseSample2 ? 1 : 0);
        ThisMaterial.SetColor(SampleColor2, _settings.SampleColor2);
        ThisMaterial.SetVector(SampleUv2, new Vector4(_settings.SampleUv2.x, _settings.SampleUv2.y, 0, 0));
        ThisMaterial.SetFloat(HueWeight2, _settings.HueWeight2);
        ThisMaterial.SetFloat(SatWeight2, _settings.SatWeight2);
        ThisMaterial.SetFloat(LumWeight2, _settings.LumWeight2);
        ThisMaterial.SetFloat(MaskLow2, _settings.MaskLow2);
        ThisMaterial.SetFloat(MaskHigh2, _settings.MaskHigh2);
        ThisMaterial.SetFloat(Sample2Smoothness, _settings.Sample2Smoothness);

        ThisMaterial.SetInt(IsolateSample3, _settings.IsolateSample3 ? 1 : 0);
        ThisMaterial.SetInt(UseSample3, _settings.UseSample3 ? 1 : 0);
        ThisMaterial.SetColor(SampleColor3, _settings.SampleColor3);
        ThisMaterial.SetVector(SampleUv3, new Vector4(_settings.SampleUv3.x, _settings.SampleUv3.y, 0, 0));
        ThisMaterial.SetFloat(HueWeight3, _settings.HueWeight3);
        ThisMaterial.SetFloat(SatWeight3, _settings.SatWeight3);
        ThisMaterial.SetFloat(LumWeight3, _settings.LumWeight3);
        ThisMaterial.SetFloat(MaskLow3, _settings.MaskLow3);
        ThisMaterial.SetFloat(MaskHigh3, _settings.MaskHigh3);
        ThisMaterial.SetFloat(Sample3Smoothness, _settings.Sample3Smoothness);
        int invert = 0;
        if (_settings.Invert)
        {
            invert = 1;
        }
        ThisMaterial.SetInt(Invert, invert);
        ThisMaterial.SetFloat(BaseSmoothness, _settings.BaseSmoothness);

        ThisMaterial.SetFloat(Slider, _slider);
        ThisMaterial.SetFloat(BlurOverlay, _settings.BlurOverlay);
        ThisMaterial.SetFloat(FinalContrast, _settings.FinalContrast);
        ThisMaterial.SetFloat(FinalBias, _settings.FinalBias);

        ThisMaterial.SetTexture(MainTex, _settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal);
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

            var sampledColor = _settings.UseAdjustedDiffuse
                ? _diffuseMap.GetPixelBilinear(pixelUv.x, pixelUv.y)
                : _diffuseMapOriginal.GetPixelBilinear(pixelUv.x, pixelUv.y);

            switch (_currentSelection)
            {
                case 1:
                    _settings.SampleUv1 = pixelUv;
                    _settings.SampleColor1 = sampledColor;
                    _sampleColorMap1.SetPixel(1, 1, _settings.SampleColor1);
                    _sampleColorMap1.Apply();
                    break;
                case 2:
                    _settings.SampleUv2 = pixelUv;
                    _settings.SampleColor2 = sampledColor;
                    _sampleColorMap2.SetPixel(1, 1, _settings.SampleColor2);
                    _sampleColorMap2.Apply();
                    break;
                case 3:
                    _settings.SampleUv3 = pixelUv;
                    _settings.SampleColor3 = sampledColor;
                    _sampleColorMap3.SetPixel(1, 1, _settings.SampleColor3);
                    _sampleColorMap3.Apply();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        if (!Input.GetMouseButtonUp(0) || !_mouseButtonDown) return;
        _mouseButtonDown = false;
        _selectingColor = false;
        _currentSelection = 0;
    }

    private void DoMyWindow(int windowId)
    {
        const int offsetX = 10;
        var offsetY = 30;

        GUI.enabled = _diffuseMap != null;
        if (GUI.Toggle(new Rect(offsetX, offsetY, 140, 30), _settings.UseAdjustedDiffuse, " Use Edited Diffuse"))
        {
            _settings.UseAdjustedDiffuse = true;
            _settings.UseOriginalDiffuse = false;
        }

        GUI.enabled = true;
        if (GUI.Toggle(new Rect(offsetX + 150, offsetY, 140, 30), _settings.UseOriginalDiffuse,
            " Use Original Diffuse"))
        {
            _settings.UseAdjustedDiffuse = false;
            _settings.UseOriginalDiffuse = true;
        }

        offsetY += 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Smoothness Reveal Slider");
        _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Metal Smoothness", _settings.MetalSmoothness,
            _settings.MetalSmoothnessText, out _settings.MetalSmoothness, out _settings.MetalSmoothnessText, 0.0f,
            1.0f);
        offsetY += 40;

        _settings.UseSample1 =
            GUI.Toggle(new Rect(offsetX, offsetY, 150, 20), _settings.UseSample1, "Use Color Sample 1");
        if (_settings.UseSample1)
        {
            _settings.IsolateSample1 =
                GUI.Toggle(new Rect(offsetX + 180, offsetY, 150, 20), _settings.IsolateSample1, "Isolate Mask");
            if (_settings.IsolateSample1)
            {
                _settings.IsolateSample2 = false;
                _settings.IsolateSample3 = false;
            }

            offsetY += 30;

            if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
            {
                _selectingColor = true;
                _currentSelection = 1;
            }

            GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _sampleColorMap1);

            GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
            _settings.HueWeight1 = GUI.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70),
                _settings.HueWeight1, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
            _settings.SatWeight1 =
                GUI.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70), _settings.SatWeight1, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
            _settings.LumWeight1 =
                GUI.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70), _settings.LumWeight1, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
            _settings.MaskLow1 = GUI.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70), _settings.MaskLow1,
                1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
            _settings.MaskHigh1 = GUI.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70), _settings.MaskHigh1,
                1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Smooth");
            _settings.Sample1Smoothness = GUI.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70),
                _settings.Sample1Smoothness, 1.0f, 0.0f);

            offsetY += 110;
        }
        else
        {
            offsetY += 30;
            _settings.IsolateSample1 = false;
        }


        _settings.UseSample2 =
            GUI.Toggle(new Rect(offsetX, offsetY, 150, 20), _settings.UseSample2, "Use Color Sample 2");
        if (_settings.UseSample2)
        {
            _settings.IsolateSample2 =
                GUI.Toggle(new Rect(offsetX + 180, offsetY, 150, 20), _settings.IsolateSample2, "Isolate Mask");
            if (_settings.IsolateSample2)
            {
                _settings.IsolateSample1 = false;
                _settings.IsolateSample3 = false;
            }

            offsetY += 30;

            if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
            {
                _selectingColor = true;
                _currentSelection = 2;
            }

            GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _sampleColorMap2);

            GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
            _settings.HueWeight2 = GUI.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70),
                _settings.HueWeight2, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
            _settings.SatWeight2 =
                GUI.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70), _settings.SatWeight2, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
            _settings.LumWeight2 =
                GUI.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70), _settings.LumWeight2, 1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
            _settings.MaskLow2 = GUI.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70), _settings.MaskLow2,
                1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
            _settings.MaskHigh2 = GUI.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70), _settings.MaskHigh2,
                1.0f, 0.0f);

            GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Smooth");
            _settings.Sample2Smoothness = GUI.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70),
                _settings.Sample2Smoothness, 1.0f, 0.0f);

            offsetY += 110;
        }
        else
        {
            offsetY += 30;
            _settings.IsolateSample2 = false;
        }

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Base Smoothness", _settings.BaseSmoothness,
            _settings.BaseSmoothnessText, out _settings.BaseSmoothness, out _settings.BaseSmoothnessText, 0.0f, 1.0f);
        offsetY += 40;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Sample Blur Size", _settings.BlurSize,
            _settings.BlurSizeText,
            out _settings.BlurSize, out _settings.BlurSizeText, 0, 100)) _doStuff = true;
        offsetY += 40;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "High Pass Blur Size", _settings.OverlayBlurSize,
            _settings.OverlayBlurSizeText, out _settings.OverlayBlurSize, out _settings.OverlayBlurSizeText, 10, 100))
            _doStuff = true;
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "High Pass Overlay", _settings.BlurOverlay,
            _settings.BlurOverlayText,
            out _settings.BlurOverlay, out _settings.BlurOverlayText, -10.0f, 10.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _settings.FinalContrast,
            _settings.FinalContrastText,
            out _settings.FinalContrast, out _settings.FinalContrastText, -2.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _settings.FinalBias,
            _settings.FinalBiasText,
            out _settings.FinalBias, out _settings.FinalBiasText, -0.5f, 0.5f);
        offsetY += 40;

        _settings.Invert = GUI.Toggle(new Rect(offsetX, offsetY, 150, 20), _settings.Invert, "Invert Smoothness");

        offsetY += 50;

        if (GUI.Button(new Rect(offsetX + 10, offsetY, 130, 30), "Reset to Defaults"))
        {
            //_settingsInitialized = false;
            SetValues(new ProjectObject());
            //StartCoroutine(ProcessDiffuse());
        }

        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as Smoothness"))
            StartCoroutine(ProcessSmoothness());

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        if (!_settingsInitialized) return;

        _windowRect.width = 300;
        _windowRect.height = 550;

        if (_settings.UseSample1) _windowRect.height += 110;

        if (_settings.UseSample2) _windowRect.height += 110;

        _windowRect = GUI.Window(17, _windowRect, DoMyWindow, "Smoothness From Diffuse");
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
        InitializeSettings();

        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        CleanupTextures();

        _diffuseMap = MainGuiScript.DiffuseMap;
        _diffuseMapOriginal = MainGuiScript.DiffuseMapOriginal;

        _metallicMap = MainGuiScript.MetallicMap;
        ThisMaterial.SetTexture(MetallicTex, _metallicMap != null ? _metallicMap : DefaultMetallicMap);

        if (_diffuseMap)
        {
            ThisMaterial.SetTexture(MainTex, _diffuseMap);
            _imageSizeX = _diffuseMap.width;
            _imageSizeY = _diffuseMap.height;
        }
        else if (_diffuseMapOriginal)
        {
            ThisMaterial.SetTexture(MainTex, _diffuseMapOriginal);
            _imageSizeX = _diffuseMapOriginal.width;
            _imageSizeY = _diffuseMapOriginal.height;

            _settings.UseAdjustedDiffuse = false;
            _settings.UseOriginalDiffuse = true;
            _settings.UsePastedSmoothness = false;

        }
        else
        {
            _smoothnessMap = MainGuiScript.SmoothnessMap;
            ThisMaterial.SetTexture(MainTex, _smoothnessMap);
            _imageSizeX = _smoothnessMap.width;
            _imageSizeY = _smoothnessMap.height;
            _settings.UseAdjustedDiffuse = false;
            _settings.UseOriginalDiffuse = false;
            _settings.UsePastedSmoothness = true;
        }

        Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

        _tempMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _overlayBlurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
    }

    public IEnumerator ProcessSmoothness()
    {
        Busy = true;

        Debug.Log("Processing Height");

        _blitMaterial.SetVector("_ImageSize", new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        _blitMaterial.SetTexture("_MetallicTex", _metallicMap != null ? _metallicMap : DefaultMetallicMap);


        _blitMaterial.SetTexture("_BlurTex", _blurMap);

        _blitMaterial.SetTexture("_OverlayBlurTex", _overlayBlurMap);

        _blitMaterial.SetFloat("_MetalSmoothness", _settings.MetalSmoothness);

        _blitMaterial.SetInt("_UseSample1", _settings.UseSample1 ? 1 : 0);
        _blitMaterial.SetColor("_SampleColor1", _settings.SampleColor1);
        _blitMaterial.SetVector("_SampleUV1",
            new Vector4(_settings.SampleUv1.x, _settings.SampleUv1.y, 0, 0));
        _blitMaterial.SetFloat("_HueWeight1", _settings.HueWeight1);
        _blitMaterial.SetFloat("_SatWeight1", _settings.SatWeight1);
        _blitMaterial.SetFloat("_LumWeight1", _settings.LumWeight1);
        _blitMaterial.SetFloat("_MaskLow1", _settings.MaskLow1);
        _blitMaterial.SetFloat("_MaskHigh1", _settings.MaskHigh1);
        _blitMaterial.SetFloat("_Sample1Smoothness", _settings.Sample1Smoothness);

        _blitMaterial.SetInt("_UseSample2", _settings.UseSample2 ? 1 : 0);
        _blitMaterial.SetColor("_SampleColor2", _settings.SampleColor2);
        _blitMaterial.SetVector("_SampleUV2",
            new Vector4(_settings.SampleUv2.x, _settings.SampleUv2.y, 0, 0));
        _blitMaterial.SetFloat("_HueWeight2", _settings.HueWeight2);
        _blitMaterial.SetFloat("_SatWeight2", _settings.SatWeight2);
        _blitMaterial.SetFloat("_LumWeight2", _settings.LumWeight2);
        _blitMaterial.SetFloat("_MaskLow2", _settings.MaskLow2);
        _blitMaterial.SetFloat("_MaskHigh2", _settings.MaskHigh2);
        _blitMaterial.SetFloat("_Sample2Smoothness", _settings.Sample2Smoothness);

        _blitMaterial.SetInt("_UseSample3", _settings.UseSample3 ? 1 : 0);
        _blitMaterial.SetColor("_SampleColor3", _settings.SampleColor3);
        _blitMaterial.SetVector("_SampleUV3",
            new Vector4(_settings.SampleUv3.x, _settings.SampleUv3.y, 0, 0));
        _blitMaterial.SetFloat("_HueWeight3", _settings.HueWeight3);
        _blitMaterial.SetFloat("_SatWeight3", _settings.SatWeight3);
        _blitMaterial.SetFloat("_LumWeight3", _settings.LumWeight3);
        _blitMaterial.SetFloat("_MaskLow3", _settings.MaskLow3);
        _blitMaterial.SetFloat("_MaskHigh3", _settings.MaskHigh3);
        _blitMaterial.SetFloat("_Sample3Smoothness", _settings.Sample3Smoothness);
        int invert = 0;
        if (_settings.Invert)
        {
            invert = 1;
        }
        _blitMaterial.SetInt("_Invert", invert);
        _blitMaterial.SetFloat("_BaseSmoothness", _settings.BaseSmoothness);

        _blitMaterial.SetFloat("_BlurOverlay", _settings.BlurOverlay);
        _blitMaterial.SetFloat("_FinalContrast", _settings.FinalContrast);
        _blitMaterial.SetFloat("_FinalBias", _settings.FinalBias);

        CleanupTexture(_tempMap);
        _tempMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        Material m = ThisMaterial;
        m.SetFloat("_Slider", 0);
        Graphics.Blit(_settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal, _tempMap,
            m, 0);

        RenderTexture.active = _tempMap;

        if (MainGuiScript.SmoothnessMap) Destroy(MainGuiScript.SmoothnessMap);

        MainGuiScript.SmoothnessMap = new Texture2D(_tempMap.width, _tempMap.height, TextureFormat.ARGB32, true, true);
        MainGuiScript.SmoothnessMap.ReadPixels(new Rect(0, 0, _tempMap.width, _tempMap.height), 0, 0);
        MainGuiScript.SmoothnessMap.Apply();

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

        // Blur the image for selection
        _blitMaterial.SetInt("_BlurSamples", _settings.BlurSize);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        if (_settings.UseAdjustedDiffuse)
        {
            if (_settings.BlurSize == 0)
                Graphics.Blit(_diffuseMap, _tempMap);
            else
                Graphics.Blit(_diffuseMap, _tempMap, _blitMaterial, 1);
        }else if(_settings.UsePastedSmoothness){
            Graphics.Blit(_smoothnessMap, _tempMap);
        }
        else
        {
            if (_settings.BlurSize == 0)
                Graphics.Blit(_diffuseMapOriginal, _tempMap);
            else
                Graphics.Blit(_diffuseMapOriginal, _tempMap, _blitMaterial, 1);
        }

        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        if (_settings.BlurSize == 0)
            Graphics.Blit(_tempMap, _blurMap);
        else
            Graphics.Blit(_tempMap, _blurMap, _blitMaterial, 1);
        ThisMaterial.SetTexture("_BlurTex", _blurMap);

        // Blur the image for overlay
        _blitMaterial.SetInt("_BlurSamples", _settings.OverlayBlurSize);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_settings.UseAdjustedDiffuse ? _diffuseMap : _diffuseMapOriginal, _tempMap, _blitMaterial, 1);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempMap, _overlayBlurMap, _blitMaterial, 1);
        ThisMaterial.SetTexture("_OverlayBlurTex", _overlayBlurMap);

        yield return new WaitForSeconds(0.01f);

        Busy = false;
    }
}