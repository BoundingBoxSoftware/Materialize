#region

using System.Collections;
using UnityEngine;

#endregion

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

public class SpecularGui : MonoBehaviour
{
    private static readonly int BlurContrast = Shader.PropertyToID("_BlurContrast");
    private static readonly int LightMaskPow = Shader.PropertyToID("_LightMaskPow");
    private static readonly int LightPow = Shader.PropertyToID("_LightPow");
    private static readonly int DarkMaskPow = Shader.PropertyToID("_DarkMaskPow");
    private static readonly int DarkPow = Shader.PropertyToID("_DarkPow");
    private static readonly int DiffuseContrast = Shader.PropertyToID("_DiffuseContrast");
    private static readonly int DiffuseBias = Shader.PropertyToID("_DiffuseBias");
    private static readonly int FinalContrast = Shader.PropertyToID("_FinalContrast");
    private static readonly int FinalBias = Shader.PropertyToID("_FinalBias");
    private static readonly int ColorLerp = Shader.PropertyToID("_ColorLerp");
    private static readonly int Saturation = Shader.PropertyToID("_Saturation");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private Material _blitMaterial;

    private float _blurContrast = 1.0f;
    private RenderTexture _blurMap;

    private int _blurSize = 20;

    private float _colorLerp = 0.5f;

    private float _darkMaskPow = 1.0f;
    private float _darkPow = 1.0f;
    private float _diffuseBias;

    private float _diffuseContrast = 1.0f;
    private Texture2D _diffuseMap;
    private bool _doStuff;
    private Texture2D _edgeMap;
    private float _finalBias;

    private float _finalContrast = 1.0f;

    private Texture2D _heightMap;

    private int _imageSizeX;
    private int _imageSizeY;

    private int _lastBlurSize = 20;
    //float BlurWeight = 1.0f;

    private float _lightMaskPow = 1.0f;
    private float _lightPow = 1.0f;
    private Texture2D _metallicMap;

    private bool _newTexture;
    private Texture2D _normalMap;

    private float _saturation;
    private Texture2D _smoothnessMap;

    private RenderTexture _tempMap;


    public GameObject TestObject;

    public Material ThisMaterial;

