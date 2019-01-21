using System.Collections;
using UnityEngine;

public class SpecularGui : MonoBehaviour
{
    private RenderTexture _BlurMap;
    private Texture2D _DiffuseMap;
    private Texture2D _EdgeMap;

    private Texture2D _HeightMap;
    private Texture2D _MetallicMap;
    private Texture2D _NormalMap;
    private Texture2D _SmoothnessMap;

    private RenderTexture _TempMap;
    private Material blitMaterial;

    private float BlurContrast = 1.0f;

    private int BlurSize = 20;

    private float ColorLerp = 0.5f;

    private float DarkMaskPow = 1.0f;
    private float DarkPow = 1.0f;
    private float DiffuseBias;

    private float DiffuseContrast = 1.0f;
    private bool doStuff;
    private float FinalBias;

    private float FinalContrast = 1.0f;

    private int imageSizeX;
    private int imageSizeY;

    private int LastBlurSize = 20;
    //float BlurWeight = 1.0f;

    private float LightMaskPow = 1.0f;
    private float LightPow = 1.0f;

    public GameObject MainGuiObject;
    private MainGui MainGuiScript;
    private bool newTexture;

    private float Saturation;


    public GameObject testObject;

    private Textures textureToLoad = Textures.diffuse;

    public Material thisMaterial;

    // Use this for initialization
    private void Start()
    {
        LastBlurSize = BlurSize;

        MainGuiScript = MainGuiObject.GetComponent<MainGui>();

        //thisMaterial = testObject.renderer.material;
        testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;

        blitMaterial = new Material(Shader.Find("Hidden/Blit_Shader"));
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
        if (BlurSize != LastBlurSize) doStuff = true;

        LastBlurSize = BlurSize;

        if (newTexture)
        {
            InitializeTextures();
            newTexture = false;
        }

        if (doStuff)
        {
            StartCoroutine(ProcessBlur());
            doStuff = false;
        }

        //thisMaterial.SetFloat ("_BlurWeight", BlurWeight);

        thisMaterial.SetFloat("_BlurContrast", BlurContrast);

        thisMaterial.SetFloat("_LightMaskPow", LightMaskPow);
        thisMaterial.SetFloat("_LightPow", LightPow);

        thisMaterial.SetFloat("_DarkMaskPow", DarkMaskPow);
        thisMaterial.SetFloat("_DarkPow", DarkPow);

        thisMaterial.SetFloat("_DiffuseContrast", DiffuseContrast);
        thisMaterial.SetFloat("_DiffuseBias", DiffuseBias);

        thisMaterial.SetFloat("_FinalContrast", FinalContrast);

        thisMaterial.SetFloat("_FinalBias", FinalBias);

        thisMaterial.SetFloat("_ColorLerp", ColorLerp);

        thisMaterial.SetFloat("_Saturation", Saturation);
    }

    private string FloatToString(float num, int length)
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

        var spacingX = 0;
        var spacingY = 50;
        var spacing2Y = 70;

        var offsetX = 30;
        var offsetY = 330;

        GUI.Box(new Rect(20, 300, 300, 630), "Metallic");

