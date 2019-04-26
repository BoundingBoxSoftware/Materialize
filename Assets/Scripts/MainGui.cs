#region

using System;
using System.Collections.Generic;
using System.Linq;
using SFB;
using UnityEngine;
using UnityEngine.UI;


#endregion

public class MainGui : MonoBehaviour
{
    #region "Vars"
    private const float GamaCorrection = 2.2f;


    public static MainGui Instance;
    public GameObject Modle;
    public static readonly string[] LoadFormats =
    {
        "png", "jpg", "jpeg", "tga", "bmp", "exr"
    };
    private bool _doOnce;
    private static readonly int CorrectionId = Shader.PropertyToID("_GamaCorrection");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int GlobalCubemapId = Shader.PropertyToID("_GlobalCubemap");
    private static readonly int DisplacementMapId = Shader.PropertyToID("_DisplacementMap");
    private static readonly int DiffuseMapId = Shader.PropertyToID("_DiffuseMap");
    private static readonly int NormalMapId = Shader.PropertyToID("_NormalMap");
    private static readonly int MetallicMapId = Shader.PropertyToID("_MetallicMap");
    private static readonly int SmoothnessMapId = Shader.PropertyToID("_SmoothnessMap");
    private static readonly int AoMapId = Shader.PropertyToID("_AOMap");
    private static readonly int EdgeMapId = Shader.PropertyToID("_EdgeMap");
    private static readonly int TilingId = Shader.PropertyToID("_Tiling");

    private readonly ExtensionFilter[] _imageLoadFilter =
    {
        new ExtensionFilter("Image Files", LoadFormats)
    };

    private readonly ExtensionFilter[] _imageSaveFilter =
    {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg", "tga", "exr")
    };

    private MapType _activeMapType;

    private bool _busySaving;

    private bool _clearTextures;
    public bool _exrSelected { get; set; }
    public bool _pngSelected { get; set; }
    public bool _tgaSelected { get; set; }
    public bool _jpgSelected { get; set; }


    public string _lastDirectory = "";

    private List<GameObject> _objectsToUnhide;
    public char _pathChar = '/';

    private bool _propBlueChoose;
    private Material _propertyCompMaterial;

    private Shader _propertyCompShader;
    private bool _propGreenChoose;
    private bool _propRedChoose;
    private bool _propAlphaChoose;
    public SaveLoadProject _saveLoadProjectScript;
    private int _selectedCubemap;
    private SettingsGui _settingsGuiScript;

    private Texture2D _textureToLoad;
    private Texture2D _textureToSave;


    private Material _thisMaterial;
    private TilingTextureMakerGui _tilingTextureMakerGuiScript;

    public AlignmentGui AlignmentGuiScript;

    public GameObject AoFromNormalGuiObject;
    public AoFromNormalGui AoFromNormalGuiScript;
    public Texture2D AoMap;

    public Cubemap[] CubeMaps;
    public Texture2D DiffuseMap;
    public Texture2D DiffuseMapOriginal;

    public GameObject EdgeFromNormalGuiObject;
    public EdgeFromNormalGui EdgeFromNormalGuiScript;
    public Texture2D EdgeMap;

    public GameObject EditDiffuseGuiObject;
    public EditDiffuseGui EditDiffuseGuiScript;
    public Material FullMaterial;

    public Material FullMaterialRef;

    public RenderTexture HdHeightMap;

    public GameObject HeightFromDiffuseGuiObject;
    public HeightFromDiffuseGui HeightFromDiffuseGuiScript;
    public Texture2D HeightMap;

    public GameObject MaterialGuiObject;
    public MaterialGui MaterialGuiScript;

    public GameObject MetallicGuiObject;
    public MetallicGui MetallicGuiScript;
    public Texture2D MetallicMap;

    public GameObject NormalFromHeightGuiObject;
    public NormalFromHeightGui NormalFromHeightGuiScript;
    public Texture2D NormalMap;

    public GameObject PostProcessGuiObject;
    public PropChannelMap PropBlue = PropChannelMap.None;
    public Texture2D TextureBlue = null;
    public Texture2D PropertyMap;
    public PropChannelMap PropGreen = PropChannelMap.None;
    public Texture2D TextureGreen = null;
    public PropChannelMap PropRed = PropChannelMap.None;
    public Texture2D TextureRed = null;
    public PropChannelMap PropAlpha = PropChannelMap.None;
    public Texture2D TextureAlpha = null;

    public string QuicksavePathAo = "";
    public string QuicksavePathDiffuse = "";
    public string QuicksavePathEdge = "";
    public string QuicksavePathHeight = "";
    public string QuicksavePathMetallic = "";
    public string QuicksavePathNormal = "";
    public string QuicksavePathProperty = "";
    public string QuicksavePathSmoothness = "";

    //public Material skyboxMaterial;
    public ReflectionProbe ReflectionProbe;
    [HideInInspector] public Material SampleMaterial;
    public Material SampleMaterialRef;
    public Material SkyboxMaterial;

    public GameObject SaveLoadProjectObject;
    public FileFormat SelectedFormat;

    public GameObject SettingsGuiObject;

    public GameObject SmoothnessGuiObject;
    public SmoothnessGui SmoothnessGuiScript;
    public Texture2D SmoothnessMap;
    public GameObject TestObject;

    public Texture2D TextureBlack;
    public Texture2D TextureGrey;
    public Texture2D TextureNormal;
    public Texture2D TextureWhite;

    public GameObject TilingTextureMakerGuiObject;
    public BatchUI Batchui;

    //public string XSize = "1024";
    public string XSize { get; set; }
    public string YSize { get; set; }
    public bool ScaleTexture;
    private bool ScaleTextureLocked;
    public Toggle[] FileFormatToggles;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Batchui = gameObject.AddComponent<BatchUI>();
        _lastDirectory = Application.dataPath;

        HeightMap = null;
        HdHeightMap = null;
        DiffuseMap = null;
        DiffuseMapOriginal = null;
        NormalMap = null;
        MetallicMap = null;
        SmoothnessMap = null;
        EdgeMap = null;
        AoMap = null;

        _propertyCompShader = Shader.Find("Hidden/Blit_Property_Comp");
        _propertyCompMaterial = new Material(_propertyCompShader);

        Shader.SetGlobalFloat(CorrectionId, GamaCorrection);

        FullMaterial = new Material(FullMaterialRef.shader);
        FullMaterial.CopyPropertiesFromMaterial(FullMaterialRef);

        SampleMaterial = new Material(SampleMaterialRef.shader);
        SampleMaterial.CopyPropertiesFromMaterial(SampleMaterialRef);

        HeightFromDiffuseGuiScript = HeightFromDiffuseGuiObject.GetComponent<HeightFromDiffuseGui>();
        NormalFromHeightGuiScript = NormalFromHeightGuiObject.GetComponent<NormalFromHeightGui>();
        EdgeFromNormalGuiScript = EdgeFromNormalGuiObject.GetComponent<EdgeFromNormalGui>();
        AoFromNormalGuiScript = AoFromNormalGuiObject.GetComponent<AoFromNormalGui>();
        EditDiffuseGuiScript = EditDiffuseGuiObject.GetComponent<EditDiffuseGui>();
        MetallicGuiScript = MetallicGuiObject.GetComponent<MetallicGui>();
        SmoothnessGuiScript = SmoothnessGuiObject.GetComponent<SmoothnessGui>();
        MaterialGuiScript = MaterialGuiObject.GetComponent<MaterialGui>();
        _tilingTextureMakerGuiScript = TilingTextureMakerGuiObject.GetComponent<TilingTextureMakerGui>();
        _saveLoadProjectScript = SaveLoadProjectObject.GetComponent<SaveLoadProject>();
        _settingsGuiScript = SettingsGuiObject.GetComponent<SettingsGui>();

