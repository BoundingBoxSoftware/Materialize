using System.ComponentModel;
using UnityEngine;

public class MaterialSettings
{
    [DefaultValue(1.0f)] public float AOPower;

    [DefaultValue("1")] public string AOPowerText;

    [DefaultValue(1.0f)] public float EdgePower;

    [DefaultValue("1")] public string EdgePowerText;

    [DefaultValue(1.0f)] public float LightB;

    [DefaultValue(1.0f)] public float LightG;

    [DefaultValue(1.0f)] public float LightIntensity;

    [DefaultValue(1.0f)] public float LightR;

    [DefaultValue(1.0f)] public float Metallic;

    [DefaultValue("1")] public string MetallicText;

    [DefaultValue(0.5f)] public float Parallax;

    [DefaultValue("0.5")] public string ParallaxText;

    [DefaultValue(1.0f)] public float Smoothness;

    [DefaultValue("1")] public string SmoothnessText;

    [DefaultValue(0.0f)] public float TexOffsetX;

    [DefaultValue("0")] public string TexOffsetXText;

    [DefaultValue(0.0f)] public float TexOffsetY;

    [DefaultValue("0")] public string TexOffsetYText;

    [DefaultValue(1.0f)] public float TexTilingX = 1.0f;

    [DefaultValue("1")] public string TexTilingXText;

    [DefaultValue(1.0f)] public float TexTilingY = 1.0f;

    [DefaultValue("1")] public string TexTilingYText;

    public MaterialSettings()
    {
        TexTilingX = 1.0f;
        TexTilingY = 1.0f;
        TexOffsetX = 0.0f;
        TexOffsetY = 0.0f;

        TexTilingXText = "1";
        TexTilingYText = "1";
        TexOffsetXText = "0";
        TexOffsetYText = "0";

        Metallic = 1.0f;
        Smoothness = 1.0f;
        Parallax = 0.5f;
        EdgePower = 1.0f;
        AOPower = 1.0f;

        MetallicText = "1";
        SmoothnessText = "1";
        ParallaxText = "0.5";
        EdgePowerText = "1";
        AOPowerText = "1";

        LightR = 1.0f;
        LightG = 1.0f;
        LightB = 1.0f;
        LightIntensity = 1.0f;
    }
}

public class MaterialGui : MonoBehaviour
{
    private static readonly int Metallic = Shader.PropertyToID("_Metallic");
    private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
    private static readonly int Parallax = Shader.PropertyToID("_Parallax");
    private static readonly int EdgePower = Shader.PropertyToID("_EdgePower");
    private static readonly int AoPower = Shader.PropertyToID("_AOPower");
    private static readonly int Tiling = Shader.PropertyToID("_Tiling");
    private static readonly int DispOffset = Shader.PropertyToID("_DispOffset");
    private static readonly int DisplacementMap = Shader.PropertyToID("_DisplacementMap");
    private static readonly int DiffuseMap = Shader.PropertyToID("_DiffuseMap");
    private static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
    private static readonly int MetallicMap = Shader.PropertyToID("_MetallicMap");
    private static readonly int SmoothnessMap = Shader.PropertyToID("_SmoothnessMap");
    private static readonly int AoMap = Shader.PropertyToID("_AOMap");
    private static readonly int EdgeMap = Shader.PropertyToID("_EdgeMap");
    private Texture2D _AOMap;
    private Texture2D _DiffuseMap;
    private Texture2D _EdgeMap;

    private Texture2D _HeightMap;
    private Texture2D _MetallicMap;
    private Texture2D _NormalMap;
    private Texture2D _SmoothnessMap;
    private bool cubeShown;
    private bool cylinderShown;
    private float dispOffset = 0.5f;

    public GameObject LightObject;

    private MainGui MainGuiScript;

    private MaterialSettings MatS;

    private Texture2D myColorTexture;

    private bool planeShown = true;

    private bool settingsInitialized;
    private bool sphereShown;
    public GameObject testObject;
    public GameObject testObjectCube;
    public GameObject testObjectCylinder;

    public GameObject testObjectParent;
    public GameObject testObjectSphere;
    public ObjRotator testRotator;

    private Material thisMaterial;

    private Rect windowRect = new Rect(30, 300, 300, 530);

    private void OnDisable()
    {
        if (MainGuiScript.HideGui || testObjectParent == null) return;
        if (!testObjectParent.activeSelf) testRotator.Reset();

        testObjectParent.SetActive(true);
        testObjectCube.SetActive(false);
        testObjectCylinder.SetActive(false);
        testObjectSphere.SetActive(false);
    }

