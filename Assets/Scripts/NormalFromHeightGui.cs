#region

using System.Collections;
using UnityEngine;

#endregion

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

public class NormalFromHeightGui : MonoBehaviour
{
    private static readonly int Blur0Weight = Shader.PropertyToID("_Blur0Weight");
    private static readonly int Blur1Weight = Shader.PropertyToID("_Blur1Weight");
    private static readonly int Blur2Weight = Shader.PropertyToID("_Blur2Weight");
    private static readonly int Blur3Weight = Shader.PropertyToID("_Blur3Weight");
    private static readonly int Blur4Weight = Shader.PropertyToID("_Blur4Weight");
    private static readonly int Blur5Weight = Shader.PropertyToID("_Blur5Weight");
    private static readonly int Blur6Weight = Shader.PropertyToID("_Blur6Weight");
    private static readonly int Slider = Shader.PropertyToID("_Slider");
    private static readonly int Angularity = Shader.PropertyToID("_Angularity");
    private static readonly int AngularIntensity = Shader.PropertyToID("_AngularIntensity");
    private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
    private static readonly int LightDir = Shader.PropertyToID("_LightDir");
    private static readonly int HeightTex = Shader.PropertyToID("_HeightTex");
    private Material _blitMaterial;
    private RenderTexture _blurMap0;
    private RenderTexture _blurMap1;
    private RenderTexture _blurMap2;
    private RenderTexture _blurMap3;
    private RenderTexture _blurMap4;
    private RenderTexture _blurMap5;
    private RenderTexture _blurMap6;
    private bool _doStuff;
    private int _imageSizeX = 1024;
    private int _imageSizeY = 1024;

    private MainGui _mgs;
    private bool _newTexture;

    private NormalFromHeightSettings _settings;
    private bool _settingsInitialized;

    private float _slider = 0.5f;

    private RenderTexture _tempBlurMap;
    private RenderTexture _tempNormalMap;

    private Rect _windowRect = new Rect(30, 300, 300, 450);

    [HideInInspector] public bool Busy;

    [HideInInspector] public RenderTexture HeightBlurMap;

    public Light MainLight;

    public GameObject TestObject;