        _settingsGuiScript.LoadSettings();

        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer)
            _pathChar = '\\';

        TestObject.GetComponent<Renderer>().material = FullMaterial;
        SetMaterialValues();

        ReflectionProbe.RenderProbe();

        HideGuiLocker.LockEmpty += LoadHideState;
    }

    public void SaveHideState()
    {
        if (HideGuiLocker.IsLocked) return;
        _lastGuiIsHiddenState = IsGuiHidden;
    }

    public void SaveHideStateAndHideAndLock(object sender)
    {
        SaveHideState();
        IsGuiHidden = true;
        HideGuiLocker.Lock(sender);
    }

    private void LoadHideState(object sender, EventArgs eventArgs)
    {
        IsGuiHidden = _lastGuiIsHiddenState;
    }

    public void SetPreviewMaterial(Texture2D textureToPreview)
    {
        CloseWindows();
        if (textureToPreview == null) return;
        FixSizeMap(textureToPreview);
        SampleMaterial.SetTexture(MainTexId, textureToPreview);
        TestObject.GetComponent<Renderer>().material = SampleMaterial;
    }

    public void SetPreviewMaterial(RenderTexture textureToPreview)
    {
        CloseWindows();
        if (textureToPreview == null) return;
        FixSizeMap(textureToPreview);
        SampleMaterial.SetTexture(MainTexId, textureToPreview);
        TestObject.GetComponent<Renderer>().material = SampleMaterial;
    }

    public void SetMaterialValues()
    {
        Shader.SetGlobalTexture(GlobalCubemapId, CubeMaps[_selectedCubemap]);

        FullMaterial.SetTexture(DisplacementMapId, HeightMap != null ? HeightMap : TextureGrey);

        if (DiffuseMap != null)
            FullMaterial.SetTexture(DiffuseMapId, DiffuseMap);
        else if (DiffuseMapOriginal != null)
            FullMaterial.SetTexture(DiffuseMapId, DiffuseMapOriginal);
        else
            FullMaterial.SetTexture(DiffuseMapId, TextureGrey);

        FullMaterial.SetTexture(NormalMapId, NormalMap != null ? NormalMap : TextureNormal);

        FullMaterial.SetTexture(MetallicMapId, MetallicMap != null ? MetallicMap : TextureBlack);

        FullMaterial.SetTexture(SmoothnessMapId, SmoothnessMap != null ? SmoothnessMap : TextureBlack);

        FullMaterial.SetTexture(AoMapId, AoMap != null ? AoMap : TextureWhite);

        FullMaterial.SetTexture(EdgeMapId, EdgeMap != null ? EdgeMap : TextureGrey);

        TestObject.GetComponent<Renderer>().material = FullMaterial;

        FullMaterial.SetVector(TilingId, new Vector4(1, 1, 0, 0));
    }

    public void SetFileFormat(int selection)
    {
        switch (selection)
        {

            case 0:
                SelectedFormat = FileFormat.Png;
                break;
            case 1:
                SelectedFormat = FileFormat.Jpg;
                break;
            case 2:
                SelectedFormat = FileFormat.Exr;
                break;
            case 3:
                SelectedFormat = FileFormat.Tga;
                break;
            default:
                break;
        }
    }
    public void CloseWindows()
    {
        HeightFromDiffuseGuiScript.Close();
        NormalFromHeightGuiScript.Close();
        EdgeFromNormalGuiScript.Close();
        AoFromNormalGuiScript.Close();
        EditDiffuseGuiScript.Close();
        MetallicGuiScript.Close();
        SmoothnessGuiScript.Close();
        _tilingTextureMakerGuiScript.Close();
        AlignmentGuiScript.Close();
        MaterialGuiObject.SetActive(false);
        PostProcessGuiObject.SetActive(false);
    }

    private void HideWindows()
    {
        _objectsToUnhide = new List<GameObject>();

        if (HeightFromDiffuseGuiObject.activeSelf) _objectsToUnhide.Add(HeightFromDiffuseGuiObject);

        if (NormalFromHeightGuiObject.activeSelf) _objectsToUnhide.Add(NormalFromHeightGuiObject);

        if (EdgeFromNormalGuiObject.activeSelf) _objectsToUnhide.Add(EdgeFromNormalGuiObject);

        if (AoFromNormalGuiObject.activeSelf) _objectsToUnhide.Add(AoFromNormalGuiObject);

        if (EditDiffuseGuiObject.activeSelf) _objectsToUnhide.Add(EditDiffuseGuiObject);

        if (MetallicGuiObject.activeSelf) _objectsToUnhide.Add(MetallicGuiObject);

        if (SmoothnessGuiObject.activeSelf) _objectsToUnhide.Add(SmoothnessGuiObject);

        if (MaterialGuiObject.activeSelf) _objectsToUnhide.Add(MaterialGuiObject);

        if (PostProcessGuiObject.activeSelf) _objectsToUnhide.Add(PostProcessGuiObject);

        if (TilingTextureMakerGuiObject.activeSelf) _objectsToUnhide.Add(TilingTextureMakerGuiObject);

        HeightFromDiffuseGuiObject.SetActive(false);
        NormalFromHeightGuiObject.SetActive(false);
        EdgeFromNormalGuiObject.SetActive(false);
        AoFromNormalGuiObject.SetActive(false);
        EditDiffuseGuiObject.SetActive(false);
        MetallicGuiObject.SetActive(false);
        SmoothnessGuiObject.SetActive(false);
        MaterialGuiObject.SetActive(false);
        PostProcessGuiObject.SetActive(false);
        TilingTextureMakerGuiObject.SetActive(false);
    }

    private static void Fullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void Quit()
    {
        Application.Quit();
    }
    public void BatchLoadTextures()
    {
        Batchui.BatchLoadTextures();
    }
    public void Windowed()
    {
        Fullscreen();
    }
    public void UseInitalLocation(bool Bool)
    {
        Batchui.UseInitalLocation = Bool;
    }
    public void ProcessPropertyMap(bool Bool)
    {
        Batchui.ProcessPropertyMap = Bool;
    }
    private void OnGUI()
    {
        #region Unhideable Buttons

        //==================================================//
        // 					Unhidable Buttons				//
        //==================================================//
        /*
        if (GUI.Button(new Rect(Screen.width - 80, Screen.height - 40, 70, 30), "Quit")) Application.Quit();

        if (GUI.Button(new Rect(Screen.width - 480, Screen.height - 40, 100, 30), "Batch Textures"))
        {
            
            Batchui.BatchLoadTextures();
        }

        Batchui.UseInitalLocation = GUI.Toggle(new Rect(Screen.width - 480, Screen.height - 60, 140, 20), Batchui.UseInitalLocation, "Use Inital Location");
        Batchui.ProcessPropertyMap = GUI.Toggle(new Rect(Screen.width - 480, Screen.height - 80, 140, 20), Batchui.ProcessPropertyMap, "Use Property Map");

        GUI.enabled = false;
        if (Screen.fullScreen)
        {
            if (GUI.Button(new Rect(Screen.width - 190, Screen.height - 40, 100, 30), "Windowed")) Fullscreen();
        }
        else
        {
            if (GUI.Button(new Rect(Screen.width - 190, Screen.height - 40, 100, 30), "Full Screen")) Fullscreen();
        }

        GUI.enabled = true;

//        if (GUI.Button(new Rect(Screen.width - 260, 10, 140, 30), "Make Suggestion"))
//            SuggestionGuiObject.SetActive(true);
*/
        if (IsGuiHidden)
        {
            if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Show Gui"))
            {
                IsGuiHidden = false;
            }
            else return;
        }
        else
        {
            if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Hide Gui"))
            {
                IsGuiHidden = true;
            }
        }

        #endregion

        #region Main Gui

        //==================================================//
        // 						Main Gui					//
        //==================================================//


        const int spacingX = 130;

        var offsetX = 20;
        var offsetY = 20;

        #region"HeightMap"
        //==============================//
        // 			Height Map			//
        //==============================//

        GUI.Box(new Rect(offsetX, offsetY, 110, 250), "Height Map");

        if (HeightMap != null) GUI.DrawTexture(new Rect(offsetX + 5, offsetY + 25, 100, 100), HeightMap);

        // Paste 
        if (GUI.Button(new Rect(offsetX + 5, offsetY + 130, 20, 20), "P"))
        {
            _activeMapType = MapType.Height;
            PasteFile();
        }

        GUI.enabled = HeightMap != null;

        // Copy
        if (GUI.Button(new Rect(offsetX + 30, offsetY + 130, 20, 20), "C"))
        {
            _textureToSave = HeightMap;
            CopyFile();
        }

        GUI.enabled = true;

        // Open
        if (GUI.Button(new Rect(offsetX + 60, offsetY + 130, 20, 20), "O")) OpenTextureFile(MapType.Height);

        GUI.enabled = HeightMap != null;

        // Save
        if (GUI.Button(new Rect(offsetX + 85, offsetY + 130, 20, 20), "S")) SaveTextureFile(MapType.Height);


        if (HeightMap == null || QuicksavePathHeight == "")
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Quick Save
        if (GUI.Button(new Rect(offsetX + 15, offsetY + 160, 80, 20), "Quick Save"))
        {
            _textureToSave = HeightMap;
            SaveFile(QuicksavePathProperty);
        }

        GUI.enabled = HeightMap != null;

        if (GUI.Button(new Rect(offsetX + 15, offsetY + 190, 80, 20), "Preview")) SetPreviewMaterial(HeightMap);

        GUI.enabled = true;

        if (DiffuseMapOriginal == null && DiffuseMap == null && NormalMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        if (GUI.Button(new Rect(offsetX + 5, offsetY + 220, 50, 20), "Create"))
        {
            CloseWindows();
            FixSize();
            HeightFromDiffuseGuiObject.SetActive(true);
            HeightFromDiffuseGuiScript.NewTexture();
            HeightFromDiffuseGuiScript.DoStuff();
        }

        GUI.enabled = true;

        GUI.enabled = HeightMap != null;

        if (GUI.Button(new Rect(offsetX + 60, offsetY + 220, 45, 20), "Clear"))
        {
            ClearTexture(MapType.Height);
            CloseWindows();
            SetMaterialValues();
            FixSize();
        }

        GUI.enabled = true;

        #endregion
        #region "Diffuse Map"
        //==============================//
        // 			Diffuse Map			//
        //==============================//

        GUI.Box(new Rect(offsetX + spacingX, offsetY, 110, 250), "Diffuse Map");

        if (DiffuseMap != null)
            GUI.DrawTexture(new Rect(offsetX + spacingX + 5, offsetY + 25, 100, 100), DiffuseMap);
        else if (DiffuseMapOriginal != null)
            GUI.DrawTexture(new Rect(offsetX + spacingX + 5, offsetY + 25, 100, 100), DiffuseMapOriginal);

        // Paste 
        if (GUI.Button(new Rect(offsetX + spacingX + 5, offsetY + 130, 20, 20), "P"))
        {
            _activeMapType = MapType.DiffuseOriginal;
            PasteFile();
        }

        if (DiffuseMapOriginal == null && DiffuseMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Copy
        if (GUI.Button(new Rect(offsetX + spacingX + 30, offsetY + 130, 20, 20), "C"))
        {
            _textureToSave = DiffuseMap != null ? DiffuseMap : DiffuseMapOriginal;

            CopyFile();
        }

        GUI.enabled = true;

        // Open
        if (GUI.Button(new Rect(offsetX + spacingX + 60, offsetY + 130, 20, 20), "O"))
            OpenTextureFile(MapType.DiffuseOriginal);

        if (DiffuseMapOriginal == null && DiffuseMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Save
        if (GUI.Button(new Rect(offsetX + spacingX + 85, offsetY + 130, 20, 20), "S")) SaveTextureFile(MapType.Diffuse);

        if (DiffuseMapOriginal == null && DiffuseMap == null || QuicksavePathDiffuse == "")
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Quick Save
        if (GUI.Button(new Rect(offsetX + spacingX + 15, offsetY + 160, 80, 20), "Quick Save"))
        {
            _textureToSave = DiffuseMap != null ? DiffuseMap : DiffuseMapOriginal;

            SaveFile(QuicksavePathDiffuse);
        }

        if (DiffuseMapOriginal == null && DiffuseMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        if (GUI.Button(new Rect(offsetX + spacingX + 15, offsetY + 190, 80, 20), "Preview"))
        {
            SetPreviewMaterial(DiffuseMap != null ? DiffuseMap : DiffuseMapOriginal);
        }

        GUI.enabled = DiffuseMapOriginal != null;

        if (GUI.Button(new Rect(offsetX + spacingX + 5, offsetY + 220, 50, 20), "Edit"))
        {
            CloseWindows();
            FixSize();
            EditDiffuseGuiObject.SetActive(true);
            EditDiffuseGuiScript.NewTexture();
            EditDiffuseGuiScript.DoStuff();
        }

        if (DiffuseMapOriginal == null && DiffuseMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        if (GUI.Button(new Rect(offsetX + spacingX + 60, offsetY + 220, 45, 20), "Clear"))
        {
            ClearTexture(MapType.Diffuse);
            CloseWindows();
            SetMaterialValues();
            FixSize();
        }

        GUI.enabled = true;
        #endregion
        #region"NormalMap"
        //==============================//
        // 			Normal Map			//
        //==============================//

        GUI.Box(new Rect(offsetX + spacingX * 2, offsetY, 110, 250), "Normal Map");

        if (NormalMap != null)
            GUI.DrawTexture(new Rect(offsetX + spacingX * 2 + 5, offsetY + 25, 100, 100), NormalMap);

        // Paste 
        if (GUI.Button(new Rect(offsetX + spacingX * 2 + 5, offsetY + 130, 20, 20), "P"))
        {
            _activeMapType = MapType.Normal;
            PasteFile();
        }

        GUI.enabled = NormalMap != null;

        // Copy
        if (GUI.Button(new Rect(offsetX + spacingX * 2 + 30, offsetY + 130, 20, 20), "C"))
        {
            _textureToSave = NormalMap;
            CopyFile();
        }

        GUI.enabled = true;

        //Open
        if (GUI.Button(new Rect(offsetX + spacingX * 2 + 60, offsetY + 130, 20, 20), "O"))
            OpenTextureFile(MapType.Normal);

        GUI.enabled = NormalMap != null;

        // Save
        if (GUI.Button(new Rect(offsetX + spacingX * 2 + 85, offsetY + 130, 20, 20), "S"))
            SaveTextureFile(MapType.Normal);

        if (NormalMap == null || QuicksavePathNormal == "")
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Quick Save
        if (GUI.Button(new Rect(offsetX + spacingX * 2 + 15, offsetY + 160, 80, 20), "Quick Save"))
        {
            _textureToSave = NormalMap;
            SaveFile(QuicksavePathNormal);
        }

        GUI.enabled = NormalMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 2 + 15, offsetY + 190, 80, 20), "Preview"))
            SetPreviewMaterial(NormalMap);

        GUI.enabled = HeightMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 2 + 5, offsetY + 220, 50, 20), "Create"))
        {
            CloseWindows();
            FixSize();
            NormalFromHeightGuiObject.SetActive(true);
            NormalFromHeightGuiScript.NewTexture();
            NormalFromHeightGuiScript.DoStuff();
        }

        GUI.enabled = NormalMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 2 + 60, offsetY + 220, 45, 20), "Clear"))
        {
            ClearTexture(MapType.Normal);
            CloseWindows();
            SetMaterialValues();
            FixSize();
        }

        GUI.enabled = true;
        #endregion
        #region"MetallicMap"
        //==============================//
        // 			Metallic Map		//
        //==============================//

        GUI.Box(new Rect(offsetX + spacingX * 3, offsetY, 110, 250), "Metallic Map");

        if (MetallicMap != null)
            GUI.DrawTexture(new Rect(offsetX + spacingX * 3 + 5, offsetY + 25, 100, 100), MetallicMap);

        // Paste 
        if (GUI.Button(new Rect(offsetX + spacingX * 3 + 5, offsetY + 130, 20, 20), "P"))
        {
            _activeMapType = MapType.Metallic;
            PasteFile();
        }

        GUI.enabled = MetallicMap != null;

        // Copy
        if (GUI.Button(new Rect(offsetX + spacingX * 3 + 30, offsetY + 130, 20, 20), "C"))
        {
            _textureToSave = MetallicMap;
            CopyFile();
        }

        GUI.enabled = true;

        //Open
        if (GUI.Button(new Rect(offsetX + spacingX * 3 + 60, offsetY + 130, 20, 20), "O"))
            OpenTextureFile(MapType.Metallic);

        GUI.enabled = MetallicMap != null;

        // Save
        if (GUI.Button(new Rect(offsetX + spacingX * 3 + 85, offsetY + 130, 20, 20), "S"))
            SaveTextureFile(MapType.Metallic);

        if (MetallicMap == null || QuicksavePathMetallic == "")
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Quick Save
        if (GUI.Button(new Rect(offsetX + spacingX * 3 + 15, offsetY + 160, 80, 20), "Quick Save"))
        {
            _textureToSave = MetallicMap;
            SaveFile(QuicksavePathMetallic);
        }

        GUI.enabled = MetallicMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 3 + 15, offsetY + 190, 80, 20), "Preview"))
            SetPreviewMaterial(MetallicMap);

        if (DiffuseMapOriginal == null && DiffuseMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        if (GUI.Button(new Rect(offsetX + spacingX * 3 + 5, offsetY + 220, 50, 20), "Create"))
        {
            CloseWindows();
            FixSize();

            MetallicGuiObject.SetActive(true);
            MetallicGuiScript.NewTexture();
            MetallicGuiScript.DoStuff();
        }

        GUI.enabled = MetallicMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 3 + 60, offsetY + 220, 45, 20), "Clear"))
        {
            ClearTexture(MapType.Metallic);
            CloseWindows();
            SetMaterialValues();
            FixSize();
        }

        GUI.enabled = true;
        #endregion
        #region"SmoothnessMap"
        //==============================//
        // 		Smoothness Map			//
        //==============================//

        GUI.Box(new Rect(offsetX + spacingX * 4, offsetY, 110, 250), "Smoothness Map");

        if (SmoothnessMap != null)
            GUI.DrawTexture(new Rect(offsetX + spacingX * 4 + 5, offsetY + 25, 100, 100), SmoothnessMap);

        // Paste 
        if (GUI.Button(new Rect(offsetX + spacingX * 4 + 5, offsetY + 130, 20, 20), "P"))
        {
            _activeMapType = MapType.Smoothness;
            PasteFile();
        }

        GUI.enabled = SmoothnessMap != null;

        // Copy
        if (GUI.Button(new Rect(offsetX + spacingX * 4 + 30, offsetY + 130, 20, 20), "C"))
        {
            _textureToSave = SmoothnessMap;
            CopyFile();
        }

        GUI.enabled = true;

        //Open
        if (GUI.Button(new Rect(offsetX + spacingX * 4 + 60, offsetY + 130, 20, 20), "O"))
            OpenTextureFile(MapType.Smoothness);

        GUI.enabled = SmoothnessMap != null;

        // Save
        if (GUI.Button(new Rect(offsetX + spacingX * 4 + 85, offsetY + 130, 20, 20), "S"))
            SaveTextureFile(MapType.Smoothness);

        if (SmoothnessMap == null || QuicksavePathSmoothness == "")
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Quick Save
        if (GUI.Button(new Rect(offsetX + spacingX * 4 + 15, offsetY + 160, 80, 20), "Quick Save"))
        {
            _textureToSave = SmoothnessMap;
            SaveFile(QuicksavePathSmoothness);
        }

        GUI.enabled = SmoothnessMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 4 + 15, offsetY + 190, 80, 20), "Preview"))
            SetPreviewMaterial(SmoothnessMap);

        if (DiffuseMapOriginal == null && DiffuseMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        if (GUI.Button(new Rect(offsetX + spacingX * 4 + 5, offsetY + 220, 50, 20), "Create"))
        {
            CloseWindows();
            FixSize();
            SmoothnessGuiObject.SetActive(true);
            SmoothnessGuiScript.NewTexture();
            SmoothnessGuiScript.DoStuff();
        }

        GUI.enabled = SmoothnessMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 4 + 60, offsetY + 220, 45, 20), "Clear"))
        {
            ClearTexture(MapType.Smoothness);
            CloseWindows();
            SetMaterialValues();
            FixSize();
        }

        GUI.enabled = true;
        #endregion
        #region"EdgeMap"
        //==============================//
        // 			Edge Map			//
        //==============================//

        GUI.Box(new Rect(offsetX + spacingX * 5, offsetY, 110, 250), "Edge Map");

        if (EdgeMap != null) GUI.DrawTexture(new Rect(offsetX + spacingX * 5 + 5, offsetY + 25, 100, 100), EdgeMap);

        // Paste 
        if (GUI.Button(new Rect(offsetX + spacingX * 5 + 5, offsetY + 130, 20, 20), "P"))
        {
            _activeMapType = MapType.Edge;
            PasteFile();
        }

        GUI.enabled = EdgeMap != null;

        // Copy
        if (GUI.Button(new Rect(offsetX + spacingX * 5 + 30, offsetY + 130, 20, 20), "C"))
        {
            _textureToSave = EdgeMap;
            CopyFile();
        }

        GUI.enabled = true;

        //Open
        if (GUI.Button(new Rect(offsetX + spacingX * 5 + 60, offsetY + 130, 20, 20), "O"))
            OpenTextureFile(MapType.Edge);

        GUI.enabled = EdgeMap != null;

        // Save
        if (GUI.Button(new Rect(offsetX + spacingX * 5 + 85, offsetY + 130, 20, 20), "S"))
            SaveTextureFile(MapType.Edge);

        if (EdgeMap == null || QuicksavePathEdge == "")
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Quick Save
        if (GUI.Button(new Rect(offsetX + spacingX * 5 + 15, offsetY + 160, 80, 20), "Quick Save"))
        {
            _textureToSave = EdgeMap;
            SaveFile(QuicksavePathEdge);
        }

        GUI.enabled = EdgeMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 5 + 15, offsetY + 190, 80, 20), "Preview"))
            SetPreviewMaterial(EdgeMap);

        GUI.enabled = NormalMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 5 + 5, offsetY + 220, 50, 20), "Create"))
        {
            CloseWindows();
            FixSize();
            EdgeFromNormalGuiObject.SetActive(true);
            EdgeFromNormalGuiScript.NewTexture();
            EdgeFromNormalGuiScript.DoStuff();
        }

        GUI.enabled = EdgeMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 5 + 60, offsetY + 220, 45, 20), "Clear"))
        {
            ClearTexture(MapType.Edge);
            CloseWindows();
            SetMaterialValues();
            FixSize();
        }

        GUI.enabled = true;
        #endregion
        #region"AO Map"
        //==============================//
        // 			AO Map				//
        //==============================//

        GUI.Box(new Rect(offsetX + spacingX * 6, offsetY, 110, 250), "AO Map");

        if (AoMap != null) GUI.DrawTexture(new Rect(offsetX + spacingX * 6 + 5, offsetY + 25, 100, 100), AoMap);


        // Paste 
        if (GUI.Button(new Rect(offsetX + spacingX * 6 + 5, offsetY + 130, 20, 20), "P"))
        {
            _activeMapType = MapType.Ao;
            PasteFile();
        }

        GUI.enabled = AoMap != null;

        // Copy
        if (GUI.Button(new Rect(offsetX + spacingX * 6 + 30, offsetY + 130, 20, 20), "C"))
        {
            _textureToSave = AoMap;
            CopyFile();
        }

        GUI.enabled = true;

        //Open
        if (GUI.Button(new Rect(offsetX + spacingX * 6 + 60, offsetY + 130, 20, 20), "O")) OpenTextureFile(MapType.Ao);

        GUI.enabled = AoMap != null;

        // Save
        if (GUI.Button(new Rect(offsetX + spacingX * 6 + 85, offsetY + 130, 20, 20), "S")) SaveTextureFile(MapType.Ao);

        if (AoMap == null || QuicksavePathAo == "")
            GUI.enabled = false;
        else
            GUI.enabled = true;

        // Quick Save
        if (GUI.Button(new Rect(offsetX + spacingX * 6 + 15, offsetY + 160, 80, 20), "Quick Save"))
        {
            _textureToSave = AoMap;
            SaveFile(QuicksavePathAo);
        }

        GUI.enabled = AoMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 6 + 15, offsetY + 190, 80, 20), "Preview"))
            SetPreviewMaterial(AoMap);

        if (NormalMap == null && HeightMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        if (GUI.Button(new Rect(offsetX + spacingX * 6 + 5, offsetY + 220, 50, 20), "Create"))
        {
            CloseWindows();
            FixSize();
            AoFromNormalGuiObject.SetActive(true);
            AoFromNormalGuiScript.NewTexture();
            AoFromNormalGuiScript.DoStuff();
        }

        GUI.enabled = AoMap != null;

        if (GUI.Button(new Rect(offsetX + spacingX * 6 + 60, offsetY + 220, 45, 20), "Clear"))
        {
            ClearTexture(MapType.Ao);
            CloseWindows();
            SetMaterialValues();
            FixSize();
        }

        GUI.enabled = true;
        #endregion
        #region"Map Saving Options"
        /*
        //==============================//
        // 		Map Saving Options		//
        //==============================//

        offsetX = offsetX + spacingX * 7;

        GUI.Box(new Rect(offsetX, offsetY, 230, 270), "Saving Options");

        GUI.Label(new Rect(offsetX + 20, offsetY + 20, 100, 25), "File Format");

        _pngSelected = GUI.Toggle(new Rect(offsetX + 5, offsetY + 45, 45, 20), _pngSelected, "PNG");
        if (_pngSelected) SetFormat(FileFormat.Png);

        _jpgSelected = GUI.Toggle(new Rect(offsetX + 55, offsetY + 45, 40, 20), _jpgSelected, "JPG");
        if (_jpgSelected) SetFormat(FileFormat.Jpg);

        _tgaSelected = GUI.Toggle(new Rect(offsetX + 5, offsetY + 65, 45, 20), _tgaSelected, "TGA");
        if (_tgaSelected) SetFormat(FileFormat.Tga);

        _exrSelected = GUI.Toggle(new Rect(offsetX + 55, offsetY + 65, 40, 20), _exrSelected, "EXR");
        if (_exrSelected) SetFormat(FileFormat.Exr);

        //File Size

        GUI.Label(new Rect(offsetX + 20, offsetY + 85, 100, 25), "Texture Size");


        ScaleTexture = GUI.Toggle(new Rect(offsetX + 5, offsetY + 105, 100, 20), ScaleTexture, "Custom Size");
        //if (_exrSelected) SetFormat(FileFormat.Exr);

        GUI.Label(new Rect(offsetX + 15, offsetY + 125, 100, 25), "X:");
        

        XSize = GUI.TextArea(new Rect(offsetX + 30, offsetY + 125, 50, 20), XSize, 200);

        ScaleTextureLocked = GUI.Toggle(new Rect(offsetX + 80, offsetY + 133, 20, 20), ScaleTextureLocked, "L");
        if (ScaleTextureLocked)
        {
            if (DiffuseMapOriginal != null)
            {
                YSize = GUI.TextArea(new Rect(offsetX + 30, offsetY + 150, 50, 20), YSize = LockTextureValue(float.Parse(XSize), DiffuseMapOriginal.width, DiffuseMapOriginal.height).ToString(), 200);
            }
            else
            {
                YSize = GUI.TextArea(new Rect(offsetX + 30, offsetY + 150, 50, 20), YSize, 200);
            }
        }
        else
        {
            YSize = GUI.TextArea(new Rect(offsetX + 30, offsetY + 150, 50, 20), YSize, 200);
        }
        GUI.Label(new Rect(offsetX + 15, offsetY + 150, 100, 25), "Y:");

        

        // Flip Normal Map Y
        GUI.enabled = NormalMap != null;

        if (GUI.Button(new Rect(offsetX + 10, offsetY + 180, 100, 25), "Flip Normal Y")) FlipNormalMapY();

        GUI.enabled = true;

        //Save Project
        if (GUI.Button(new Rect(offsetX + 10, offsetY + 208, 100, 25), "Save Project"))
        {
            SaveProject();
            
        }

        //Load Project
        if (GUI.Button(new Rect(offsetX + 10, offsetY + 235, 100, 25), "Load Project"))
        {
            LoadProject();
        }
        #endregion
        #region"Property Map"
        //======================================//
        //			Property Map Settings		//
        //======================================//

        GUI.Label(new Rect(offsetX + 130, offsetY + 20, 100, 25), "Property Map");

        GUI.enabled = !_propRedChoose;

        GUI.Label(new Rect(offsetX + 100, offsetY + 45, 20, 20), "R:");
        if (GUI.Button(new Rect(offsetX + 120, offsetY + 45, 100, 25), PCM2String(PropRed, "Red None")))
        {
            _propRedChoose = true;
            _propGreenChoose = false;
            _propBlueChoose = false;
            _propAlphaChoose = false;
        }

        GUI.enabled = !_propGreenChoose;

        GUI.Label(new Rect(offsetX + 100, offsetY + 80, 20, 20), "G:");
        if (GUI.Button(new Rect(offsetX + 120, offsetY + 80, 100, 25), PCM2String(PropGreen, "Green None")))
        {
            _propRedChoose = false;
            _propGreenChoose = true;
            _propBlueChoose = false;
            _propAlphaChoose = false;
        }

        GUI.enabled = !_propBlueChoose;

        GUI.Label(new Rect(offsetX + 100, offsetY + 115, 20, 20), "B:");
        if (GUI.Button(new Rect(offsetX + 120, offsetY + 115, 100, 25), PCM2String(PropBlue, "Blue None")))
        {
            _propRedChoose = false;
            _propGreenChoose = false;
            _propBlueChoose = true;
            _propAlphaChoose = false;
        }

        GUI.enabled = !_propAlphaChoose;

        GUI.Label(new Rect(offsetX + 100, offsetY + 150, 20, 20), "A:");
        if (GUI.Button(new Rect(offsetX + 120, offsetY + 150, 100, 25), PCM2String(PropAlpha, "Alpha None")))
        {
            _propRedChoose = false;
            _propGreenChoose = false;
            _propBlueChoose = false;
            _propAlphaChoose = true;
        }

        GUI.enabled = true;

        var propBoxOffsetX = offsetX + 250;
        const int propBoxOffsetY = 20;
        if (_propRedChoose || _propGreenChoose || _propBlueChoose || _propAlphaChoose)
        {
            GUI.Box(new Rect(propBoxOffsetX, propBoxOffsetY, 150, 245), "Map for Channel");
            var chosen = false;
            var chosenPcm = PropChannelMap.None;
            Texture2D ChosenTexture = null;
            if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 30, 130, 25), "None"))
            {
                chosen = true;
                chosenPcm = PropChannelMap.None;
            }

            if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 60, 130, 25), "Height"))
            {
                chosen = true;
                chosenPcm = PropChannelMap.Height;
                ChosenTexture = HeightMap;
            }

            if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 90, 130, 25), "Metallic"))
            {
                chosen = true;
                chosenPcm = PropChannelMap.Metallic;
                ChosenTexture = MetallicMap;
            }

            if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 120, 130, 25), "Smoothness"))
            {
                chosen = true;
                chosenPcm = PropChannelMap.Smoothness;
                ChosenTexture = SmoothnessMap;
            }

            if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 150, 130, 25), "Edge"))
            {
                chosen = true;
                chosenPcm = PropChannelMap.Edge;
                ChosenTexture = EdgeMap;
            }

            if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 180, 130, 25), "Ambient Occlusion"))
            {
                chosen = true;
                chosenPcm = PropChannelMap.Ao;
                ChosenTexture = AoMap;
            }

            if (GUI.Button(new Rect(propBoxOffsetX + 10, propBoxOffsetY + 210, 130, 25), "AO + Edge"))
            {
                chosen = true;
                chosenPcm = PropChannelMap.AoEdge;
                ChosenTexture = AoMap;
            }

            if (chosen)
            {
                if (_propRedChoose) { PropRed = chosenPcm; TextureRed = ChosenTexture; }

                if (_propGreenChoose) { PropGreen = chosenPcm; TextureGreen = ChosenTexture; }

                if (_propBlueChoose) { PropBlue = chosenPcm; TextureBlue = ChosenTexture; }

                if (_propAlphaChoose) { PropAlpha = chosenPcm; TextureAlpha = ChosenTexture; }

                _propRedChoose = false;
                _propGreenChoose = false;
                _propBlueChoose = false;
                _propAlphaChoose = false;
            }
        }

        if (GUI.Button(new Rect(offsetX + 120, offsetY + 180, 100, 40), "Save\r\nProperty Map"))
        {
            if (PropAlpha == PropChannelMap.None)
            {
                ProcessPropertyMap();
            }
            else
            {
                ProcessPropertyMapRevised();
            }
            SaveTextureFile(MapType.Property);
        }

        if (QuicksavePathProperty == "") GUI.enabled = false;

        if (GUI.Button(new Rect(offsetX + 120, offsetY + 220, 100, 40), "Quick Save\r\nProperty Map"))
        {
            ProcessPropertyMap();
            _textureToSave = PropertyMap;
            SaveFile(QuicksavePathProperty);
        }

        GUI.enabled = true;
        #endregion

        //==========================//
        // 		View Buttons		//
        //==========================//

        offsetX = 430;
        offsetY = 300;
        /*
        if (GUI.Button(new Rect(offsetX, offsetY, 100, 40), "Post Process"))
        {
            PostProcessGuiObject.SetActive(!PostProcessGuiObject.activeSelf);
        }

        offsetX += 110;

        if (GUI.Button(new Rect(offsetX, offsetY, 80, 40), "Show Full\r\nMaterial"))
        {
            CloseWindows();
            FixSize();
            MaterialGuiObject.SetActive(true);
            MaterialGuiScript.Initialize();
        }

        offsetX += 90;

        if (GUI.Button(new Rect(offsetX, offsetY, 80, 40), "Next\r\nCube Map"))
        {
            _selectedCubemap += 1;
            if (_selectedCubemap >= CubeMaps.Length) _selectedCubemap = 0;

            //skyboxMaterial.SetTexture ("_Tex", CubeMaps[selectedCubemap] );
            Shader.SetGlobalTexture(GlobalCubemapId, CubeMaps[_selectedCubemap]);
            ReflectionProbe.RenderProbe();
        }

        offsetX += 90;

        GUI.enabled = HeightMap != null;

        if (GUI.Button(new Rect(offsetX, offsetY, 60, 40), "Tile\r\nMaps"))
        {
            CloseWindows();
            FixSize();
            TilingTextureMakerGuiObject.SetActive(true);
            _tilingTextureMakerGuiScript.Initialize();
        }

        GUI.enabled = true;

        offsetX += 70;

        if (HeightMap == null && DiffuseMapOriginal == null && MetallicMap == null && SmoothnessMap == null &&
            EdgeMap == null && AoMap == null)
            GUI.enabled = false;
        else
            GUI.enabled = true;

        if (GUI.Button(new Rect(offsetX, offsetY, 90, 40), "Adjust\r\nAlignment"))
        {
            CloseWindows();
            FixSize();
            AlignmentGuiScript.Initialize();
        }

        GUI.enabled = true;

        offsetX += 100;

        if (GUI.Button(new Rect(offsetX, offsetY, 120, 40), "Clear All\r\nTexture Maps")) _clearTextures = true;

        if (_clearTextures)
        {
            offsetY += 60;

            GUI.Box(new Rect(offsetX, offsetY, 120, 60), "Are You Sure?");

            if (GUI.Button(new Rect(offsetX + 10, offsetY + 30, 45, 20), "Yes"))
            {
                _clearTextures = false;
                ClearAllTextures();
                CloseWindows();
                SetMaterialValues();
                FixSizeSize(1024.0f, 1024.0f);
            }

            if (GUI.Button(new Rect(offsetX + 65, offsetY + 30, 45, 20), "No")) _clearTextures = false;
        }
        */
        GUI.enabled = true;

        #endregion
    }
    public void ShowPostWindow()
    {
        PostProcessGuiObject.SetActive(!PostProcessGuiObject.activeSelf);
    }
    public void TileTextures()
    {
        CloseWindows();
        FixSize();
        TilingTextureMakerGuiObject.SetActive(true);
        _tilingTextureMakerGuiScript.Initialize();
    }
    public void NextCubemap()
    {
        _selectedCubemap += 1;
        if (_selectedCubemap >= CubeMaps.Length) _selectedCubemap = 0;

        SkyboxMaterial.SetTexture("_Tex", CubeMaps[_selectedCubemap]);
        Shader.SetGlobalTexture(GlobalCubemapId, CubeMaps[_selectedCubemap]);
        ReflectionProbe.RenderProbe();
    }
    public void AdjustAlignment()
    {
        CloseWindows();
        FixSize();
        AlignmentGuiScript.Initialize();
    }
    public void ClearAllTexturesGUI()
    {
        _clearTextures = false;
        ClearAllTextures();
        CloseWindows();
        SetMaterialValues();
        FixSizeSize(1024.0f, 1024.0f);
    }
    public void ShowFullMat()
    {
        CloseWindows();
        FixSize();
        MaterialGuiObject.SetActive(true);
        MaterialGuiScript.Initialize();
    }
    private void ShowGui()
    {
        foreach (var objToHide in _objectsToUnhide)
            objToHide.SetActive(true);
    }

    private void HideGui()
    {
        HideWindows();
    }

    public void SaveTextureFile(MapType mapType)
    {
        _textureToSave = HeightMap;
        var defaultName = "_" + mapType + ".png";
        var path = StandaloneFileBrowser.SaveFilePanel("Save Height Map", _lastDirectory, defaultName,
            _imageSaveFilter);
        if (path.IsNullOrEmpty()) return;

        _textureToSave = GetTextureToSave(mapType);

        var lastBar = path.LastIndexOf(_pathChar);
        _lastDirectory = path.Substring(0, lastBar + 1);
        SaveFile(path);
    }

    public void SaveTextureFile(MapType mapType, string path, string name)
    {
        _textureToSave = HeightMap;
        var defaultName = name + "_" + mapType + ".png";
        // var path = StandaloneFileBrowser.SaveFilePanel("Save Height Map", _lastDirectory, defaultName,
        //   _imageSaveFilter);
        List<string> PathSplit = path.Split(new string[] { "/", "\\" }, StringSplitOptions.None).ToList<string>();
        //PathSplit[PathSplit.Length - 1]
        PathSplit.RemoveAt(PathSplit.Count - 1);
        //Debug.Log(PathSplit);
        path = string.Join("/", PathSplit.ToArray());
        path = path + "/" + defaultName;
        if (path.IsNullOrEmpty()) return;

        _textureToSave = GetTextureToSave(mapType);
        var lastBar = path.LastIndexOf(_pathChar);
        _lastDirectory = path.Substring(0, lastBar + 1);
        Debug.Log(path);
        SaveFile(path);
    }
    private Texture2D GetTextureToSave(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.Height:
                return HeightMap;
            case MapType.Diffuse:
                return DiffuseMap != null ? DiffuseMap : DiffuseMapOriginal;
            case MapType.DiffuseOriginal:
                return DiffuseMapOriginal;
            case MapType.Metallic:
                return MetallicMap;
            case MapType.Smoothness:
                return SmoothnessMap;
            case MapType.Normal:
                return NormalMap;
            case MapType.Edge:
                return EdgeMap;
            case MapType.Ao:
                return AoMap;
            case MapType.Property:
                return PropertyMap;
            default:
                throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
        }
    }

    private void OpenTextureFile(MapType mapType)
    {
        _activeMapType = mapType;
        var title = "Open " + mapType + " Map";
        var path = StandaloneFileBrowser.OpenFilePanel(title, _lastDirectory, _imageLoadFilter, false);
        if (path[0].IsNullOrEmpty()) return;
        var lastBar = path[0].LastIndexOf(_pathChar);
        _lastDirectory = path[0].Substring(0, lastBar + 1);
        OpenFile(path[0]);
    }

    // ReSharper disable once InconsistentNaming
    private static string PCM2String(PropChannelMap pcm, string defaultName)
    {
        var returnString = defaultName;

        switch (pcm)
        {
            case PropChannelMap.Height:
                returnString = "Height";
                break;
            case PropChannelMap.Metallic:
                returnString = "Metallic";
                break;
            case PropChannelMap.Smoothness:
                returnString = "Smoothness";
                break;
            case PropChannelMap.Edge:
                returnString = "Edge";
                break;
            case PropChannelMap.Ao:
                returnString = "Ambient Occ";
                break;
            case PropChannelMap.AoEdge:
                returnString = "AO + Edge";
                break;
            case PropChannelMap.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pcm), pcm, null);
        }

        return returnString;
    }

    public void FlipNormalMapY()
    {
        if (NormalMap == null) return;
        for (var i = 0; i < NormalMap.width; i++)
            for (var j = 0; j < NormalMap.height; j++)
            {
                var pixelColor = NormalMap.GetPixel(i, j);
                pixelColor.g = 1.0f - pixelColor.g;
                NormalMap.SetPixel(i, j, pixelColor);
            }

        NormalMap.Apply();
    }

    private void ClearTexture(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.Height:
                if (HeightMap)
                {
                    Destroy(HeightMap);
                    HeightMap = null;
                }

                if (HdHeightMap)
                {
                    Destroy(HdHeightMap);
                    HdHeightMap = null;
                }

                break;
            case MapType.Diffuse:
                if (DiffuseMap)
                {
                    Destroy(DiffuseMap);
                    DiffuseMap = null;
                }

                if (DiffuseMapOriginal)
                {
                    Destroy(DiffuseMapOriginal);
                    DiffuseMapOriginal = null;
                }

                break;
            case MapType.Normal:
                if (NormalMap)
                {
                    Destroy(NormalMap);
                    NormalMap = null;
                }

                break;
            case MapType.Metallic:
                if (MetallicMap)
                {
                    Destroy(MetallicMap);
                    MetallicMap = null;
                }

                break;
            case MapType.Smoothness:
                if (SmoothnessMap)
                {
                    Destroy(SmoothnessMap);
                    SmoothnessMap = null;
                }

                break;
            case MapType.Edge:
                if (EdgeMap)
                {
                    Destroy(EdgeMap);
                    EdgeMap = null;
                }

                break;
            case MapType.Ao:
                if (AoMap)
                {
                    Destroy(AoMap);
                    AoMap = null;
                }

                break;
            case MapType.DiffuseOriginal:
                if (DiffuseMapOriginal)
                {
                    Destroy(DiffuseMapOriginal);
                    DiffuseMapOriginal = null;
                }

                break;
            case MapType.Property:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
        }

        Resources.UnloadUnusedAssets();
    }

    public void ClearAllTextures()
    {
        ClearTexture(MapType.Height);
        ClearTexture(MapType.Diffuse);
        ClearTexture(MapType.Normal);
        ClearTexture(MapType.Metallic);
        ClearTexture(MapType.Smoothness);
        ClearTexture(MapType.Edge);
        ClearTexture(MapType.Ao);
    }

    public void SaveProject()
    {
        const string defaultName = "baseName.mtz";
        var path = StandaloneFileBrowser.SaveFilePanel("Save Project", _lastDirectory, defaultName, "mtz");
        if (path.IsNullOrEmpty()) return;

        var lastBar = path.LastIndexOf(_pathChar);
        _lastDirectory = path.Substring(0, lastBar + 1);

        _saveLoadProjectScript.SaveProject(path);
    }

    public void LoadProject()
    {
        var path = StandaloneFileBrowser.OpenFilePanel("Load Project", _lastDirectory, "mtz", false);
        if (path[0].IsNullOrEmpty()) return;

        var lastBar = path[0].LastIndexOf(_pathChar);
        _lastDirectory = path[0].Substring(0, lastBar + 1);

        _saveLoadProjectScript.LoadProject(path[0]);
    }

    public void SetFormat(FileFormat newFormat)
    {
        _jpgSelected = false;
        _pngSelected = false;
        _tgaSelected = false;
        _exrSelected = false;

        switch (newFormat)
        {
            case FileFormat.Jpg:
                _jpgSelected = true;
                break;
            case FileFormat.Png:
                _pngSelected = true;
                break;
            case FileFormat.Tga:
                _tgaSelected = true;
                break;
            case FileFormat.Exr:
                _exrSelected = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newFormat), newFormat, null);
        }

        SelectedFormat = newFormat;
    }

    public void SetFormat(string newFormat)
    {
        _jpgSelected = false;
        _pngSelected = false;
        _tgaSelected = false;
        _exrSelected = false;

        switch (newFormat)
        {
            case "jpg":
                _jpgSelected = true;
                SelectedFormat = FileFormat.Jpg;
                break;
            case "png":
                _pngSelected = true;
                SelectedFormat = FileFormat.Png;
                break;
            case "tga":
                _tgaSelected = true;
                SelectedFormat = FileFormat.Tga;
                break;
            case "exr":
                _exrSelected = true;
                SelectedFormat = FileFormat.Exr;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newFormat), newFormat, null);
        }
    }

    public void SetLoadedTexture(MapType loadedTexture)
    {
        switch (loadedTexture)
        {
            case MapType.Height:
                SetPreviewMaterial(HeightMap);
                break;
            case MapType.Diffuse:
                SetPreviewMaterial(DiffuseMap);
                break;
            case MapType.DiffuseOriginal:
                SetPreviewMaterial(DiffuseMapOriginal);
                break;
            case MapType.Normal:
                SetPreviewMaterial(NormalMap);
                break;
            case MapType.Metallic:
                SetPreviewMaterial(MetallicMap);
                break;
            case MapType.Smoothness:
                SetPreviewMaterial(SmoothnessMap);
                break;
            case MapType.Edge:
                SetPreviewMaterial(EdgeMap);
                break;
            case MapType.Ao:
                SetPreviewMaterial(AoMap);
                break;
            case MapType.Property:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(loadedTexture), loadedTexture, null);
        }

        FixSize();
    }

    //==================================================//
    //					Property Map					//
    //==================================================//

    private void SetPropertyTexture(string texPrefix, Texture2D texture, Texture overlayTexture)
    {
        _propertyCompMaterial.SetTexture(texPrefix + "Tex", texture != null ? texture : TextureBlack);

        _propertyCompMaterial.SetTexture(texPrefix + "OverlayTex", overlayTexture);
    }

    private void SetPropertyMapChannel(string texPrefix, PropChannelMap pcm)
    {
        switch (pcm)
        {
            case PropChannelMap.Height:
                SetPropertyTexture(texPrefix, HeightMap, TextureGrey);
                break;
            case PropChannelMap.Metallic:
                SetPropertyTexture(texPrefix, MetallicMap, TextureGrey);
                break;
            case PropChannelMap.Smoothness:
                SetPropertyTexture(texPrefix, SmoothnessMap, TextureGrey);
                break;
            case PropChannelMap.Edge:
                SetPropertyTexture(texPrefix, EdgeMap, TextureGrey);
                break;
            case PropChannelMap.Ao:
                SetPropertyTexture(texPrefix, AoMap, TextureGrey);
                break;
            case PropChannelMap.AoEdge:
                SetPropertyTexture(texPrefix, AoMap, EdgeMap);
                break;
            case PropChannelMap.None:
                SetPropertyTexture(texPrefix, TextureBlack, TextureGrey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pcm), pcm, null);
        }
    }
    public void ProcessPropertyMapRevised()
    {
        var Size = GetSize();
        Texture2D TempMap = null;
        if (PropAlpha == PropChannelMap.None)
        {
            TempMap = new Texture2D(HeightMap.width, HeightMap.height, TextureFormat.RGB24, false);
        }
        else
        {
            TempMap = new Texture2D(HeightMap.width, HeightMap.height, TextureFormat.RGBA32, false);
        }

        Color theColour = new Color();
        for (int x = 0; x < TempMap.width; x++)
        {
            for (int y = 0; y < TempMap.height; y++)
            {
                theColour.r = TextureRed.GetPixel(x, y).grayscale;
                theColour.g = TextureGreen.GetPixel(x, y).grayscale;
                theColour.b = TextureBlue.GetPixel(x, y).grayscale;
                if (PropAlpha == PropChannelMap.None)
                {
                    theColour.a = 255;
                }
                else
                {
                    theColour.a = TextureAlpha.GetPixel(x, y).grayscale;

                }

                TempMap.SetPixel(x, y, theColour);
            }
        }
        TempMap.Apply();
        PropertyMap = TempMap;
        SaveTextureFile(MapType.Property);

    }
    public void ProcessPropertyMap()
    {
        SetPropertyMapChannel("_Red", PropRed);
        SetPropertyMapChannel("_Green", PropGreen);
        SetPropertyMapChannel("_Blue", PropBlue);

        var size = GetSize();
        var tempMap = RenderTexture.GetTemporary((int)size.x, (int)size.y, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default);
        Graphics.Blit(MetallicMap, tempMap, _propertyCompMaterial, 0);
        RenderTexture.active = tempMap;

        if (PropertyMap != null)
        {
            Destroy(PropertyMap);
            PropertyMap = null;
        }

        PropertyMap = new Texture2D(tempMap.width, tempMap.height, TextureFormat.RGB24, false);
        PropertyMap.ReadPixels(new Rect(0, 0, tempMap.width, tempMap.height), 0, 0);
        PropertyMap.Apply();

        RenderTexture.ReleaseTemporary(tempMap);
        // ReSharper disable once RedundantAssignment
        tempMap = null;
    }

    //==================================================//
    //					Project Saving					//
    //==================================================//

    private void SaveFile(string pathToFile)
    {
        _saveLoadProjectScript.SaveFile(pathToFile, _textureToSave);
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void CopyFile()
    {
        _saveLoadProjectScript.CopyFile(_textureToSave);
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void PasteFile()
    {
        ClearTexture(_activeMapType);
        _saveLoadProjectScript.PasteFile(_activeMapType);
    }

    private void OpenFile(string pathToFile)
    {
        if (pathToFile == null) return;

        // clear the existing texture we are loading
        ClearTexture(_activeMapType);

        StartCoroutine(_saveLoadProjectScript.LoadTexture(_activeMapType, pathToFile));
    }

    //==================================================//
    //			Fix the size of the test model			//
    //==================================================//


    private Vector2 GetSize()
    {
        Texture2D mapToUse = null;

        var size = new Vector2(1024, 1024);

        if (HeightMap != null)
            mapToUse = HeightMap;
        else if (DiffuseMap != null)
            mapToUse = DiffuseMap;
        else if (DiffuseMapOriginal != null)
            mapToUse = DiffuseMapOriginal;
        else if (NormalMap != null)
            mapToUse = NormalMap;
        else if (MetallicMap != null)
            mapToUse = MetallicMap;
        else if (SmoothnessMap != null)
            mapToUse = SmoothnessMap;
        else if (EdgeMap != null)
            mapToUse = EdgeMap;
        else if (AoMap != null) mapToUse = AoMap;

        if (mapToUse == null) return size;
        size.x = mapToUse.width;
        size.y = mapToUse.height;

        return size;
    }

    public void FixSize()
    {
        var size = GetSize();
        FixSizeSize(size.x, size.y);
    }

    private void FixSizeMap(Texture mapToUse)
    {
        FixSizeSize(mapToUse.width, mapToUse.height);
    }

    private void FixSizeMap(RenderTexture mapToUse)
    {
        FixSizeSize(mapToUse.width, mapToUse.height);
    }

    private void FixSizeSize(float width, float height)
    {
        var testObjectScale = new Vector3(1, 1, 1);
        const float area = 1.0f;

        testObjectScale.x = width / height;

        var newArea = testObjectScale.x * testObjectScale.y;
        var areaScale = Mathf.Sqrt(area / newArea);

        testObjectScale.x *= areaScale;
        testObjectScale.y *= areaScale;

        TestObject.transform.localScale = testObjectScale;
    }
    private float TextureAspectRatio(float Height, float Width)
    {
        return Width / Height;
    }
    private float LockTextureValue(float Input, float Height, float Width)
    {
        return Input * (1 + TextureAspectRatio(Height, Width));
    }
    public void MapSelection(int dropDownValue, string PropertyType)
    {
        PropChannelMap _channels;
        switch (dropDownValue)
        {

            case (0):
                _channels = PropChannelMap.None;
                break;
            case (1):
                _channels = PropChannelMap.Metallic;
                break;
            case (2):
                _channels = PropChannelMap.Smoothness;
                break;
            case (3):
                _channels = PropChannelMap.Height;
                break;
            case (4):
                _channels = PropChannelMap.Ao;
                break;
            case (5):
                _channels = PropChannelMap.Edge;
                break;
            case (6):
                _channels = PropChannelMap.AoEdge;
                break;
            default:
                _channels = PropChannelMap.None;
                break;
        }
        switch (PropertyType)
        {
            case ("Red"):
                PropRed = _channels;
                break;
            case ("Blue"):
                PropBlue = _channels;
                break;
            case ("Green"):
                PropGreen = _channels;
                break;
            case ("Alpha"):
                PropAlpha = _channels;
                break;
            default:
                break;
        }
    }
    public void SavePropertyMap()
    {
        if (PropAlpha == PropChannelMap.None)
        {
            ProcessPropertyMap();
        }
        else
        {
            ProcessPropertyMapRevised();
        }
        SaveTextureFile(MapType.Property);
    }
    public void QuickSavePropertyMap()
    {
        ProcessPropertyMap();
        _textureToSave = PropertyMap;
        SaveFile(QuicksavePathProperty);
    }

    #region Gui Hide Variables

    [HideInInspector] public CountLocker HideGuiLocker = new CountLocker();
    private bool _lastGuiIsHiddenState;
    private bool _isGuiHidden;

    public bool IsGuiHidden
    {
        get => _isGuiHidden;
        set
        {
            if (HideGuiLocker.IsLocked)
            {
                Debug.Log("Tentando modificar IsGuiHidden quando travado");
                return;
            }

            if (value && !_isGuiHidden)
            {
                HideGui();
                _isGuiHidden = true;
            }
            else if (!value && _isGuiHidden)
            {
                ShowGui();
                _isGuiHidden = false;
            }
        }
    }


    //public void SetPropertyMap()

    #endregion
    #endregion
}