#region

using System.ComponentModel;
using UnityEngine;

#endregion

public class MaterialSettings
{
    [DefaultValue(1.0f)] public float AoPower;

    [DefaultValue("1")] public string AoPowerText;

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

    [DefaultValue(1.0f)] public float TexTilingX;

    [DefaultValue("1")] public string TexTilingXText;

    [DefaultValue(1.0f)] public float TexTilingY;

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
        AoPower = 1.0f;

        MetallicText = "1";
        SmoothnessText = "1";
        ParallaxText = "0.5";
        EdgePowerText = "1";
        AoPowerText = "1";

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
    private Texture2D _aoMap;
    private bool _cubeShown;
    private bool _cylinderShown;
    private Texture2D _diffuseMap;
    private float _dispOffset = 0.5f;
    private Texture2D _edgeMap;

    private Texture2D _heightMap;
    private Light _light;

    private MainGui _mainGuiScript;

    private MaterialSettings _materialSettings;
    private Texture2D _metallicMap;

    private Texture2D _myColorTexture;
    private Texture2D _normalMap;

    private bool _planeShown = true;

    private bool _settingsInitialized;
    private Texture2D _smoothnessMap;
    private bool _sphereShown;

    private Material _thisMaterial;

    private Rect _windowRect = new Rect(30, 300, 300, 530);

    public GameObject LightObject;
    public GameObject TestObject;
    public GameObject TestObjectCube;
    public GameObject TestObjectCylinder;

    public GameObject TestObjectParent;
    public GameObject TestObjectSphere;
    public ObjRotator TestRotator;

    private void OnDisable()
    {
        if (!_mainGuiScript.IsGuiHidden || TestObjectParent == null) return;
        if (!TestObjectParent.activeSelf) TestRotator.Reset();

        TestObjectParent.SetActive(true);
        TestObjectCube.SetActive(false);
        TestObjectCylinder.SetActive(false);
        TestObjectSphere.SetActive(false);
    }