    public Material ThisMaterial;

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.NormalFromHeightSettings = _settings;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.NormalFromHeightSettings != null)
        {
            _settings = projectObject.NormalFromHeightSettings;
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
        Debug.Log("Initializing Normal From Height Settings");
        _settings = new NormalFromHeightSettings();
        _settingsInitialized = true;
    }

    // Use this for initialization
    private void Start()
    {
        _mgs = MainGui.Instance;

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

    private void SetWeightEqDefault()
    {
        _settings.Blur0Weight = 0.3f;
        _settings.Blur1Weight = 0.35f;
        _settings.Blur2Weight = 0.5f;
        _settings.Blur3Weight = 0.8f;
        _settings.Blur4Weight = 1.0f;
        _settings.Blur5Weight = 0.95f;
        _settings.Blur6Weight = 0.8f;
        _doStuff = true;
    }

    private void SetWeightEqSmooth()
    {
        _settings.Blur0Weight = 0.1f;
        _settings.Blur1Weight = 0.15f;
        _settings.Blur2Weight = 0.25f;
        _settings.Blur3Weight = 0.6f;
        _settings.Blur4Weight = 0.9f;
        _settings.Blur5Weight = 1.0f;
        _settings.Blur6Weight = 1.0f;
        _doStuff = true;
    }

    private void SetWeightEqCrisp()
    {
        _settings.Blur0Weight = 1.0f;
        _settings.Blur1Weight = 0.9f;
        _settings.Blur2Weight = 0.6f;
        _settings.Blur3Weight = 0.4f;
        _settings.Blur4Weight = 0.25f;
        _settings.Blur5Weight = 0.15f;
        _settings.Blur6Weight = 0.1f;
        _doStuff = true;
    }

    // ReSharper disable once IdentifierTypo
    private void SetWeightEqMids()
    {
        _settings.Blur0Weight = 0.15f;
        _settings.Blur1Weight = 0.5f;
        _settings.Blur2Weight = 0.85f;
        _settings.Blur3Weight = 1.0f;
        _settings.Blur4Weight = 0.85f;
        _settings.Blur5Weight = 0.5f;
        _settings.Blur6Weight = 0.15f;
        _doStuff = true;
    }

    private void Update()
    {
        if (_newTexture)
        {
            InitializeTextures();
            _newTexture = false;
        }

        if (_doStuff)
        {
            StartCoroutine(ProcessHeight());
            _doStuff = false;
        }

        ThisMaterial.SetFloat(Blur0Weight, _settings.Blur0Weight);
        ThisMaterial.SetFloat(Blur1Weight, _settings.Blur1Weight);
        ThisMaterial.SetFloat(Blur2Weight, _settings.Blur2Weight);
        ThisMaterial.SetFloat(Blur3Weight, _settings.Blur3Weight);
        ThisMaterial.SetFloat(Blur4Weight, _settings.Blur4Weight);
        ThisMaterial.SetFloat(Blur5Weight, _settings.Blur5Weight);
        ThisMaterial.SetFloat(Blur6Weight, _settings.Blur6Weight);

        ThisMaterial.SetFloat(Slider, _slider);

        ThisMaterial.SetFloat(Angularity, _settings.Angularity);
        ThisMaterial.SetFloat(AngularIntensity, _settings.AngularIntensity);

        ThisMaterial.SetFloat(FinalContrast, _settings.FinalContrast);

        ThisMaterial.SetVector(LightDir, MainLight.transform.forward);
    }

    private void DoMyWindow(int windowId)
    {
        var offsetX = 10;
        var offsetY = 30;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Normal Reveal Slider");
        _slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), _slider, 0.0f, 1.0f);

        offsetY += 40;

        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Pre Contrast", _settings.Blur0Contrast,
            _settings.Blur0ContrastText, out _settings.Blur0Contrast, out _settings.Blur0ContrastText, 0.0f, 50.0f))
            _doStuff = true;
        offsetY += 50;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Equalizer");
        GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
        if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default")) SetWeightEqDefault();
        if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Smooth")) SetWeightEqSmooth();
        if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Crisp")) SetWeightEqCrisp();
        if (GUI.Button(new Rect(offsetX + 215, offsetY + 120, 60, 20), "Mids")) SetWeightEqMids();
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

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Angular Intensity");
        offsetY += 25;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.AngularIntensity,
            _settings.AngularIntensityText,
            out _settings.AngularIntensity, out _settings.AngularIntensityText, 0.0f, 1.0f);
        offsetY += 25;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Angularity Amount");
        offsetY += 25;
        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.Angularity, _settings.AngularityText,
            out _settings.Angularity,
            out _settings.AngularityText, 0.0f, 1.0f);

        offsetY += 30;

        if (_mgs.DiffuseMapOriginal)
        {
            GUI.enabled = true;
        }
        else
        {
            GUI.enabled = false;
            _settings.UseDiffuse = false;
        }

        var tempBool = _settings.UseDiffuse;
        _settings.UseDiffuse = GUI.Toggle(new Rect(offsetX, offsetY, 280, 30), _settings.UseDiffuse,
            " Shape from Diffuse (Unchecked from Height)");
        if (tempBool != _settings.UseDiffuse) _doStuff = true;
        offsetY += 30;

        GUI.enabled = true;

        GUI.Label(new Rect(offsetX, offsetY, 280, 30), " Shape Recognition, Rotation, Spread, Bias");
        offsetY += 30;
        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.ShapeRecognition,
            _settings.ShapeRecognitionText,
            out _settings.ShapeRecognition, out _settings.ShapeRecognitionText, 0.0f, 1.0f)) _doStuff = true;
        offsetY += 25;
        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.LightRotation, _settings.LightRotationText,
            out _settings.LightRotation, out _settings.LightRotationText, -3.14f, 3.14f)) _doStuff = true;
        offsetY += 25;
        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.SlopeBlur, _settings.SlopeBlurText,
            out _settings.SlopeBlur, out _settings.SlopeBlurText, 5, 100)) _doStuff = true;
        offsetY += 25;
        if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _settings.ShapeBias, _settings.ShapeBiasText,
            out _settings.ShapeBias, out _settings.ShapeBiasText, 0.0f, 1.0f)) _doStuff = true;
        offsetY += 30;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", _settings.FinalContrast,
            _settings.FinalContrastText, out _settings.FinalContrast, out _settings.FinalContrastText, 0.0f, 10.0f);
        offsetY += 50;

        if (GUI.Button(new Rect(offsetX + 10, offsetY, 130, 30), "Reset to Defaults"))
        {
            //_settingsInitialized = false;
            SetValues(new ProjectObject());
            //StartCoroutine(ProcessDiffuse());
        }

        if (GUI.Button(new Rect(offsetX + 150, offsetY, 130, 30), "Set as Normal Map")) StartCoroutine(ProcessNormal());

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = 630;

        _windowRect = GUI.Window(16, _windowRect, DoMyWindow, "Normal From Height");
    }

    public void InitializeTextures()
    {
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        CleanupTextures();

        if (!_mgs.HdHeightMap)
            ThisMaterial.SetTexture(HeightTex, _mgs.HeightMap);
        else
            ThisMaterial.SetTexture(HeightTex, _mgs.HdHeightMap);

        _imageSizeX = _mgs.HeightMap.width;
        _imageSizeY = _mgs.HeightMap.height;

        Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

        _tempBlurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        HeightBlurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.RHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap0 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap1 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap2 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap3 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap4 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap5 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        _blurMap6 = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
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
        CleanupTexture(HeightBlurMap);
        CleanupTexture(_blurMap0);
        CleanupTexture(_blurMap1);
        CleanupTexture(_blurMap2);
        CleanupTexture(_blurMap3);
        CleanupTexture(_blurMap4);
        CleanupTexture(_blurMap5);
        CleanupTexture(_blurMap6);
        CleanupTexture(_tempNormalMap);
    }

    public void StartProcessNormal()
    {
        StartCoroutine(ProcessNormal());
    }

    public IEnumerator ProcessNormal()
    {
        Busy = true;

        Debug.Log("Processing Normal");

        _blitMaterial.SetVector("_ImageSize", new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        _blitMaterial.SetFloat("_Blur0Weight", _settings.Blur0Weight);
        _blitMaterial.SetFloat("_Blur1Weight", _settings.Blur1Weight);
        _blitMaterial.SetFloat("_Blur2Weight", _settings.Blur2Weight);
        _blitMaterial.SetFloat("_Blur3Weight", _settings.Blur3Weight);
        _blitMaterial.SetFloat("_Blur4Weight", _settings.Blur4Weight);
        _blitMaterial.SetFloat("_Blur5Weight", _settings.Blur5Weight);
        _blitMaterial.SetFloat("_Blur6Weight", _settings.Blur6Weight);
        _blitMaterial.SetFloat("_FinalContrast", _settings.FinalContrast);

        _blitMaterial.SetTexture("_HeightBlurTex", HeightBlurMap);

        _blitMaterial.SetTexture("_MainTex", _blurMap0);
        _blitMaterial.SetTexture("_BlurTex0", _blurMap0);
        _blitMaterial.SetTexture("_BlurTex1", _blurMap1);
        _blitMaterial.SetTexture("_BlurTex2", _blurMap2);
        _blitMaterial.SetTexture("_BlurTex3", _blurMap3);
        _blitMaterial.SetTexture("_BlurTex4", _blurMap4);
        _blitMaterial.SetTexture("_BlurTex5", _blurMap5);
        _blitMaterial.SetTexture("_BlurTex6", _blurMap6);

        _blitMaterial.SetFloat("_Angularity", _settings.Angularity);
        _blitMaterial.SetFloat("_AngularIntensity", _settings.AngularIntensity);


        CleanupTexture(_tempNormalMap);
        _tempNormalMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear) {wrapMode = TextureWrapMode.Repeat};
        Graphics.Blit(_blurMap0, _tempNormalMap, _blitMaterial, 4);

        if (_mgs.NormalMap) Destroy(_mgs.NormalMap);

        RenderTexture.active = _tempNormalMap;
        _mgs.NormalMap = new Texture2D(_tempNormalMap.width, _tempNormalMap.height, TextureFormat.ARGB32, true, true);
        _mgs.NormalMap.ReadPixels(new Rect(0, 0, _tempNormalMap.width, _tempNormalMap.height), 0, 0);
        _mgs.NormalMap.Apply();

        yield return new WaitForSeconds(0.1f);

        CleanupTexture(_tempNormalMap);

        Busy = false;
    }

    public IEnumerator ProcessHeight()
    {
        Busy = true;

        Debug.Log("Processing Height");

        _blitMaterial.SetVector("_ImageSize", new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        // Blur the height map for normal slope
        _blitMaterial.SetFloat("_BlurSpread", 1.0f);
        _blitMaterial.SetInt("_BlurSamples", _settings.SlopeBlur);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        _blitMaterial.SetFloat("_BlurContrast", 1.0f);

        if (_mgs.DiffuseMapOriginal && _settings.UseDiffuse)
        {
            _blitMaterial.SetInt("_Desaturate", 1);
            Graphics.Blit(_mgs.DiffuseMapOriginal, _tempBlurMap, _blitMaterial, 1);
            _blitMaterial.SetTexture("_LightTex", _mgs.DiffuseMapOriginal);
        }
        else
        {
            if (_mgs.HdHeightMap == null)
            {
                _blitMaterial.SetInt("_Desaturate", 0);
                Graphics.Blit(_mgs.HeightMap, _tempBlurMap, _blitMaterial, 1);
                _blitMaterial.SetTexture("_LightTex", _mgs.HeightMap);
            }
            else
            {
                _blitMaterial.SetInt("_Desaturate", 0);
                Graphics.Blit(_mgs.HdHeightMap, _tempBlurMap, _blitMaterial, 1);
                _blitMaterial.SetTexture("_LightTex", _mgs.HdHeightMap);
            }
        }

        _blitMaterial.SetInt("_Desaturate", 0);

        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, HeightBlurMap, _blitMaterial, 1);

        _blitMaterial.SetFloat("_BlurSpread", 3.0f);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(HeightBlurMap, _tempBlurMap, _blitMaterial, 1);

        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, HeightBlurMap, _blitMaterial, 1);

        _blitMaterial.SetTexture("_LightBlurTex", HeightBlurMap);

        // Make normal from height
        _blitMaterial.SetFloat("_LightRotation", _settings.LightRotation);
        _blitMaterial.SetFloat("_ShapeRecognition", _settings.ShapeRecognition);
        _blitMaterial.SetFloat("_ShapeBias", _settings.ShapeBias);
        _blitMaterial.SetTexture("_DiffuseTex", _mgs.DiffuseMapOriginal);

        _blitMaterial.SetFloat("_BlurContrast", _settings.Blur0Contrast);

        if (_mgs.HdHeightMap == null)
            Graphics.Blit(_mgs.HeightMap, _blurMap0, _blitMaterial, 3);
        else
            Graphics.Blit(_mgs.HdHeightMap, _blurMap0, _blitMaterial, 3);

        var extraSpread = (_blurMap0.width + _blurMap0.height) * 0.5f / 1024.0f;
        var spread = 1.0f;

        _blitMaterial.SetFloat("_BlurContrast", 1.0f);
        _blitMaterial.SetInt("_Desaturate", 0);

        // Blur the image 1
        _blitMaterial.SetInt("_BlurSamples", 4);
        _blitMaterial.SetFloat("_BlurSpread", spread);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap0, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap1, _blitMaterial, 1);

        spread += extraSpread;

        // Blur the image 2
        _blitMaterial.SetFloat("_BlurSpread", spread);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap1, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap2, _blitMaterial, 1);

        spread += 2 * extraSpread;

        // Blur the image 3
        _blitMaterial.SetFloat("_BlurSpread", spread);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap2, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap3, _blitMaterial, 1);

        spread += 4 * extraSpread;

        // Blur the image 4
        _blitMaterial.SetFloat("_BlurSpread", spread);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap3, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap4, _blitMaterial, 1);

        spread += 8 * extraSpread;

        // Blur the image 5
        _blitMaterial.SetFloat("_BlurSpread", spread);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap4, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap5, _blitMaterial, 1);

        spread += 16 * extraSpread;

        // Blur the image 6
        _blitMaterial.SetFloat("_BlurSpread", spread);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_blurMap5, _tempBlurMap, _blitMaterial, 1);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempBlurMap, _blurMap6, _blitMaterial, 1);

        //if( _HDHeightMap != null) {
        //	thisMaterial.SetTexture ("_MainTex", _HDHeightMap);
        //} else {
        //	thisMaterial.SetTexture ("_MainTex", MGS._HeightMap);
        //}


        ThisMaterial.SetTexture("_BlurTex0", _blurMap0);
        ThisMaterial.SetTexture("_BlurTex1", _blurMap1);
        ThisMaterial.SetTexture("_BlurTex2", _blurMap2);
        ThisMaterial.SetTexture("_BlurTex3", _blurMap3);
        ThisMaterial.SetTexture("_BlurTex4", _blurMap4);
        ThisMaterial.SetTexture("_BlurTex5", _blurMap5);
        ThisMaterial.SetTexture("_BlurTex6", _blurMap6);

        yield return new WaitForSeconds(0.01f);

        Busy = false;
    }
}