        GUI.Label(new Rect(offsetX, offsetY, 250, 30),
            "Contrast: " + FloatToString(DiffuseContrast, 4) + " Bias: " + FloatToString(DiffuseBias, 4));
        DiffuseContrast = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), DiffuseContrast, -1.0f, 1.0f);
        DiffuseBias = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 50, 280, 10), DiffuseBias, -0.5f, 0.5f);

        offsetY += spacing2Y;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30),
            "Blur Size: " + BlurSize + " Contrast: " + FloatToString(BlurContrast, 4));
        BlurSize = Mathf.FloorToInt(GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), BlurSize, 1.0f,
            100.0f));
        BlurContrast = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 50, 280, 10), BlurContrast, -5.0f, 5.0f);

        offsetY += spacing2Y;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30),
            "Light Mask Power: " + FloatToString(LightMaskPow, 4) + " Intensity: " + FloatToString(LightPow, 4));
        LightMaskPow = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), LightMaskPow, 0.1f, 5.0f);
        LightPow = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 50, 280, 10), LightPow, -1.0f, 1.0f);

        offsetY += spacing2Y;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30),
            "Dark Mask Power: " + FloatToString(DarkMaskPow, 4) + " Intensity: " + FloatToString(DarkPow, 4));
        DarkMaskPow = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), DarkMaskPow, 0.1f, 5.0f);
        DarkPow = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 50, 280, 10), DarkPow, 0.0f, 5.0f);

        offsetY += spacing2Y;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Final Contrast: " + FloatToString(FinalContrast, 4));
        FinalContrast = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), FinalContrast, -2.0f, 2.0f);

        offsetY += spacingY;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Final Bias: " + FloatToString(FinalBias, 4));
        FinalBias = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), FinalBias, -0.5f, 0.5f);

        offsetY += spacingY;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Keep Original Color: " + FloatToString(ColorLerp, 4));
        ColorLerp = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), ColorLerp, 0.0f, 1.0f);

        offsetY += spacingY;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Saturation: " + FloatToString(Saturation, 4));
        Saturation = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 30, 280, 10), Saturation, 0.0f, 1.0f);

        offsetY += spacingY;

        if (GUI.Button(new Rect(offsetX, offsetY, 130, 30), "Set as Metallic"))
            StartCoroutine(ProcessRoughSpec(Textures.specular));
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

        CleanupTexture(_BlurMap);
        CleanupTexture(_TempMap);
    }

    private void InitializeTextures()
    {
        testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;

        CleanupTextures();

        _DiffuseMap = MainGuiScript.DiffuseMapOriginal;

        thisMaterial.SetTexture("_MainTex", _DiffuseMap);

        imageSizeX = _DiffuseMap.width;
        imageSizeY = _DiffuseMap.height;

        Debug.Log("Initializing Textures of size: " + imageSizeX + "x" + imageSizeY);

        _TempMap = new RenderTexture(imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf);
        _TempMap.wrapMode = TextureWrapMode.Repeat;
        _BlurMap = new RenderTexture(imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf);
        _BlurMap.wrapMode = TextureWrapMode.Repeat;
    }

    private IEnumerator ProcessRoughSpec(Textures whichTexture)
    {
        Debug.Log("Processing Height");

        blitMaterial.SetVector("_ImageSize", new Vector4(imageSizeX, imageSizeY, 0, 0));

        blitMaterial.SetTexture("_MainTex", _DiffuseMap);

        blitMaterial.SetTexture("_BlurTex", _BlurMap);
        blitMaterial.SetFloat("_BlurContrast", BlurContrast);

        blitMaterial.SetFloat("_LightMaskPow", LightMaskPow);
        blitMaterial.SetFloat("_LightPow", LightPow);

        blitMaterial.SetFloat("_DarkMaskPow", DarkMaskPow);
        blitMaterial.SetFloat("_DarkPow", DarkPow);

        blitMaterial.SetFloat("_DiffuseContrast", DiffuseContrast);
        blitMaterial.SetFloat("_DiffuseBias", DiffuseBias);

        blitMaterial.SetFloat("_FinalContrast", FinalContrast);

        blitMaterial.SetFloat("_FinalBias", FinalBias);

        blitMaterial.SetFloat("_ColorLerp", ColorLerp);

        blitMaterial.SetFloat("_Saturation", Saturation);

        CleanupTexture(_TempMap);
        _TempMap = new RenderTexture(imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32);
        _TempMap.wrapMode = TextureWrapMode.Repeat;

        Graphics.Blit(_DiffuseMap, _TempMap, blitMaterial, 10);

        RenderTexture.active = _TempMap;

        if (whichTexture == Textures.roughness)
        {
            MainGuiScript.SmoothnessMap = new Texture2D(_TempMap.width, _TempMap.height);
            MainGuiScript.SmoothnessMap.ReadPixels(new Rect(0, 0, _TempMap.width, _TempMap.height), 0, 0);
            MainGuiScript.SmoothnessMap.Apply();
        }
        else if (whichTexture == Textures.specular)
        {
            MainGuiScript.MetallicMap = new Texture2D(_TempMap.width, _TempMap.height);
            MainGuiScript.MetallicMap.ReadPixels(new Rect(0, 0, _TempMap.width, _TempMap.height), 0, 0);
            MainGuiScript.MetallicMap.Apply();
        }
        else
        {
            MainGuiScript.DiffuseMap = new Texture2D(_TempMap.width, _TempMap.height);
            MainGuiScript.DiffuseMap.ReadPixels(new Rect(0, 0, _TempMap.width, _TempMap.height), 0, 0);
            MainGuiScript.DiffuseMap.Apply();
        }

        yield return new WaitForEndOfFrame();

        CleanupTexture(_TempMap);
    }

    private IEnumerator ProcessBlur()
    {
        Debug.Log("Processing Blur");

        blitMaterial.SetVector("_ImageSize", new Vector4(imageSizeX, imageSizeY, 0, 0));
        blitMaterial.SetFloat("_BlurContrast", 1.0f);

        // Blur the image 1
        blitMaterial.SetTexture("_MainTex", _DiffuseMap);
        blitMaterial.SetInt("_BlurSamples", BlurSize);
        blitMaterial.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
        Graphics.Blit(_DiffuseMap, _TempMap, blitMaterial, 1);

        blitMaterial.SetTexture("_MainTex", _TempMap);
        blitMaterial.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
        Graphics.Blit(_TempMap, _BlurMap, blitMaterial, 1);

        thisMaterial.SetTexture("_BlurTex", _BlurMap);

        yield return new WaitForEndOfFrame();
    }

    private enum Textures
    {
        height,
        diffuse,
        specular,
        roughness,
        normal
    }
}