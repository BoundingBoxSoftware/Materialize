#region

using System;
using System.Collections;
using UnityEngine;

#endregion

// ReSharper disable SpecifyACultureInStringConversionExplicitly

public class HeightFromDiffuseGui : MonoBehaviour
{
    private const float BlurScale = 1.0f;
    private static readonly int BlurScaleId = Shader.PropertyToID("_BlurScale");
    private static readonly int ImageSize = Shader.PropertyToID("_ImageSize");
    private static readonly int Isolate = Shader.PropertyToID("_Isolate");
    private static readonly int Blur0Weight = Shader.PropertyToID("_Blur0Weight");
    private static readonly int Blur1Weight = Shader.PropertyToID("_Blur1Weight");
    private static readonly int Blur2Weight = Shader.PropertyToID("_Blur2Weight");
    private static readonly int Blur3Weight = Shader.PropertyToID("_Blur3Weight");
    private static readonly int Blur4Weight = Shader.PropertyToID("_Blur4Weight");
    private static readonly int Blur5Weight = Shader.PropertyToID("_Blur5Weight");
    private static readonly int Blur6Weight = Shader.PropertyToID("_Blur6Weight");
    private static readonly int Blur0Contrast = Shader.PropertyToID("_Blur0Contrast");
    private static readonly int Blur1Contrast = Shader.PropertyToID("_Blur1Contrast");
    private static readonly int Blur2Contrast = Shader.PropertyToID("_Blur2Contrast");
    private static readonly int Blur3Contrast = Shader.PropertyToID("_Blur3Contrast");
    private static readonly int Blur4Contrast = Shader.PropertyToID("_Blur4Contrast");
    private static readonly int Blur5Contrast = Shader.PropertyToID("_Blur5Contrast");
    private static readonly int Blur6Contrast = Shader.PropertyToID("_Blur6Contrast");
    private static readonly int FinalGain = Shader.PropertyToID("_FinalGain");
    private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
    private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
    private static readonly int Slider = Shader.PropertyToID("_Slider");
    private static readonly int BlurTex0 = Shader.PropertyToID("_BlurTex0");
    private static readonly int HeightFromNormal = Shader.PropertyToID("_HeightFromNormal");
    private static readonly int BlurTex1 = Shader.PropertyToID("_BlurTex1");
    private static readonly int BlurTex2 = Shader.PropertyToID("_BlurTex2");
    private static readonly int BlurTex3 = Shader.PropertyToID("_BlurTex3");
    private static readonly int BlurTex4 = Shader.PropertyToID("_BlurTex4");
    private static readonly int BlurTex5 = Shader.PropertyToID("_BlurTex5");
    private static readonly int BlurTex6 = Shader.PropertyToID("_BlurTex6");
    private static readonly int AvgTex = Shader.PropertyToID("_AvgTex");
    private static readonly int Spread = Shader.PropertyToID("_Spread");
    private static readonly int SpreadBoost = Shader.PropertyToID("_SpreadBoost");
    private static readonly int Samples = Shader.PropertyToID("_Samples");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int BlendTex = Shader.PropertyToID("_BlendTex");
    private static readonly int IsNormal = Shader.PropertyToID("_IsNormal");
    private static readonly int BlendAmount = Shader.PropertyToID("_BlendAmount");
    private static readonly int Progress = Shader.PropertyToID("_Progress");
    private static readonly int IsolateSample1 = Shader.PropertyToID("_IsolateSample1");
    private static readonly int UseSample1 = Shader.PropertyToID("_UseSample1");
    private static readonly int SampleColor1 = Shader.PropertyToID("_SampleColor1");
    private static readonly int SampleUv1 = Shader.PropertyToID("_SampleUV1");
    private static readonly int HueWeight1 = Shader.PropertyToID("_HueWeight1");
    private static readonly int SatWeight1 = Shader.PropertyToID("_SatWeight1");
    private static readonly int LumWeight1 = Shader.PropertyToID("_LumWeight1");
    private static readonly int MaskLow1 = Shader.PropertyToID("_MaskLow1");
    private static readonly int MaskHigh1 = Shader.PropertyToID("_MaskHigh1");
    private static readonly int Sample1Height = Shader.PropertyToID("_Sample1Height");
    private static readonly int IsolateSample2 = Shader.PropertyToID("_IsolateSample2");
    private static readonly int UseSample2 = Shader.PropertyToID("_UseSample2");
    private static readonly int SampleColor2 = Shader.PropertyToID("_SampleColor2");
    private static readonly int SampleUv2 = Shader.PropertyToID("_SampleUV2");
    private static readonly int HueWeight2 = Shader.PropertyToID("_HueWeight2");
    private static readonly int SatWeight2 = Shader.PropertyToID("_SatWeight2");
    private static readonly int LumWeight2 = Shader.PropertyToID("_LumWeight2");
    private static readonly int MaskLow2 = Shader.PropertyToID("_MaskLow2");
    private static readonly int MaskHigh2 = Shader.PropertyToID("_MaskHigh2");
    private static readonly int Sample2Height = Shader.PropertyToID("_Sample2Height");
    private static readonly int SampleBlend = Shader.PropertyToID("_SampleBlend");
    private static readonly int BlurContrast = Shader.PropertyToID("_BlurContrast");
    private static readonly int BlurSamples = Shader.PropertyToID("_BlurSamples");
    private static readonly int BlurSpread = Shader.PropertyToID("_BlurSpread");
    private static readonly int BlurDirection = Shader.PropertyToID("_BlurDirection");
    private RenderTexture _avgMap;

    private RenderTexture _avgTempMap;
    private Material _blitMaterial;
    private Material _blitMaterialNormal;
    private Material _blitMaterialSample;
    private RenderTexture _blurMap0;
    private RenderTexture _blurMap1;
    private RenderTexture _blurMap2;
    private RenderTexture _blurMap3;
    private RenderTexture _blurMap4;
    private RenderTexture _blurMap5;
    private RenderTexture _blurMap6;
    private Camera _camera;