    // Use this for initialization
    private void Start()
    {
        InitializeSettings();
    }

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.MatS = MatS;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.MatS != null)
        {
            MatS = projectObject.MatS;
        }
        else
        {
            settingsInitialized = false;
            InitializeSettings();
        }
    }

    private void InitializeSettings()
    {
        if (settingsInitialized == false)
        {
            Debug.Log("Initializing MaterialSettings");
            MatS = new MaterialSettings();
            myColorTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            settingsInitialized = true;
        }
    }


    // Update is called once per frame
    private void Update()
    {
        thisMaterial.SetFloat(Metallic, MatS.Metallic);
        thisMaterial.SetFloat(Smoothness, MatS.Smoothness);
        thisMaterial.SetFloat(Parallax, MatS.Parallax);
        thisMaterial.SetFloat(EdgePower, MatS.EdgePower);
        thisMaterial.SetFloat(AoPower, MatS.AOPower);

        thisMaterial.SetVector(Tiling, new Vector4(MatS.TexTilingX, MatS.TexTilingY, MatS.TexOffsetX, MatS.TexOffsetY));

        LightObject.GetComponent<Light>().color = new Color(MatS.LightR, MatS.LightG, MatS.LightB);
        LightObject.GetComponent<Light>().intensity = MatS.LightIntensity;

        testObjectParent.SetActive(planeShown);
        testObjectCube.SetActive(cubeShown);
        testObjectCylinder.SetActive(cylinderShown);
        testObjectSphere.SetActive(sphereShown);
        thisMaterial.SetFloat(DispOffset, dispOffset);
    }

    private string FloatToString(float num, int length)
    {
        var numString = num.ToString();
        var numStringLength = numString.Length;
        var lastIndex = Mathf.FloorToInt(Mathf.Min(numStringLength, (float) length));

        return numString.Substring(0, lastIndex);
    }

    private void chooseLightColor(int posX, int posY)
    {
        MatS.LightR = GUI.VerticalSlider(new Rect(posX + 10, posY + 5, 30, 100), MatS.LightR, 1.0f, 0.0f);
        MatS.LightG = GUI.VerticalSlider(new Rect(posX + 40, posY + 5, 30, 100), MatS.LightG, 1.0f, 0.0f);
        MatS.LightB = GUI.VerticalSlider(new Rect(posX + 70, posY + 5, 30, 100), MatS.LightB, 1.0f, 0.0f);
        MatS.LightIntensity =
            GUI.VerticalSlider(new Rect(posX + 120, posY + 5, 30, 100), MatS.LightIntensity, 3.0f, 0.0f);

        GUI.Label(new Rect(posX + 10, posY + 110, 30, 30), "R");
        GUI.Label(new Rect(posX + 40, posY + 110, 30, 30), "G");
        GUI.Label(new Rect(posX + 70, posY + 110, 30, 30), "B");
        GUI.Label(new Rect(posX + 100, posY + 110, 100, 30), "Intensity");

        SetColorTexture();

        GUI.DrawTexture(new Rect(posX + 170, posY + 5, 100, 100), myColorTexture);
    }

    private void SetColorTexture()
    {
        var colorArray = new Color[1];
        colorArray[0] = new Color(MatS.LightR, MatS.LightG, MatS.LightB, 1.0f);

        myColorTexture.SetPixels(colorArray);
        myColorTexture.Apply();
    }

    private void DoMyWindow(int windowID)
    {
        var spacingX = 0;
        var spacingY = 50;
        var spacing2Y = 70;

        var offsetX = 10;
        var offsetY = 30;


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Metallic Multiplier", MatS.Metallic, MatS.MetallicText,
            out MatS.Metallic, out MatS.MetallicText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Smoothness Multiplier", MatS.Smoothness,
            MatS.SmoothnessText, out MatS.Smoothness, out MatS.SmoothnessText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Paralax Displacement", MatS.Parallax, MatS.ParallaxText,
            out MatS.Parallax, out MatS.ParallaxText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Edge Amount", MatS.EdgePower, MatS.EdgePowerText,
            out MatS.EdgePower, out MatS.EdgePowerText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Ambient Occlusion Power", MatS.AOPower, MatS.AOPowerText,
            out MatS.AOPower, out MatS.AOPowerText, 0.0f, 2.0f);
        offsetY += 40;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Light Color");
        chooseLightColor(offsetX, offsetY + 20);
        offsetY += 160;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling X", MatS.TexTilingX, MatS.TexTilingXText,
            out MatS.TexTilingX, out MatS.TexTilingXText, 0.1f, 5.0f);
        offsetY += 30;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling Y", MatS.TexTilingY, MatS.TexTilingYText,
            out MatS.TexTilingY, out MatS.TexTilingYText, 0.1f, 5.0f);
        offsetY += 50;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset X", MatS.TexOffsetX, MatS.TexOffsetXText,
            out MatS.TexOffsetX, out MatS.TexOffsetXText, -1.0f, 1.0f);
        offsetY += 30;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset Y", MatS.TexOffsetY, MatS.TexOffsetYText,
            out MatS.TexOffsetY, out MatS.TexOffsetYText, -1.0f, 1.0f);
        offsetY += 50;

        if (GUI.Button(new Rect(offsetX, offsetY, 60, 30), "Plane"))
        {
            planeShown = true;
            cubeShown = false;
            cylinderShown = false;
            sphereShown = false;
            dispOffset = 1.0f;
            Shader.DisableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 70, offsetY, 60, 30), "Cube"))
        {
            planeShown = false;
            cubeShown = true;
            cylinderShown = false;
            sphereShown = false;
            dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 140, offsetY, 70, 30), "Cylinder"))
        {
            planeShown = false;
            cubeShown = false;
            cylinderShown = true;
            sphereShown = false;
            dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 220, offsetY, 60, 30), "Sphere"))
        {
            planeShown = false;
            cubeShown = false;
            cylinderShown = false;
            sphereShown = true;
            dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        windowRect.width = 300;
        windowRect.height = 590;

        windowRect = GUI.Window(14, windowRect, DoMyWindow, "Full Material");
    }

    public void Initialize()
    {
        InitializeSettings();

        MainGuiScript = MainGui.Instance;
        thisMaterial = MainGuiScript.FullMaterial;

        thisMaterial.SetTexture(DisplacementMap, MainGuiScript.TextureGrey);
        thisMaterial.SetTexture(DiffuseMap, MainGuiScript.TextureGrey);
        thisMaterial.SetTexture(NormalMap, MainGuiScript.TextureNormal);
        thisMaterial.SetTexture(MetallicMap, MainGuiScript.TextureBlack);
        thisMaterial.SetTexture(SmoothnessMap, MainGuiScript.TextureGrey);
        thisMaterial.SetTexture(AoMap, MainGuiScript.TextureWhite);
        thisMaterial.SetTexture(EdgeMap, MainGuiScript.TextureGrey);
        thisMaterial.SetFloat(DispOffset, 1.0f);

        _HeightMap = MainGuiScript.HeightMap;

        if (MainGuiScript.DiffuseMap != null)
            _DiffuseMap = MainGuiScript.DiffuseMap;
        else
            _DiffuseMap = MainGuiScript.DiffuseMapOriginal;
        _NormalMap = MainGuiScript.NormalMap;
        _EdgeMap = MainGuiScript.EdgeMap;
        _MetallicMap = MainGuiScript.MetallicMap;
        _SmoothnessMap = MainGuiScript.SmoothnessMap;
        _AOMap = MainGuiScript.AoMap;

        if (_HeightMap != null) thisMaterial.SetTexture(DisplacementMap, _HeightMap);
        if (_DiffuseMap != null) thisMaterial.SetTexture(DiffuseMap, _DiffuseMap);
        if (_NormalMap != null) thisMaterial.SetTexture(NormalMap, _NormalMap);
        if (_MetallicMap != null) thisMaterial.SetTexture(MetallicMap, _MetallicMap);
        if (_SmoothnessMap != null) thisMaterial.SetTexture(SmoothnessMap, _SmoothnessMap);
        if (_AOMap != null) thisMaterial.SetTexture(AoMap, _AOMap);
        if (_EdgeMap != null) thisMaterial.SetTexture(EdgeMap, _EdgeMap);

        testObject.GetComponent<Renderer>().material = thisMaterial;
        testObjectCube.GetComponent<Renderer>().material = thisMaterial;
        testObjectCylinder.GetComponent<Renderer>().material = thisMaterial;
        testObjectSphere.GetComponent<Renderer>().material = thisMaterial;
    }
}