    private void Start()
    {
        _light = LightObject.GetComponent<Light>();
        InitializeSettings();
    }

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.MaterialSettings = _materialSettings;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.MaterialSettings != null)
        {
            _materialSettings = projectObject.MaterialSettings;
        }
        else
        {
            _settingsInitialized = false;
            InitializeSettings();
        }
    }

    private void InitializeSettings()
    {
        if (_settingsInitialized) return;
        Debug.Log("Initializing MaterialSettings");
        _materialSettings = new MaterialSettings();
        _myColorTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        _settingsInitialized = true;
    }


    // Update is called once per frame
    private void Update()
    {
        _thisMaterial.SetFloat(Metallic, _materialSettings.Metallic);
        _thisMaterial.SetFloat(Smoothness, _materialSettings.Smoothness);
        _thisMaterial.SetFloat(Parallax, _materialSettings.Parallax);
        _thisMaterial.SetFloat(EdgePower, _materialSettings.EdgePower);
        _thisMaterial.SetFloat(AoPower, _materialSettings.AoPower);

        _thisMaterial.SetVector(Tiling,
            new Vector4(_materialSettings.TexTilingX, _materialSettings.TexTilingY, _materialSettings.TexOffsetX,
                _materialSettings.TexOffsetY));

        _light.color = new Color(_materialSettings.LightR, _materialSettings.LightG, _materialSettings.LightB);
        _light.intensity = _materialSettings.LightIntensity;

        TestObjectParent.SetActive(_planeShown);
        TestObjectCube.SetActive(_cubeShown);
        TestObjectCylinder.SetActive(_cylinderShown);
        TestObjectSphere.SetActive(_sphereShown);
        _thisMaterial.SetFloat(DispOffset, _dispOffset);
    }

    private void ChooseLightColor(int posX, int posY)
    {
        _materialSettings.LightR =
            GUI.VerticalSlider(new Rect(posX + 10, posY + 5, 30, 100), _materialSettings.LightR, 1.0f, 0.0f);
        _materialSettings.LightG =
            GUI.VerticalSlider(new Rect(posX + 40, posY + 5, 30, 100), _materialSettings.LightG, 1.0f, 0.0f);
        _materialSettings.LightB =
            GUI.VerticalSlider(new Rect(posX + 70, posY + 5, 30, 100), _materialSettings.LightB, 1.0f, 0.0f);
        _materialSettings.LightIntensity =
            GUI.VerticalSlider(new Rect(posX + 120, posY + 5, 30, 100), _materialSettings.LightIntensity, 3.0f, 0.0f);

        GUI.Label(new Rect(posX + 10, posY + 110, 30, 30), "R");
        GUI.Label(new Rect(posX + 40, posY + 110, 30, 30), "G");
        GUI.Label(new Rect(posX + 70, posY + 110, 30, 30), "B");
        GUI.Label(new Rect(posX + 100, posY + 110, 100, 30), "Intensity");

        SetColorTexture();

        GUI.DrawTexture(new Rect(posX + 170, posY + 5, 100, 100), _myColorTexture);
    }

    private void SetColorTexture()
    {
        var colorArray = new Color[1];
        colorArray[0] = new Color(_materialSettings.LightR, _materialSettings.LightG, _materialSettings.LightB, 1.0f);

        _myColorTexture.SetPixels(colorArray);
        _myColorTexture.Apply();
    }

    private void DoMyWindow(int windowId)
    {
        const int offsetX = 10;
        var offsetY = 30;


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Metallic Multiplier", _materialSettings.Metallic,
            _materialSettings.MetallicText,
            out _materialSettings.Metallic, out _materialSettings.MetallicText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Smoothness Multiplier", _materialSettings.Smoothness,
            _materialSettings.SmoothnessText, out _materialSettings.Smoothness, out _materialSettings.SmoothnessText,
            0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Paralax Displacement", _materialSettings.Parallax,
            _materialSettings.ParallaxText,
            out _materialSettings.Parallax, out _materialSettings.ParallaxText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Edge Amount", _materialSettings.EdgePower,
            _materialSettings.EdgePowerText,
            out _materialSettings.EdgePower, out _materialSettings.EdgePowerText, 0.0f, 2.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Ambient Occlusion Power", _materialSettings.AoPower,
            _materialSettings.AoPowerText,
            out _materialSettings.AoPower, out _materialSettings.AoPowerText, 0.0f, 2.0f);
        offsetY += 40;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Light Color");
        ChooseLightColor(offsetX, offsetY + 20);
        offsetY += 160;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling X", _materialSettings.TexTilingX,
            _materialSettings.TexTilingXText,
            out _materialSettings.TexTilingX, out _materialSettings.TexTilingXText, 0.1f, 5.0f);
        offsetY += 30;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling Y", _materialSettings.TexTilingY,
            _materialSettings.TexTilingYText,
            out _materialSettings.TexTilingY, out _materialSettings.TexTilingYText, 0.1f, 5.0f);
        offsetY += 50;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset X", _materialSettings.TexOffsetX,
            _materialSettings.TexOffsetXText,
            out _materialSettings.TexOffsetX, out _materialSettings.TexOffsetXText, -1.0f, 1.0f);
        offsetY += 30;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset Y", _materialSettings.TexOffsetY,
            _materialSettings.TexOffsetYText,
            out _materialSettings.TexOffsetY, out _materialSettings.TexOffsetYText, -1.0f, 1.0f);
        offsetY += 50;

        if (GUI.Button(new Rect(offsetX, offsetY, 60, 30), "Plane"))
        {
            _planeShown = true;
            _cubeShown = false;
            _cylinderShown = false;
            _sphereShown = false;
            _dispOffset = 1.0f;
            Shader.DisableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 70, offsetY, 60, 30), "Cube"))
        {
            _planeShown = false;
            _cubeShown = true;
            _cylinderShown = false;
            _sphereShown = false;
            _dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 140, offsetY, 70, 30), "Cylinder"))
        {
            _planeShown = false;
            _cubeShown = false;
            _cylinderShown = true;
            _sphereShown = false;
            _dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 220, offsetY, 60, 30), "Sphere"))
        {
            _planeShown = false;
            _cubeShown = false;
            _cylinderShown = false;
            _sphereShown = true;
            _dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = 590;

        _windowRect = GUI.Window(14, _windowRect, DoMyWindow, "Full Material");
    }

    public void Initialize()
    {
        InitializeSettings();

        _mainGuiScript = MainGui.Instance;
        _thisMaterial = _mainGuiScript.FullMaterial;

        _thisMaterial.SetTexture(DisplacementMap, _mainGuiScript.TextureGrey);
        _thisMaterial.SetTexture(DiffuseMap, _mainGuiScript.TextureGrey);
        _thisMaterial.SetTexture(NormalMap, _mainGuiScript.TextureNormal);
        _thisMaterial.SetTexture(MetallicMap, _mainGuiScript.TextureBlack);
        _thisMaterial.SetTexture(SmoothnessMap, _mainGuiScript.TextureGrey);
        _thisMaterial.SetTexture(AoMap, _mainGuiScript.TextureWhite);
        _thisMaterial.SetTexture(EdgeMap, _mainGuiScript.TextureGrey);
        _thisMaterial.SetFloat(DispOffset, 1.0f);

        _heightMap = _mainGuiScript.HeightMap;

        _diffuseMap = _mainGuiScript.DiffuseMap != null ? _mainGuiScript.DiffuseMap : _mainGuiScript.DiffuseMapOriginal;
        _normalMap = _mainGuiScript.NormalMap;
        _edgeMap = _mainGuiScript.EdgeMap;
        _metallicMap = _mainGuiScript.MetallicMap;
        _smoothnessMap = _mainGuiScript.SmoothnessMap;
        _aoMap = _mainGuiScript.AoMap;

        if (_heightMap != null) _thisMaterial.SetTexture(DisplacementMap, _heightMap);
        if (_diffuseMap != null) _thisMaterial.SetTexture(DiffuseMap, _diffuseMap);
        if (_normalMap != null) _thisMaterial.SetTexture(NormalMap, _normalMap);
        if (_metallicMap != null) _thisMaterial.SetTexture(MetallicMap, _metallicMap);
        if (_smoothnessMap != null) _thisMaterial.SetTexture(SmoothnessMap, _smoothnessMap);
        if (_aoMap != null) _thisMaterial.SetTexture(AoMap, _aoMap);
        if (_edgeMap != null) _thisMaterial.SetTexture(EdgeMap, _edgeMap);

        TestObject.GetComponent<Renderer>().material = _thisMaterial;
        TestObjectCube.GetComponent<Renderer>().material = _thisMaterial;
        TestObjectCylinder.GetComponent<Renderer>().material = _thisMaterial;
        TestObjectSphere.GetComponent<Renderer>().material = _thisMaterial;
    }
}