    private int _currentSelection;
    private bool _doStuff;

    private HeightFromDiffuseSettings _heightFromDiffuseSettings;
    private int _imageSizeX = 1024;
    private int _imageSizeY = 1024;

    private float _lastBlur0Contrast = 1.0f;

    private bool _lastUseDiffuse;
    private bool _lastUseNormal;
    private bool _lastUseOriginalDiffuse;
    private bool _mouseButtonDown;
    private bool _newTexture;

    private Texture2D _sampleColorMap1;
    private Texture2D _sampleColorMap2;
    private bool _selectingColor;
    private bool _settingsInitialized;

    private float _slider = 0.5f;

    private RenderTexture _tempBlurMap;
    private RenderTexture _tempHeightMap;

    private Rect _windowRect = new Rect(30, 300, 300, 480);

    [HideInInspector] public bool Busy;

    public MainGui MainGuiScript;

    public GameObject TestObject;

    public Material ThisMaterial;

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.HeightFromDiffuseSettings = _heightFromDiffuseSettings;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.HeightFromDiffuseSettings != null)
        {
            _heightFromDiffuseSettings = projectObject.HeightFromDiffuseSettings;
        }
        else
        {
            _settingsInitialized = false;
            InitializeSettings();
        }

        _sampleColorMap1.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor1);
        _sampleColorMap1.Apply();

        _sampleColorMap2.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor2);
        _sampleColorMap2.Apply();

        _doStuff = true;
    }

    private void InitializeSettings()
    {
        if (_settingsInitialized) return;
//        Debug.Log("Initializing Height From Diffuse Settings");

        _heightFromDiffuseSettings = new HeightFromDiffuseSettings();

        if (_sampleColorMap1) Destroy(_sampleColorMap1);
        _sampleColorMap1 = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        _sampleColorMap1.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor1);
        _sampleColorMap1.Apply();

        if (_sampleColorMap2) Destroy(_sampleColorMap2);
        _sampleColorMap2 = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        _sampleColorMap2.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor2);
        _sampleColorMap2.Apply();

        _settingsInitialized = true;
    }

    // Use this for initialization
    private void Start()
    {
        _camera = Camera.main;
        Resources.UnloadUnusedAssets();

        //MainGuiScript = MainGui.instance;

        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;
        _blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));
        _blitMaterialSample = new Material(Shader.Find("Hidden/Blit_Sample"));
        _blitMaterialNormal = new Material(Shader.Find("Hidden/Blit_Height_From_Normal"));

        InitializeSettings();

        if (_newTexture)
        {
            InitializeTextures();
            _newTexture = false;
        }

        FixUseMaps();

        _lastUseDiffuse = _heightFromDiffuseSettings.UseAdjustedDiffuse;
        _lastUseOriginalDiffuse = _heightFromDiffuseSettings.UseOriginalDiffuse;
        _lastUseNormal = _heightFromDiffuseSettings.UseNormal;
        _lastBlur0Contrast = _heightFromDiffuseSettings.Blur0Contrast;

        SetMaterialValues();
    }

    private void FixUseMaps()
    {
        if (MainGuiScript.DiffuseMapOriginal == null && _heightFromDiffuseSettings.UseOriginalDiffuse)
        {
            _heightFromDiffuseSettings.UseAdjustedDiffuse = true;
            _heightFromDiffuseSettings.UseOriginalDiffuse = false;
            _heightFromDiffuseSettings.UseNormal = false;
        }

        if (MainGuiScript.DiffuseMap == null && _heightFromDiffuseSettings.UseAdjustedDiffuse)
        {
            _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
            _heightFromDiffuseSettings.UseOriginalDiffuse = true;
            _heightFromDiffuseSettings.UseNormal = false;
        }

        if (MainGuiScript.NormalMap == null && _heightFromDiffuseSettings.UseNormal)
        {
            _heightFromDiffuseSettings.UseAdjustedDiffuse = true;
            _heightFromDiffuseSettings.UseOriginalDiffuse = false;
            _heightFromDiffuseSettings.UseNormal = false;
        }

        if ((MainGuiScript.DiffuseMapOriginal == null) & (MainGuiScript.NormalMap == null))
        {
            _heightFromDiffuseSettings.UseAdjustedDiffuse = true;
            _heightFromDiffuseSettings.UseOriginalDiffuse = false;
            _heightFromDiffuseSettings.UseNormal = false;
        }

        if (MainGuiScript.DiffuseMap == null && MainGuiScript.NormalMap == null)
        {
            _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
            _heightFromDiffuseSettings.UseOriginalDiffuse = true;
            _heightFromDiffuseSettings.UseNormal = false;
        }

        if (MainGuiScript.DiffuseMap != null || MainGuiScript.DiffuseMapOriginal != null) return;
        _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
        _heightFromDiffuseSettings.UseOriginalDiffuse = false;
        _heightFromDiffuseSettings.UseNormal = true;
    }

    public void DoStuff()
    {
        _doStuff = true;
    }

    public void NewTexture()
    {
        _newTexture = true;
    }

    private void SetMaterialValues()
    {
        ThisMaterial.SetFloat(BlurScaleId, BlurScale);
        ThisMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));
    }

    private void SetWeightEqDefault()
    {
        _heightFromDiffuseSettings.Blur0Weight = 0.15f;
        _heightFromDiffuseSettings.Blur1Weight = 0.19f;
        _heightFromDiffuseSettings.Blur2Weight = 0.3f;
        _heightFromDiffuseSettings.Blur3Weight = 0.5f;
        _heightFromDiffuseSettings.Blur4Weight = 0.7f;
        _heightFromDiffuseSettings.Blur5Weight = 0.9f;
        _heightFromDiffuseSettings.Blur6Weight = 1.0f;
        _doStuff = true;
    }

    private void SetWeightEqDetail()
    {
        _heightFromDiffuseSettings.Blur0Weight = 0.7f;
        _heightFromDiffuseSettings.Blur1Weight = 0.4f;
        _heightFromDiffuseSettings.Blur2Weight = 0.3f;
        _heightFromDiffuseSettings.Blur3Weight = 0.5f;
        _heightFromDiffuseSettings.Blur4Weight = 0.8f;
        _heightFromDiffuseSettings.Blur5Weight = 0.9f;
        _heightFromDiffuseSettings.Blur6Weight = 0.7f;
        _doStuff = true;
    }

    private void SetWeightEqDisplace()
    {
        _heightFromDiffuseSettings.Blur0Weight = 0.02f;
        _heightFromDiffuseSettings.Blur1Weight = 0.03f;
        _heightFromDiffuseSettings.Blur2Weight = 0.1f;
        _heightFromDiffuseSettings.Blur3Weight = 0.35f;
        _heightFromDiffuseSettings.Blur4Weight = 0.7f;
        _heightFromDiffuseSettings.Blur5Weight = 0.9f;
        _heightFromDiffuseSettings.Blur6Weight = 1.0f;
        _doStuff = true;
    }

    private void SetContrastEqDefault()
    {
        _heightFromDiffuseSettings.Blur0Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur1Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur2Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur3Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur4Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur5Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur6Contrast = 1.0f;
        _doStuff = true;
    }

    private void SetContrastEqCrackedMud()
    {
        _heightFromDiffuseSettings.Blur0Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur1Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur2Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur3Contrast = 1.0f;
        _heightFromDiffuseSettings.Blur4Contrast = -0.2f;
        _heightFromDiffuseSettings.Blur5Contrast = -2.0f;
        _heightFromDiffuseSettings.Blur6Contrast = -4.0f;
        _doStuff = true;
    }

    private void SetContrastEqFunky()
    {
        _heightFromDiffuseSettings.Blur0Contrast = -3.0f;
        _heightFromDiffuseSettings.Blur1Contrast = -1.2f;
        _heightFromDiffuseSettings.Blur2Contrast = 0.30f;
        _heightFromDiffuseSettings.Blur3Contrast = 1.3f;
        _heightFromDiffuseSettings.Blur4Contrast = 2.0f;
        _heightFromDiffuseSettings.Blur5Contrast = 2.5f;
        _heightFromDiffuseSettings.Blur6Contrast = 2.0f;
        _doStuff = true;
    }

    private void SelectColor()
    {
        if (Input.GetMouseButton(0))
        {
            _mouseButtonDown = true;
            if (!_camera) return;

            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
                return;

            var rend = hit.transform.GetComponent<Renderer>();
            var meshCollider = hit.collider as MeshCollider;
            if (!rend || !rend.sharedMaterial || !rend.sharedMaterial.mainTexture ||
                !meshCollider)
                return;

            var pixelUv = hit.textureCoord;

            var useAdjusted = _heightFromDiffuseSettings.UseAdjustedDiffuse;
            var sampledColor = useAdjusted
                ? MainGuiScript.DiffuseMap.GetPixelBilinear(pixelUv.x, pixelUv.y)
                : MainGuiScript.DiffuseMapOriginal.GetPixelBilinear(pixelUv.x, pixelUv.y);

            switch (_currentSelection)
            {
                case 1:
                    _heightFromDiffuseSettings.SampleUv1 = pixelUv;
                    _heightFromDiffuseSettings.SampleColor1 = sampledColor;
                    _sampleColorMap1.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor1);
                    _sampleColorMap1.Apply();
                    break;
                case 2:
                    _heightFromDiffuseSettings.SampleUv2 = pixelUv;
                    _heightFromDiffuseSettings.SampleColor2 = sampledColor;
                    _sampleColorMap2.SetPixel(1, 1, _heightFromDiffuseSettings.SampleColor2);
                    _sampleColorMap2.Apply();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _doStuff = true;
        }

        if (!Input.GetMouseButtonUp(0) || !_mouseButtonDown) return;

        _mouseButtonDown = false;
        _selectingColor = false;
        _currentSelection = 0;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_selectingColor) SelectColor();

        if (_heightFromDiffuseSettings.UseAdjustedDiffuse != _lastUseDiffuse)
        {
            _lastUseDiffuse = _heightFromDiffuseSettings.UseAdjustedDiffuse;
            _doStuff = true;
        }

        if (_heightFromDiffuseSettings.UseOriginalDiffuse != _lastUseOriginalDiffuse)
        {
            _lastUseOriginalDiffuse = _heightFromDiffuseSettings.UseOriginalDiffuse;
            _doStuff = true;
        }

        if (_heightFromDiffuseSettings.UseNormal != _lastUseNormal)
        {
            _lastUseNormal = _heightFromDiffuseSettings.UseNormal;
            _doStuff = true;
        }

        if (Math.Abs(_heightFromDiffuseSettings.Blur0Contrast - _lastBlur0Contrast) > 0.001f)
        {
            _lastBlur0Contrast = _heightFromDiffuseSettings.Blur0Contrast;
            _doStuff = true;
        }

        if (_newTexture)
        {
            InitializeTextures();
            _newTexture = false;
        }

        if (_doStuff)
        {
            if (_heightFromDiffuseSettings.UseNormal)
            {
                StopAllCoroutines();
                StartCoroutine(ProcessNormal());
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(ProcessDiffuse());
            }

            _doStuff = false;
        }

        if (_heightFromDiffuseSettings.IsolateSample1 || _heightFromDiffuseSettings.IsolateSample2)
            ThisMaterial.SetInt(Isolate, 1);
        else
            ThisMaterial.SetInt(Isolate, 0);

        ThisMaterial.SetFloat(Blur0Weight, _heightFromDiffuseSettings.Blur0Weight);
        ThisMaterial.SetFloat(Blur1Weight, _heightFromDiffuseSettings.Blur1Weight);
        ThisMaterial.SetFloat(Blur2Weight, _heightFromDiffuseSettings.Blur2Weight);
        ThisMaterial.SetFloat(Blur3Weight, _heightFromDiffuseSettings.Blur3Weight);
        ThisMaterial.SetFloat(Blur4Weight, _heightFromDiffuseSettings.Blur4Weight);
        ThisMaterial.SetFloat(Blur5Weight, _heightFromDiffuseSettings.Blur5Weight);
        ThisMaterial.SetFloat(Blur6Weight, _heightFromDiffuseSettings.Blur6Weight);

        ThisMaterial.SetFloat(Blur0Contrast, _heightFromDiffuseSettings.Blur0Contrast);
        ThisMaterial.SetFloat(Blur1Contrast, _heightFromDiffuseSettings.Blur1Contrast);
        ThisMaterial.SetFloat(Blur2Contrast, _heightFromDiffuseSettings.Blur2Contrast);
        ThisMaterial.SetFloat(Blur3Contrast, _heightFromDiffuseSettings.Blur3Contrast);
        ThisMaterial.SetFloat(Blur4Contrast, _heightFromDiffuseSettings.Blur4Contrast);
        ThisMaterial.SetFloat(Blur5Contrast, _heightFromDiffuseSettings.Blur5Contrast);
        ThisMaterial.SetFloat(Blur6Contrast, _heightFromDiffuseSettings.Blur6Contrast);

        var realGain = _heightFromDiffuseSettings.FinalGain;
        if (realGain < 0.0f)
            realGain = Mathf.Abs(1.0f / (realGain - 1.0f));
        else
            realGain = realGain + 1.0f;

        ThisMaterial.SetFloat(FinalGain, realGain);
        ThisMaterial.SetFloat(FinalContrast, _heightFromDiffuseSettings.FinalContrast);
        ThisMaterial.SetFloat(FinalBias, _heightFromDiffuseSettings.FinalBias);

        ThisMaterial.SetFloat(Slider, _slider);
    }

    private void DoMyWindow(int windowId)
    {
        var offsetX = 10;
        var offsetY = 30;

        GUI.enabled = MainGuiScript.DiffuseMap != null;
        _heightFromDiffuseSettings.UseAdjustedDiffuse = GUI.Toggle(new Rect(offsetX, offsetY, 80, 30),
            _heightFromDiffuseSettings.UseAdjustedDiffuse, " Diffuse");
        if (_heightFromDiffuseSettings.UseAdjustedDiffuse)
        {
            _heightFromDiffuseSettings.UseOriginalDiffuse = false;
            _heightFromDiffuseSettings.UseNormal = false;
        }
        else if (!_heightFromDiffuseSettings.UseOriginalDiffuse && !_heightFromDiffuseSettings.UseNormal)
        {
            _heightFromDiffuseSettings.UseAdjustedDiffuse = true;
        }

        GUI.enabled = MainGuiScript.DiffuseMapOriginal != null;
        _heightFromDiffuseSettings.UseOriginalDiffuse = GUI.Toggle(new Rect(offsetX + 80, offsetY, 120, 30),
            _heightFromDiffuseSettings.UseOriginalDiffuse,
            "Original Diffuse");
        if (_heightFromDiffuseSettings.UseOriginalDiffuse)
        {
            _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
            _heightFromDiffuseSettings.UseNormal = false;
        }
        else if (!_heightFromDiffuseSettings.UseAdjustedDiffuse && !_heightFromDiffuseSettings.UseNormal)
        {
            _heightFromDiffuseSettings.UseOriginalDiffuse = true;
        }

        GUI.enabled = MainGuiScript.NormalMap;
        _heightFromDiffuseSettings.UseNormal = GUI.Toggle(new Rect(offsetX + 210, offsetY, 80, 30),
            _heightFromDiffuseSettings.UseNormal, " Normal");
        if (_heightFromDiffuseSettings.UseNormal)
        {
            _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
            _heightFromDiffuseSettings.UseOriginalDiffuse = false;
        }
        else if (!_heightFromDiffuseSettings.UseAdjustedDiffuse && !_heightFromDiffuseSettings.UseOriginalDiffuse)
        {
            _heightFromDiffuseSettings.UseNormal = true;
        }

        GUI.enabled = true;
        offsetY += 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Height Reveal Slider");
        _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);
        offsetY += 40;

        if (_heightFromDiffuseSettings.UseNormal)
        {
            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 10), "Sample Spread",
                _heightFromDiffuseSettings.Spread, _heightFromDiffuseSettings.SpreadText,
                out _heightFromDiffuseSettings.Spread, out _heightFromDiffuseSettings.SpreadText, 10.0f, 200.0f))
                _doStuff = true;

            offsetY += 40;

            if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 10), "Sample Spread Boost",
                _heightFromDiffuseSettings.SpreadBoost,
                _heightFromDiffuseSettings.SpreadBoostText, out _heightFromDiffuseSettings.SpreadBoost,
                out _heightFromDiffuseSettings.SpreadBoostText, 1.0f, 5.0f)) _doStuff = true;

            offsetY += 40;
        }
        else
        {
            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Weight Equalizer");
            GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default")) SetWeightEqDefault();
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Details")) SetWeightEqDetail();
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Displace")) SetWeightEqDisplace();

            offsetY += 30;
            offsetX += 10;
            _heightFromDiffuseSettings.Blur0Weight =
                GUI.VerticalSlider(new Rect(offsetX + 180, offsetY, 10, 80), _heightFromDiffuseSettings.Blur0Weight,
                    1.0f, 0.0f);
            _heightFromDiffuseSettings.Blur1Weight =
                GUI.VerticalSlider(new Rect(offsetX + 150, offsetY, 10, 80), _heightFromDiffuseSettings.Blur1Weight,
                    1.0f, 0.0f);
            _heightFromDiffuseSettings.Blur2Weight =
                GUI.VerticalSlider(new Rect(offsetX + 120, offsetY, 10, 80), _heightFromDiffuseSettings.Blur2Weight,
                    1.0f, 0.0f);
            _heightFromDiffuseSettings.Blur3Weight =
                GUI.VerticalSlider(new Rect(offsetX + 90, offsetY, 10, 80), _heightFromDiffuseSettings.Blur3Weight,
                    1.0f, 0.0f);
            _heightFromDiffuseSettings.Blur4Weight =
                GUI.VerticalSlider(new Rect(offsetX + 60, offsetY, 10, 80), _heightFromDiffuseSettings.Blur4Weight,
                    1.0f, 0.0f);
            _heightFromDiffuseSettings.Blur5Weight =
                GUI.VerticalSlider(new Rect(offsetX + 30, offsetY, 10, 80), _heightFromDiffuseSettings.Blur5Weight,
                    1.0f, 0.0f);
            _heightFromDiffuseSettings.Blur6Weight = GUI.VerticalSlider(new Rect(offsetX + 0, offsetY, 10, 80),
                _heightFromDiffuseSettings.Blur6Weight, 1.0f, 0.0f);
            offsetX -= 10;
            offsetY += 100;


            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Contrast Equalizer");
            GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default")) SetContrastEqDefault();
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Cracks")) SetContrastEqCrackedMud();
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Funky")) SetContrastEqFunky();
            offsetY += 30;
            offsetX += 10;
            _heightFromDiffuseSettings.Blur0Contrast =
                GUI.VerticalSlider(new Rect(offsetX + 180, offsetY, 10, 80), _heightFromDiffuseSettings.Blur0Contrast,
                    5.0f, -5.0f);
            _heightFromDiffuseSettings.Blur1Contrast =
                GUI.VerticalSlider(new Rect(offsetX + 150, offsetY, 10, 80), _heightFromDiffuseSettings.Blur1Contrast,
                    5.0f, -5.0f);
            _heightFromDiffuseSettings.Blur2Contrast =
                GUI.VerticalSlider(new Rect(offsetX + 120, offsetY, 10, 80), _heightFromDiffuseSettings.Blur2Contrast,
                    5.0f, -5.0f);
            _heightFromDiffuseSettings.Blur3Contrast =
                GUI.VerticalSlider(new Rect(offsetX + 90, offsetY, 10, 80), _heightFromDiffuseSettings.Blur3Contrast,
                    5.0f, -5.0f);
            _heightFromDiffuseSettings.Blur4Contrast =
                GUI.VerticalSlider(new Rect(offsetX + 60, offsetY, 10, 80), _heightFromDiffuseSettings.Blur4Contrast,
                    5.0f, -5.0f);
            _heightFromDiffuseSettings.Blur5Contrast =
                GUI.VerticalSlider(new Rect(offsetX + 30, offsetY, 10, 80), _heightFromDiffuseSettings.Blur5Contrast,
                    5.0f, -5.0f);
            _heightFromDiffuseSettings.Blur6Contrast =
                GUI.VerticalSlider(new Rect(offsetX + 0, offsetY, 10, 80), _heightFromDiffuseSettings.Blur6Contrast,
                    5.0f, -5.0f);
            offsetX -= 10;
            GUI.Label(new Rect(offsetX + 210, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 180, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 150, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 120, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 90, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 60, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 30, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 0, offsetY + 21, 30, 30), "-");
            offsetY += 100;


            _doStuff = GuiHelper.Toggle(new Rect(offsetX, offsetY, 150, 20), _heightFromDiffuseSettings.UseSample1,
                out _heightFromDiffuseSettings.UseSample1,
                "Use Color Sample 1", _doStuff);
            if (_heightFromDiffuseSettings.UseSample1)
            {
                _doStuff = GuiHelper.Toggle(new Rect(offsetX + 180, offsetY, 150, 20),
                    _heightFromDiffuseSettings.IsolateSample1,
                    out _heightFromDiffuseSettings.IsolateSample1, "Isolate Mask", _doStuff);
                if (_heightFromDiffuseSettings.IsolateSample1) _heightFromDiffuseSettings.IsolateSample2 = false;
                offsetY += 30;

                if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
                {
                    _selectingColor = true;
                    _currentSelection = 1;
                }

                GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _sampleColorMap1);

                GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.HueWeight1,
                    out _heightFromDiffuseSettings.HueWeight1, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.SatWeight1,
                    out _heightFromDiffuseSettings.SatWeight1, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.LumWeight1,
                    out _heightFromDiffuseSettings.LumWeight1, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.MaskLow1,
                    out _heightFromDiffuseSettings.MaskLow1, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.MaskHigh1,
                    out _heightFromDiffuseSettings.MaskHigh1, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Height");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.Sample1Height,
                    out _heightFromDiffuseSettings.Sample1Height, 1.0f, 0.0f, _doStuff);

                offsetY += 110;
            }
            else
            {
                offsetY += 30;
                _heightFromDiffuseSettings.IsolateSample1 = false;
            }


            _doStuff = GuiHelper.Toggle(new Rect(offsetX, offsetY, 150, 20), _heightFromDiffuseSettings.UseSample2,
                out _heightFromDiffuseSettings.UseSample2,
                "Use Color Sample 2", _doStuff);
            if (_heightFromDiffuseSettings.UseSample2)
            {
                _doStuff = GuiHelper.Toggle(new Rect(offsetX + 180, offsetY, 150, 20),
                    _heightFromDiffuseSettings.IsolateSample2,
                    out _heightFromDiffuseSettings.IsolateSample2, "Isolate Mask", _doStuff);
                if (_heightFromDiffuseSettings.IsolateSample2) _heightFromDiffuseSettings.IsolateSample1 = false;
                offsetY += 30;

                if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
                {
                    _selectingColor = true;
                    _currentSelection = 2;
                }

                GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _sampleColorMap2);

                GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.HueWeight2,
                    out _heightFromDiffuseSettings.HueWeight2, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.SatWeight2,
                    out _heightFromDiffuseSettings.SatWeight2, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.LumWeight2,
                    out _heightFromDiffuseSettings.LumWeight2, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.MaskLow2,
                    out _heightFromDiffuseSettings.MaskLow2, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.MaskHigh2,
                    out _heightFromDiffuseSettings.MaskHigh2, 1.0f, 0.0f, _doStuff);

                GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Height");
                _doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70),
                    _heightFromDiffuseSettings.Sample2Height,
                    out _heightFromDiffuseSettings.Sample2Height, 1.0f, 0.0f, _doStuff);

                offsetY += 110;
            }
            else
            {
                offsetY += 30;
                _heightFromDiffuseSettings.IsolateSample2 = false;
            }

            if (_heightFromDiffuseSettings.UseSample1 || _heightFromDiffuseSettings.UseSample2)
            {
                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Sample Blend",
                    _heightFromDiffuseSettings.SampleBlend,
                    _heightFromDiffuseSettings.SampleBlendText, out _heightFromDiffuseSettings.SampleBlend,
                    out _heightFromDiffuseSettings.SampleBlendText, 0.0f, 1.0f)) _doStuff = true;
                offsetY += 40;
            }
        }


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Gain", _heightFromDiffuseSettings.FinalGain,
            _heightFromDiffuseSettings.FinalGainText,
            out _heightFromDiffuseSettings.FinalGain, out _heightFromDiffuseSettings.FinalGainText, -0.5f, 0.5f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast",
            _heightFromDiffuseSettings.FinalContrast,
            _heightFromDiffuseSettings.FinalContrastText, out _heightFromDiffuseSettings.FinalContrast,
            out _heightFromDiffuseSettings.FinalContrastText, -10.0f, 10.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", _heightFromDiffuseSettings.FinalBias,
            _heightFromDiffuseSettings.FinalBiasText,
            out _heightFromDiffuseSettings.FinalBias, out _heightFromDiffuseSettings.FinalBiasText, -1.0f, 1.0f);
        offsetY += 50;

        GUI.enabled = !Busy;
        if (GUI.Button(new Rect(offsetX + 10, offsetY, 130, 30), "Reset to Defaults"))
        {
            //_settingsInitialized = false;
            SetValues(new ProjectObject());
            _heightFromDiffuseSettings.UseOriginalDiffuse = true;
            if (_heightFromDiffuseSettings.UseOriginalDiffuse)
            {
                _heightFromDiffuseSettings.UseAdjustedDiffuse = false;
                _heightFromDiffuseSettings.UseNormal = false;
            }
            else if (!_heightFromDiffuseSettings.UseAdjustedDiffuse && !_heightFromDiffuseSettings.UseNormal)
            {
                _heightFromDiffuseSettings.UseOriginalDiffuse = true;
            }
            //StartCoroutine(ProcessDiffuse());
        }
        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as Height Map")) StartCoroutine(ProcessHeight());
        GUI.enabled = true;

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = 590;

        if (_heightFromDiffuseSettings.UseSample1 && !_heightFromDiffuseSettings.UseNormal) _windowRect.height += 110;

        if (_heightFromDiffuseSettings.UseSample2 && !_heightFromDiffuseSettings.UseNormal) _windowRect.height += 110;

        if ((_heightFromDiffuseSettings.UseSample1 || _heightFromDiffuseSettings.UseSample2) &&
            !_heightFromDiffuseSettings.UseNormal) _windowRect.height += 40;

        _windowRect = GUI.Window(13, _windowRect, DoMyWindow, "Height From Diffuse");
    }

    public void InitializeTextures()
    {
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        CleanupTextures();

        FixUseMaps();

        if (_heightFromDiffuseSettings.UseAdjustedDiffuse)
        {
            _imageSizeX = MainGuiScript.DiffuseMap.width;
            _imageSizeY = MainGuiScript.DiffuseMap.height;
        }
        else if (_heightFromDiffuseSettings.UseOriginalDiffuse)
        {
            _imageSizeX = MainGuiScript.DiffuseMapOriginal.width;
            _imageSizeY = MainGuiScript.DiffuseMapOriginal.height;
        }
        else if (_heightFromDiffuseSettings.UseNormal)
        {
            _imageSizeX = MainGuiScript.NormalMap.width;
            _imageSizeY = MainGuiScript.NormalMap.height;
        }


//        Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

        _tempBlurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RFloat,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap0 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RFloat,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap1 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RFloat,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap2 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RFloat,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap3 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RFloat,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap4 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RFloat,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap5 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RFloat,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap6 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RFloat,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};

        _avgMap = new RenderTexture(256, 256, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
        {
            wrapMode = TextureWrapMode.Repeat
        };

        _avgTempMap = new RenderTexture(256, 256, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
        {
            wrapMode = TextureWrapMode.Repeat
        };

        SetMaterialValues();
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
        CleanupTexture(_tempHeightMap);
        CleanupTexture(_avgMap);
        CleanupTexture(_avgTempMap);
    }
    public void StartProcessHeight()
    {
        StartCoroutine(ProcessHeight());
    }
    public IEnumerator ProcessHeight()
    {
        Busy = true;
        _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        CleanupTexture(_tempHeightMap);
        _tempHeightMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};

        _blitMaterial.SetFloat(FinalContrast, _heightFromDiffuseSettings.FinalContrast);
        _blitMaterial.SetFloat(FinalBias, _heightFromDiffuseSettings.FinalBias);

        var realGain = _heightFromDiffuseSettings.FinalGain;
        if (realGain < 0.0f)
            realGain = Mathf.Abs(1.0f / (realGain - 1.0f));
        else
            realGain = realGain + 1.0f;
        _blitMaterial.SetFloat(FinalGain, realGain);

        if (_heightFromDiffuseSettings.UseNormal)
        {
            _blitMaterial.SetTexture(BlurTex0, _blurMap0);
            _blitMaterial.SetFloat(HeightFromNormal, 1.0f);
            // Save low fidelity for texture 2d
            Graphics.Blit(_blurMap0, _tempHeightMap, _blitMaterial, 2);
        }
        else
        {
            _blitMaterial.SetFloat(HeightFromNormal, 0.0f);

            _blitMaterial.SetFloat(Blur0Weight, _heightFromDiffuseSettings.Blur0Weight);
            _blitMaterial.SetFloat(Blur1Weight, _heightFromDiffuseSettings.Blur1Weight);
            _blitMaterial.SetFloat(Blur2Weight, _heightFromDiffuseSettings.Blur2Weight);
            _blitMaterial.SetFloat(Blur3Weight, _heightFromDiffuseSettings.Blur3Weight);
            _blitMaterial.SetFloat(Blur4Weight, _heightFromDiffuseSettings.Blur4Weight);
            _blitMaterial.SetFloat(Blur5Weight, _heightFromDiffuseSettings.Blur5Weight);
            _blitMaterial.SetFloat(Blur6Weight, _heightFromDiffuseSettings.Blur6Weight);

            _blitMaterial.SetFloat(Blur0Contrast, _heightFromDiffuseSettings.Blur0Contrast);
            _blitMaterial.SetFloat(Blur1Contrast, _heightFromDiffuseSettings.Blur1Contrast);
            _blitMaterial.SetFloat(Blur2Contrast, _heightFromDiffuseSettings.Blur2Contrast);
            _blitMaterial.SetFloat(Blur3Contrast, _heightFromDiffuseSettings.Blur3Contrast);
            _blitMaterial.SetFloat(Blur4Contrast, _heightFromDiffuseSettings.Blur4Contrast);
            _blitMaterial.SetFloat(Blur5Contrast, _heightFromDiffuseSettings.Blur5Contrast);
            _blitMaterial.SetFloat(Blur6Contrast, _heightFromDiffuseSettings.Blur6Contrast);

            _blitMaterial.SetTexture(BlurTex0, _blurMap0);
            _blitMaterial.SetTexture(BlurTex1, _blurMap1);
            _blitMaterial.SetTexture(BlurTex2, _blurMap2);
            _blitMaterial.SetTexture(BlurTex3, _blurMap3);
            _blitMaterial.SetTexture(BlurTex4, _blurMap4);
            _blitMaterial.SetTexture(BlurTex5, _blurMap5);
            _blitMaterial.SetTexture(BlurTex6, _blurMap6);

            _blitMaterial.SetTexture(AvgTex, _avgMap);

            // Save low fidelity for texture 2d
            Graphics.Blit(_blurMap0, _tempHeightMap, _blitMaterial, 2);
        }


        if (MainGuiScript.HeightMap) Destroy(MainGuiScript.HeightMap);

        RenderTexture.active = _tempHeightMap;
        MainGuiScript.HeightMap =
            new Texture2D(_tempHeightMap.width, _tempHeightMap.height, TextureFormat.ARGB32, true, true);
        MainGuiScript.HeightMap.ReadPixels(new Rect(0, 0, _tempHeightMap.width, _tempHeightMap.height), 0, 0);
        MainGuiScript.HeightMap.Apply();
        RenderTexture.active = null;

        // Save high fidelity for normal making
        if (MainGuiScript.HdHeightMap != null)
        {
            MainGuiScript.HdHeightMap.Release();
            MainGuiScript.HdHeightMap = null;
        }

        MainGuiScript.HdHeightMap = new RenderTexture(_tempHeightMap.width, _tempHeightMap.height, 0,
            RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        Graphics.Blit(_blurMap0, MainGuiScript.HdHeightMap, _blitMaterial, 2);

        CleanupTexture(_tempHeightMap);

        yield return new WaitForSeconds(0.1f);

        Busy = false;
    }

    private IEnumerator ProcessNormal()
    {
        Busy = true;

        Debug.Log("Processing Normal");

        _blitMaterialNormal.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));
        _blitMaterialNormal.SetFloat(Spread, _heightFromDiffuseSettings.Spread);
        _blitMaterialNormal.SetFloat(SpreadBoost, _heightFromDiffuseSettings.SpreadBoost);
        _blitMaterialNormal.SetFloat(Samples, (int) _heightFromDiffuseSettings.Spread);
        _blitMaterialNormal.SetTexture(MainTex, MainGuiScript.NormalMap);
        _blitMaterialNormal.SetTexture(BlendTex, _blurMap1);

        ThisMaterial.SetFloat(IsNormal, 1.0f);
        ThisMaterial.SetTexture(BlurTex0, _blurMap0);
        ThisMaterial.SetTexture(BlurTex1, _blurMap1);
        ThisMaterial.SetTexture(MainTex, MainGuiScript.NormalMap);

        var yieldCountDown = 5;

        for (var i = 1; i < 100; i++)
        {
            _blitMaterialNormal.SetFloat(BlendAmount, 1.0f / i);
            _blitMaterialNormal.SetFloat(Progress, i / 100.0f);

            Graphics.Blit(MainGuiScript.NormalMap, _blurMap0, _blitMaterialNormal, 0);
            Graphics.Blit(_blurMap0, _blurMap1);

            yieldCountDown -= 1;
            if (yieldCountDown > 0) continue;
            yieldCountDown = 5;
            yield return new WaitForSeconds(0.01f);
        }

        Busy = false;
    }

    public IEnumerator ProcessDiffuse()
    {
        Busy = true;
        ThisMaterial.SetFloat(IsNormal, 0.0f);

        _blitMaterialSample.SetInt(IsolateSample1, _heightFromDiffuseSettings.IsolateSample1 ? 1 : 0);
        _blitMaterialSample.SetInt(UseSample1, _heightFromDiffuseSettings.UseSample1 ? 1 : 0);
        _blitMaterialSample.SetColor(SampleColor1, _heightFromDiffuseSettings.SampleColor1);
        _blitMaterialSample.SetVector(SampleUv1,
            new Vector4(_heightFromDiffuseSettings.SampleUv1.x, _heightFromDiffuseSettings.SampleUv1.y, 0, 0));
        _blitMaterialSample.SetFloat(HueWeight1, _heightFromDiffuseSettings.HueWeight1);
        _blitMaterialSample.SetFloat(SatWeight1, _heightFromDiffuseSettings.SatWeight1);
        _blitMaterialSample.SetFloat(LumWeight1, _heightFromDiffuseSettings.LumWeight1);
        _blitMaterialSample.SetFloat(MaskLow1, _heightFromDiffuseSettings.MaskLow1);
        _blitMaterialSample.SetFloat(MaskHigh1, _heightFromDiffuseSettings.MaskHigh1);
        _blitMaterialSample.SetFloat(Sample1Height, _heightFromDiffuseSettings.Sample1Height);

        _blitMaterialSample.SetInt(IsolateSample2, _heightFromDiffuseSettings.IsolateSample2 ? 1 : 0);
        _blitMaterialSample.SetInt(UseSample2, _heightFromDiffuseSettings.UseSample2 ? 1 : 0);
        _blitMaterialSample.SetColor(SampleColor2, _heightFromDiffuseSettings.SampleColor2);
        _blitMaterialSample.SetVector(SampleUv2,
            new Vector4(_heightFromDiffuseSettings.SampleUv2.x, _heightFromDiffuseSettings.SampleUv2.y, 0, 0));
        _blitMaterialSample.SetFloat(HueWeight2, _heightFromDiffuseSettings.HueWeight2);
        _blitMaterialSample.SetFloat(SatWeight2, _heightFromDiffuseSettings.SatWeight2);
        _blitMaterialSample.SetFloat(LumWeight2, _heightFromDiffuseSettings.LumWeight2);
        _blitMaterialSample.SetFloat(MaskLow2, _heightFromDiffuseSettings.MaskLow2);
        _blitMaterialSample.SetFloat(MaskHigh2, _heightFromDiffuseSettings.MaskHigh2);
        _blitMaterialSample.SetFloat(Sample2Height, _heightFromDiffuseSettings.Sample2Height);

        if (_heightFromDiffuseSettings.UseSample1 == false && _heightFromDiffuseSettings.UseSample2 == false)
            _blitMaterialSample.SetFloat(SampleBlend, 0.0f);
        else
            _blitMaterialSample.SetFloat(SampleBlend, _heightFromDiffuseSettings.SampleBlend);

        _blitMaterialSample.SetFloat(FinalContrast, _heightFromDiffuseSettings.FinalContrast);
        _blitMaterialSample.SetFloat(FinalBias, _heightFromDiffuseSettings.FinalBias);

        Graphics.Blit(
            _heightFromDiffuseSettings.UseOriginalDiffuse ? MainGuiScript.DiffuseMapOriginal : MainGuiScript.DiffuseMap,
            _blurMap0, _blitMaterialSample, 0);

        _blitMaterial.SetVector(ImageSize, new Vector4(_imageSizeX, _imageSizeY, 0, 0));
        _blitMaterial.SetFloat(BlurContrast, 1.0f);

        var extraSpread = (_blurMap0.width + _blurMap0.height) * 0.5f / 1024.0f;
        var spread = 1.0f;

        // Blur the image 1
        _blitMaterial.SetInt(BlurSamples, 4);
        _blitMaterial.SetFloat(BlurSpread, spread);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap0, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap1, _blitMaterial, 1);

        spread += extraSpread;

        // Blur the image 2
        _blitMaterial.SetFloat(BlurSpread, spread);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap1, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap2, _blitMaterial, 1);

        spread += 2 * extraSpread;

        // Blur the image 3
        _blitMaterial.SetFloat(BlurSpread, spread);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap2, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap3, _blitMaterial, 1);

        spread += 4 * extraSpread;

        // Blur the image 4
        _blitMaterial.SetFloat(BlurSpread, spread);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap3, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap4, _blitMaterial, 1);

        spread += 8 * extraSpread;

        // Blur the image 5
        _blitMaterial.SetFloat(BlurSpread, spread);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap4, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap5, _blitMaterial, 1);

        spread += 16 * extraSpread;

        // Blur the image 6
        _blitMaterial.SetFloat(BlurSpread, spread);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap5, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap6, _blitMaterial, 1);


        // Average Color
        _blitMaterial.SetInt(BlurSamples, 32);
        _blitMaterial.SetFloat(BlurSpread, 64.0f * extraSpread);
        _blitMaterial.SetVector(BlurDirection, new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap6, _avgTempMap, _blitMaterial, 1);
        _blitMaterial.SetVector(BlurDirection, new Vector4(0, 1, 0, 0));
        Graphics.Blit(_avgTempMap, _avgMap, _blitMaterial, 1);


        ThisMaterial.SetTexture(MainTex,
            _heightFromDiffuseSettings.UseOriginalDiffuse
                ? MainGuiScript.DiffuseMapOriginal
                : MainGuiScript.DiffuseMap);

        ThisMaterial.SetTexture(BlurTex0, _blurMap0);
        ThisMaterial.SetTexture(BlurTex1, _blurMap1);
        ThisMaterial.SetTexture(BlurTex2, _blurMap2);
        ThisMaterial.SetTexture(BlurTex3, _blurMap3);
        ThisMaterial.SetTexture(BlurTex4, _blurMap4);
        ThisMaterial.SetTexture(BlurTex5, _blurMap5);
        ThisMaterial.SetTexture(BlurTex6, _blurMap6);
        ThisMaterial.SetTexture(AvgTex, _avgMap);

        yield return new WaitForSeconds(0.01f);

        Busy = false;
    }
}