    // Use this for initialization
    private void Start()
    {
        _lastBlurSize = _blurSize;
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;
        _blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));
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
        if (_blurSize != _lastBlurSize) _doStuff = true;

        _lastBlurSize = _blurSize;

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

        //thisMaterial.SetFloat ("_BlurWeight", BlurWeight);

        ThisMaterial.SetFloat(BlurContrast, _blurContrast);

        ThisMaterial.SetFloat(LightMaskPow, _lightMaskPow);
        ThisMaterial.SetFloat(LightPow, _lightPow);

        ThisMaterial.SetFloat(DarkMaskPow, _darkMaskPow);
        ThisMaterial.SetFloat(DarkPow, _darkPow);

        ThisMaterial.SetFloat(DiffuseContrast, _diffuseContrast);
        ThisMaterial.SetFloat(DiffuseBias, _diffuseBias);

        ThisMaterial.SetFloat(FinalContrast, _finalContrast);

        ThisMaterial.SetFloat(FinalBias, _finalBias);

        ThisMaterial.SetFloat(ColorLerp, _colorLerp);

        ThisMaterial.SetFloat(Saturation, _saturation);
    }

    private static string FloatToString(float num, int length)
    {
        var numString = num.ToString();
        var numStringLength = numString.Length;
        var lastIndex = Mathf.FloorToInt(Mathf.Min(numStringLength, (float) length));

        return numString.Substring(0, lastIndex);
    }

    private void OnGUI()
    {
        //toolsWindowRect = new Rect (20, 20, 300, 700);

        //toolsWindowRect = GUI.Window (toolsWindowID, toolsWindowRect, DrawToolsWindow, toolsWindowTitle);

        const int spacingY = 50;
        const int spacing2Y = 70;

        const int offsetX = 30;
        var offsetY = 330;

        GUI.Box(new Rect(20, 300, 300, 630), "Metallic");

        GUI.Label(new Rect(offsetX, offsetY, 250, 30),
            "Contrast: " + FloatToString(_diffuseContrast, 4) + " Bias: " + FloatToString(_diffuseBias, 4));
        _diffuseContrast =
            GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), _diffuseContrast, -1.0f, 1.0f);
        _diffuseBias = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 50, 280, 10), _diffuseBias, -0.5f, 0.5f);

        offsetY += spacing2Y;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30),
            "Blur Size: " + _blurSize + " Contrast: " + FloatToString(_blurContrast, 4));
        _blurSize = Mathf.FloorToInt(GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), _blurSize, 1.0f,
            100.0f));
        _blurContrast = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 50, 280, 10), _blurContrast, -5.0f, 5.0f);

        offsetY += spacing2Y;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30),
            "Light Mask Power: " + FloatToString(_lightMaskPow, 4) + " Intensity: " + FloatToString(_lightPow, 4));
        _lightMaskPow = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), _lightMaskPow, 0.1f, 5.0f);
        _lightPow = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 50, 280, 10), _lightPow, -1.0f, 1.0f);

        offsetY += spacing2Y;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30),
            "Dark Mask Power: " + FloatToString(_darkMaskPow, 4) + " Intensity: " + FloatToString(_darkPow, 4));
        _darkMaskPow = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), _darkMaskPow, 0.1f, 5.0f);
        _darkPow = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 50, 280, 10), _darkPow, 0.0f, 5.0f);

        offsetY += spacing2Y;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Final Contrast: " + FloatToString(_finalContrast, 4));
        _finalContrast = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), _finalContrast, -2.0f, 2.0f);

        offsetY += spacingY;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Final Bias: " + FloatToString(_finalBias, 4));
        _finalBias = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), _finalBias, -0.5f, 0.5f);

        offsetY += spacingY;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Keep Original Color: " + FloatToString(_colorLerp, 4));
        _colorLerp = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), _colorLerp, 0.0f, 1.0f);

        offsetY += spacingY;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Saturation: " + FloatToString(_saturation, 4));
        _saturation = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), _saturation, 0.0f, 1.0f);

        offsetY += spacingY;

        if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Set as Metallic"))
            StartCoroutine(ProcessRoughSpec(Textures.Specular));
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
        Debug.Log("Cleaning Up Textures");

        CleanupTexture(_blurMap);
        CleanupTexture(_tempMap);
    }

    private void InitializeTextures()
    {
        TestObject.GetComponent<Renderer>().sharedMaterial = ThisMaterial;

        CleanupTextures();

        _diffuseMap = MainGui.Instance.DiffuseMapOriginal;

        ThisMaterial.SetTexture(MainTex, _diffuseMap);

        _imageSizeX = _diffuseMap.width;
        _imageSizeY = _diffuseMap.height;

        Debug.Log("Initializing Textures of size: " + _imageSizeX + "x" + _imageSizeY);

        _tempMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf)
        {
            wrapMode = TextureWrapMode.Repeat
        };
        _blurMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGBHalf)
        {
            wrapMode = TextureWrapMode.Repeat
        };
    }

    private IEnumerator ProcessRoughSpec(Textures whichTexture)
    {
        Debug.Log("Processing Height");

        _blitMaterial.SetVector("_ImageSize", new Vector4(_imageSizeX, _imageSizeY, 0, 0));

        _blitMaterial.SetTexture("_MainTex", _diffuseMap);

        _blitMaterial.SetTexture("_BlurTex", _blurMap);
        _blitMaterial.SetFloat("_BlurContrast", _blurContrast);

        _blitMaterial.SetFloat("_LightMaskPow", _lightMaskPow);
        _blitMaterial.SetFloat("_LightPow", _lightPow);

        _blitMaterial.SetFloat("_DarkMaskPow", _darkMaskPow);
        _blitMaterial.SetFloat("_DarkPow", _darkPow);

        _blitMaterial.SetFloat("_DiffuseContrast", _diffuseContrast);
        _blitMaterial.SetFloat("_DiffuseBias", _diffuseBias);

        _blitMaterial.SetFloat("_FinalContrast", _finalContrast);

        _blitMaterial.SetFloat("_FinalBias", _finalBias);

        _blitMaterial.SetFloat("_ColorLerp", _colorLerp);

        _blitMaterial.SetFloat("_Saturation", _saturation);

        CleanupTexture(_tempMap);
        _tempMap = new RenderTexture(_imageSizeX, _imageSizeY, 0, RenderTextureFormat.ARGB32)
        {
            wrapMode = TextureWrapMode.Repeat
        };

        Graphics.Blit(_diffuseMap, _tempMap, _blitMaterial, 10);

        RenderTexture.active = _tempMap;

        switch (whichTexture)
        {
            case Textures.Roughness:
                MainGui.Instance.SmoothnessMap = new Texture2D(_tempMap.width, _tempMap.height);
                MainGui.Instance.SmoothnessMap.ReadPixels(new Rect(0, 0, _tempMap.width, _tempMap.height), 0, 0);
                MainGui.Instance.SmoothnessMap.Apply();
                break;
            case Textures.Specular:
                MainGui.Instance.MetallicMap = new Texture2D(_tempMap.width, _tempMap.height);
                MainGui.Instance.MetallicMap.ReadPixels(new Rect(0, 0, _tempMap.width, _tempMap.height), 0, 0);
                MainGui.Instance.MetallicMap.Apply();
                break;
            default:
                MainGui.Instance.DiffuseMap = new Texture2D(_tempMap.width, _tempMap.height);
                MainGui.Instance.DiffuseMap.ReadPixels(new Rect(0, 0, _tempMap.width, _tempMap.height), 0, 0);
                MainGui.Instance.DiffuseMap.Apply();
                break;
        }

        yield return new WaitForEndOfFrame();

        CleanupTexture(_tempMap);
    }

    private IEnumerator ProcessBlur()
    {
        Debug.Log("Processing Blur");

        _blitMaterial.SetVector("_ImageSize", new Vector4(_imageSizeX, _imageSizeY, 0, 0));
        _blitMaterial.SetFloat("_BlurContrast", 1.0f);

        // Blur the image 1
        _blitMaterial.SetTexture("_MainTex", _diffuseMap);
        _blitMaterial.SetInt("_BlurSamples", _blurSize);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_diffuseMap, _tempMap, _blitMaterial, 1);

        _blitMaterial.SetTexture("_MainTex", _tempMap);
        _blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_tempMap, _blurMap, _blitMaterial, 1);

        ThisMaterial.SetTexture("_BlurTex", _blurMap);

        yield return new WaitForEndOfFrame();
    }

    private enum Textures
    {
        Specular,
        Roughness
    }
}