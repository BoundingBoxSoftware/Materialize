
using System;
using System.Net;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

public enum Textures {
	height,
	diffuse,
	diffuseOriginal,
	specular,
	roughness,
	normal,
	msao
}

public enum PropChannelMap {
	None,
	Height,
	Metallic,
	Smoothness,
	Edge,
	Ao,
	AoEdge
}


public class MainGui : MonoBehaviour {

	public static MainGui instance;

	string message = "";
	float alpha = 1.0f;
	char pathChar = '/';

	//Rect toolsWindowRect = new Rect( 0, 0, 500, 500 );
	//int toolsWindowID = 50;
	string toolsWindowTitle = "Texture Tools";

	MapType mapTypeToLoad;
	Texture2D textureToLoad;
	Texture2D textureToSave;
	string mapType = "";

	public GameObject HeightFromDiffuseGuiObject;
	HeightFromDiffuseGui HeightFromDiffuseGuiScript;

	public GameObject NormalFromHeightGuiObject;
	NormalFromHeightGui NormalFromHeightGuiScript;

	public GameObject EdgeFromNormalGuiObject;
	EdgeFromNormalGui EdgeFromNormalGuiScript;

	public GameObject AOFromNormalGuiObject;
	AOFromNormalGui AOFromNormalGuiScript;

	public GameObject EditDiffuseGuiObject;
	EditDiffuseGui EditDiffuseGuiScript;

	public GameObject MetallicGuiObject;
	MetallicGui MetallicGuiScript;

	public GameObject SmoothnessGuiObject;
	SmoothnessGui SmoothnessGuiScript;

	public GameObject MaterialGuiObject;
	MaterialGui MaterialGuiScript;

	public GameObject PostProcessGuiObject;
	PostProcessGui PostProcessGuiScript;

	public GameObject TilingTextureMakerGuiObject;
	TilingTextureMakerGui TilingTextureMakerGuiScript;

	public AlignmentGui AlignmentGuiScript;

	public GameObject SuggestionGuiObject;
	SuggestionGui SuggestionGuiScript;

	public GameObject SaveLoadProjectObject;
	SaveLoadProject SaveLoadProjectScript;

	public GameObject CommandListExecutorObject;
	CommandListExecutor CommandListExecutorScript;

	public GameObject SettingsGuiObject;
	SettingsGui SettingsGuiScript;

	public Texture2D _TextureBlack;
	public Texture2D _TextureWhite;
	public Texture2D _TextureGrey;
	public Texture2D _TextureNormal;
	
	public RenderTexture _HDHeightMap;
	public Texture2D _HeightMap;
	public Texture2D _DiffuseMap;
	public Texture2D _DiffuseMapOriginal;
	public Texture2D _NormalMap;
	public Texture2D _MetallicMap;
	public Texture2D _SmoothnessMap;
	public Texture2D _EdgeMap;
	public Texture2D _AOMap;

	public Texture2D _PropertyMap;

	public Material FullMaterialRef;
	public Material FullMaterial;
	public Material SampleMaterialRef;
	public Material SampleMaterial;

	public GameObject testObject;
	public GameObject testObjectCube;
	public GameObject testObjectCylinder;
	public GameObject testObjectSphere;

    //public Material skyboxMaterial;
	public ReflectionProbe reflectionProbe;
	public Cubemap[] CubeMaps;
	int selectedCubemap = 0;

	Material thisMaterial;

	float _Falloff = 0.1f;
	float _OverlapX = 0.2f;
	float _OverlapY = 0.2f;
	bool _SmoothBlend = false;
	float _GamaCorrection = 2.2f;

	bool busySaving = false;

	public FileFormat selectedFormat = FileFormat.tga;
	bool bmpSelected = true;
	bool jpgSelected = false;
	bool pngSelected = true;
	bool tgaSelected = false;
	bool tiffSelected = false;

	public bool hideGui = false;
	Camera thisCamera;
	Vector3 CameraTargetPos = Vector3.zero;
	Vector3 CameraOffsetPos = Vector3.zero;

	bool clearTextures = false;

	List<GameObject> objectsToUnhide;

	public FileBrowser fileBrowser;

	Shader PropertyCompShader;
	Material PropertyCompMaterial;

	public string QuicksavePath = "";
	public string QuicksavePathHeight = "";
	public string QuicksavePathDiffuse = "";
	public string QuicksavePathNormal = "";
	public string QuicksavePathMetallic = "";
	public string QuicksavePathSmoothness = "";
	public string QuicksavePathEdge = "";
	public string QuicksavePathAO = "";
	public string QuicksavePathProperty = "";

	public PropChannelMap propRed = PropChannelMap.None;
	public PropChannelMap propGreen = PropChannelMap.None;
	public PropChannelMap propBlue = PropChannelMap.None;
	bool propRedChoose = false;
	bool propGreenChoose = false;
	bool propBlueChoose = false;

	private ClipboardImageHelper.ClipboardImage CIH;

	void Start () {

		MainGui.instance = this;

		_HeightMap = null;
		_HDHeightMap = null;
		_DiffuseMap = null;
		_DiffuseMapOriginal = null;
		_NormalMap = null;
		_MetallicMap = null;
		_SmoothnessMap = null;
		_EdgeMap = null;
		_AOMap = null;

		//fileBrowser = this.GetComponent<FileBrowser> ();

        PropertyCompShader = Shader.Find ("Hidden/Blit_Property_Comp");
		PropertyCompMaterial = new Material (PropertyCompShader);

		thisCamera = Camera.main;
		CameraTargetPos = thisCamera.transform.position;
		CameraOffsetPos = CameraTargetPos;

		Shader.SetGlobalFloat ("_GamaCorrection", _GamaCorrection);

		FullMaterial = new Material ( FullMaterialRef.shader );
		FullMaterial.CopyPropertiesFromMaterial (FullMaterialRef);
		//FullMaterial = null;
		//FullMaterial = tempFullMaterial;

		SampleMaterial = new Material ( SampleMaterialRef.shader );
		SampleMaterial.CopyPropertiesFromMaterial (SampleMaterialRef);
		//SampleMaterial = null;
		//SampleMaterial = tempSampleMaterial;

		HeightFromDiffuseGuiScript = HeightFromDiffuseGuiObject.GetComponent<HeightFromDiffuseGui>();
		NormalFromHeightGuiScript = NormalFromHeightGuiObject.GetComponent<NormalFromHeightGui>();
		EdgeFromNormalGuiScript = EdgeFromNormalGuiObject.GetComponent<EdgeFromNormalGui>();
		AOFromNormalGuiScript = AOFromNormalGuiObject.GetComponent<AOFromNormalGui>();
		EditDiffuseGuiScript = EditDiffuseGuiObject.GetComponent<EditDiffuseGui>();
		MetallicGuiScript = MetallicGuiObject.GetComponent<MetallicGui> ();
		SmoothnessGuiScript = SmoothnessGuiObject.GetComponent<SmoothnessGui>();
		MaterialGuiScript = MaterialGuiObject.GetComponent<MaterialGui>();
		PostProcessGuiScript = PostProcessGuiObject.GetComponent<PostProcessGui> ();
		TilingTextureMakerGuiScript = TilingTextureMakerGuiObject.GetComponent<TilingTextureMakerGui>();
		SuggestionGuiScript = SuggestionGuiObject.GetComponent<SuggestionGui>();
		SaveLoadProjectScript = SaveLoadProjectObject.GetComponent<SaveLoadProject>();
		CommandListExecutorScript = CommandListExecutorObject.GetComponent<CommandListExecutor> ();
		SettingsGuiScript = SettingsGuiObject.GetComponent<SettingsGui> ();

		SettingsGuiScript.LoadSettings();

		//HeightFromNormalGuiScript = HeightFromNormalGuiObject.GetComponent<HeightFromNormalGui>();

		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
			pathChar = '\\';
		}

		CIH = new ClipboardImageHelper.ClipboardImage ();

		testObject.GetComponent<Renderer>().material = FullMaterial;
		SetMaterialValues();

		reflectionProbe.RenderProbe();

	}

	public void SetPreviewMaterial( Texture2D textureToPreview ) {
		CloseWindows();
		if (textureToPreview != null) {
			FixSizeMap (textureToPreview);
			SampleMaterial.SetTexture("_MainTex", textureToPreview );
			testObject.GetComponent<Renderer>().material = SampleMaterial;
		}
	}

	public void SetPreviewMaterial( RenderTexture textureToPreview ) {
		CloseWindows();
		if (textureToPreview != null) {
			FixSizeMap (textureToPreview);
			SampleMaterial.SetTexture("_MainTex", textureToPreview );
			testObject.GetComponent<Renderer>().material = SampleMaterial;
		}
	}

	public void SetMaterialValues() {
		
		Shader.SetGlobalTexture ("_GlobalCubemap", CubeMaps[selectedCubemap] );

		if (_HeightMap != null) { 
			FullMaterial.SetTexture ("_DisplacementMap", _HeightMap); 
		} else {
			FullMaterial.SetTexture ("_DisplacementMap", _TextureGrey); 
		}

		if (_DiffuseMap != null) { 
			FullMaterial.SetTexture ("_DiffuseMap", _DiffuseMap); 
		} else if (_DiffuseMapOriginal != null) { 
			FullMaterial.SetTexture ("_DiffuseMap", _DiffuseMapOriginal); 
		} else {
			FullMaterial.SetTexture ("_DiffuseMap", _TextureGrey); 
		}

		if (_NormalMap != null) {
			FullMaterial.SetTexture ("_NormalMap", _NormalMap);
		} else {
			FullMaterial.SetTexture ("_NormalMap", _TextureNormal);
		}

		if (_MetallicMap != null) { 
			FullMaterial.SetTexture ("_MetallicMap", _MetallicMap); 
		} else {
			FullMaterial.SetTexture ("_MetallicMap", _TextureBlack); 
		}

		if (_SmoothnessMap != null) { 
			FullMaterial.SetTexture ("_SmoothnessMap", _SmoothnessMap); 
		} else {
			FullMaterial.SetTexture ("_SmoothnessMap", _TextureBlack); 
		}

		if (_AOMap != null) { 
			FullMaterial.SetTexture ("_AOMap", _AOMap); 
		} else {
			FullMaterial.SetTexture ("_AOMap", _TextureWhite); 
		}

		if (_EdgeMap != null) { 
			FullMaterial.SetTexture ("_EdgeMap", _EdgeMap); 
		} else {
			FullMaterial.SetTexture ("_EdgeMap", _TextureGrey);
		}

		testObject.GetComponent<Renderer>().material = FullMaterial;

		FullMaterial.SetVector ("_Tiling", new Vector4 (1, 1, 0, 0));

	}

	public void CloseWindows() {
		HeightFromDiffuseGuiScript.Close ();
		NormalFromHeightGuiScript.Close ();
		EdgeFromNormalGuiScript.Close ();
		AOFromNormalGuiScript.Close ();
		EditDiffuseGuiScript.Close ();
		MetallicGuiScript.Close ();
		SmoothnessGuiScript.Close ();
		TilingTextureMakerGuiScript.Close ();
		AlignmentGuiScript.Close ();
		MaterialGuiObject.SetActive (false);
		PostProcessGuiObject.SetActive (false);
		//SettingsGuiObject.SetActive (false);
		//SuggestionGuiObject.SetActive (false);
	}

	void HideWindows() {

		objectsToUnhide = new List<GameObject> ();

		if (HeightFromDiffuseGuiObject.activeSelf) {
			objectsToUnhide.Add( HeightFromDiffuseGuiObject );
		}

		if (NormalFromHeightGuiObject.activeSelf) {
			objectsToUnhide.Add( NormalFromHeightGuiObject );
		}

		if (EdgeFromNormalGuiObject.activeSelf) {
			objectsToUnhide.Add( EdgeFromNormalGuiObject );
		}

		if (AOFromNormalGuiObject.activeSelf) {
			objectsToUnhide.Add( AOFromNormalGuiObject );
		}

		if (EditDiffuseGuiObject.activeSelf) {
			objectsToUnhide.Add( EditDiffuseGuiObject );
		}

		if (MetallicGuiObject.activeSelf) {
			objectsToUnhide.Add( MetallicGuiObject );
		}

		if (SmoothnessGuiObject.activeSelf) {
			objectsToUnhide.Add( SmoothnessGuiObject );
		}

		if (MaterialGuiObject.activeSelf) {
			objectsToUnhide.Add( MaterialGuiObject );
		}

		if( PostProcessGuiObject.activeSelf){
			objectsToUnhide.Add( PostProcessGuiObject );
		}

		if (TilingTextureMakerGuiObject.activeSelf) {
			objectsToUnhide.Add( TilingTextureMakerGuiObject );
		}

		//if (SettingsGuiObject.activeSelf) {
		//	objectsToUnhide.Add ( SettingsGuiObject );
		//}

		HeightFromDiffuseGuiObject.SetActive (false);
		NormalFromHeightGuiObject.SetActive (false);
		EdgeFromNormalGuiObject.SetActive (false);
		AOFromNormalGuiObject.SetActive (false);
		EditDiffuseGuiObject.SetActive (false);
		MetallicGuiObject.SetActive (false);
		SmoothnessGuiObject.SetActive (false);
		MaterialGuiObject.SetActive (false);
		PostProcessGuiObject.SetActive (false);
		TilingTextureMakerGuiObject.SetActive (false);
		//SettingsGuiObject.SetActive (false);

	}

	void Update() {

	}

	void ShowFullMaterial() {
		CloseWindows();
		FixSize();
		MaterialGuiObject.SetActive(true);
		MaterialGuiScript.Initialize();
	}

	void Fullscreen() {
		if (Screen.fullScreen) {
			Screen.fullScreen = false;
		} else {
			Screen.fullScreen = true;
		}
	}

	void SetFileMaskImage() {
		fileBrowser.fileMasks = "*.png;*.jpg;*.jpeg;*.tga;*.bmp;*.tif";
	}
	void SetFileMaskProject() {
		fileBrowser.fileMasks = "*.mtz";
	}
	
	void OnGUI () {

		//==================================================//
		// 					Unhidable Buttons				//
		//==================================================//

		if (GUI.Button (new Rect(Screen.width - 80, Screen.height - 40, 70, 30), "Quit")) {
			Application.Quit();
		}

		GUI.enabled = false;
		if (Screen.fullScreen) {
			if (GUI.Button (new Rect (Screen.width - 190, Screen.height - 40, 100, 30), "Windowed")) {
				Fullscreen();
			}
		} else {
			if (GUI.Button (new Rect (Screen.width - 190, Screen.height - 40, 100, 30), "Full Screen")) {
				Fullscreen();
			}
		}
		GUI.enabled = true;

		if (GUI.Button (new Rect(Screen.width - 260, 10, 140, 30), "Make Suggestion")) {
			SuggestionGuiObject.SetActive(true);
		}

		if( hideGui == false ){
			if (GUI.Button (new Rect(Screen.width - 110, 10, 100, 30), "Hide Gui")) {
				hideGui = true;
				HideWindows();
				//CameraTargetPos = new Vector3(0,0,-10);
			}
		} else {
			if (GUI.Button (new Rect(Screen.width - 110, 10, 100, 30), "Show Gui")) {
				hideGui = false;
				for( int i = 0; i< objectsToUnhide.Count; i++ ){
					objectsToUnhide[i].SetActive(true);
				}
				//CameraTargetPos = CameraOffsetPos;
			}
			return;
		}

		//==================================================//
		// 						Main Gui					//
		//==================================================//


		int spacingX = 130;
		int spacingY = 150;

		int offsetX = 20;
		int offsetY = 20;


		//==============================//
		// 			Height Map			//
		//==============================//

		GUI.Box( new Rect (offsetX, offsetY, 110, 250), "Height Map" );
		
		if ( _HeightMap != null ) {
			GUI.DrawTexture (new Rect(offsetX + 5, offsetY + 25, 100, 100), _HeightMap );
		}

		// Paste 
		if (GUI.Button (new Rect(offsetX + 5, offsetY + 130, 20, 20), "P")) {
			mapTypeToLoad = MapType.height;
			PasteFile ();
		}

		if (_HeightMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Copy
		if (GUI.Button (new Rect(offsetX + 30, offsetY + 130, 20, 20), "C")) {
			textureToSave = _HeightMap;
			CopyFile ();
		}

		GUI.enabled = true;

		// Open
		if (GUI.Button (new Rect(offsetX + 60, offsetY + 130, 20, 20), "O")) {
			mapTypeToLoad = MapType.height;
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Open Height Map", this.OpenFile);
		}

		if (_HeightMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Save
		if (GUI.Button (new Rect(offsetX + 85, offsetY + 130, 20, 20), "S")) {
			textureToSave = _HeightMap;
			mapType = "_height";
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Save Height Map", this.SaveFile);
		}


		if (_HeightMap == null || QuicksavePathHeight == "") { GUI.enabled = false; } else { GUI.enabled = true; }

		// Quick Save
		if (GUI.Button (new Rect(offsetX + 15, offsetY + 160, 80, 20), "Quick Save")) {
			textureToSave = _HeightMap;
			mapType = "_height";
			SaveFile(QuicksavePathProperty);
		}

		if (_HeightMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + 15, offsetY + 190, 80, 20), "Preview")) {
			SetPreviewMaterial( _HeightMap );
			//SetPreviewMaterial( _HDHeightMap );
		}
		GUI.enabled = true;

		if (_DiffuseMapOriginal == null && _DiffuseMap == null && _NormalMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + 5, offsetY + 220, 50, 20), "Create")) {
			CloseWindows();
			FixSize();
			HeightFromDiffuseGuiObject.SetActive(true);
			HeightFromDiffuseGuiScript.NewTexture();
			HeightFromDiffuseGuiScript.DoStuff();
		}
		GUI.enabled = true;

		if (_HeightMap == null ){ GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + 60, offsetY + 220, 45, 20), "Clear")) {
			ClearTexture(MapType.height);
			CloseWindows ();
			SetMaterialValues ();
			FixSize ();
		}
		GUI.enabled = true;


		//==============================//
		// 			Diffuse Map			//
		//==============================//

		GUI.Box( new Rect (offsetX + spacingX, offsetY, 110, 250), "Diffuse Map" );

		if (_DiffuseMap != null) {
			GUI.DrawTexture (new Rect (offsetX + spacingX + 5, offsetY + 25, 100, 100), _DiffuseMap);
		} else if (_DiffuseMapOriginal != null) {
			GUI.DrawTexture (new Rect (offsetX + spacingX + 5, offsetY + 25, 100, 100), _DiffuseMapOriginal);
		}

		// Paste 
		if (GUI.Button (new Rect(offsetX + spacingX  + 5, offsetY + 130, 20, 20), "P")) {
			mapTypeToLoad = MapType.diffuseOriginal;
			PasteFile ();
		}

		if ( _DiffuseMapOriginal == null && _DiffuseMap == null ) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Copy
		if (GUI.Button (new Rect(offsetX + spacingX  + 30, offsetY + 130, 20, 20), "C")) {
			if( _DiffuseMap != null ){
				textureToSave = _DiffuseMap;
			}else{
				textureToSave = _DiffuseMapOriginal;
			}
			CopyFile ();
		}

		GUI.enabled = true;

		// Open
		if (GUI.Button (new Rect(offsetX + spacingX + 60, offsetY + 130, 20, 20), "O")) {
			mapTypeToLoad = MapType.diffuseOriginal;
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Open Diffuse Map", this.OpenFile );
		}
		
		if ( _DiffuseMapOriginal == null && _DiffuseMap == null ) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Save
		if (GUI.Button (new Rect(offsetX + spacingX + 85, offsetY + 130, 20, 20), "S")) {
			if( _DiffuseMap != null ){
				textureToSave = _DiffuseMap;
			}else{
				textureToSave = _DiffuseMapOriginal;
			}
			mapType = "_diffuse";
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Save Diffuse Map", this.SaveFile );
		}

		if ( ( _DiffuseMapOriginal == null && _DiffuseMap == null ) || QuicksavePathDiffuse == "") { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Quick Save
		if (GUI.Button (new Rect(offsetX + spacingX + 15, offsetY + 160, 80, 20), "Quick Save")) {
			if( _DiffuseMap != null ){
				textureToSave = _DiffuseMap;
			}else{
				textureToSave = _DiffuseMapOriginal;
			}
			mapType = "_diffuse";
			SaveFile(QuicksavePathDiffuse);
		}

		if ( _DiffuseMapOriginal == null && _DiffuseMap == null ) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX + 15, offsetY + 190, 80, 20), "Preview")) {
			if( _DiffuseMap != null ){
				SetPreviewMaterial( _DiffuseMap );
			}else{
				SetPreviewMaterial( _DiffuseMapOriginal );
			}
		}

		if ( _DiffuseMapOriginal == null ) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX + 5, offsetY + 220, 50, 20), "Edit")) {
			CloseWindows();
			FixSize();
			EditDiffuseGuiObject.SetActive(true);
			EditDiffuseGuiScript.NewTexture();
			EditDiffuseGuiScript.DoStuff();
		}

		if ( _DiffuseMapOriginal == null && _DiffuseMap == null ) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX + 60, offsetY + 220, 45, 20), "Clear")) {
			ClearTexture(MapType.diffuse);
			CloseWindows ();
			SetMaterialValues ();
			FixSize ();
		}
		GUI.enabled = true;


		//==============================//
		// 			Normal Map			//
		//==============================//
		
		GUI.Box( new Rect (offsetX + spacingX * 2, offsetY, 110, 250), "Normal Map" );
		
		if ( _NormalMap != null ) {
			GUI.DrawTexture (new Rect(offsetX + spacingX * 2 + 5, offsetY + 25, 100, 100), _NormalMap );
		}

		// Paste 
		if (GUI.Button (new Rect(offsetX + spacingX * 2  + 5, offsetY + 130, 20, 20), "P")) {
			mapTypeToLoad = MapType.normal;
			PasteFile ();
		}

		if (_NormalMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Copy
		if (GUI.Button (new Rect(offsetX + spacingX * 2  + 30, offsetY + 130, 20, 20), "C")) {
			textureToSave = _NormalMap;
			CopyFile ();
		}

		GUI.enabled = true;

		//Open
		if (GUI.Button (new Rect(offsetX + spacingX * 2 + 60, offsetY + 130, 20, 20), "O")) {
			mapTypeToLoad = MapType.normal;
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Open Normal Map", this.OpenFile );
		}

		if (_NormalMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Save
		if (GUI.Button (new Rect(offsetX + spacingX * 2 + 85, offsetY + 130, 20, 20), "S")) {
			textureToSave = _NormalMap;
			mapType = "_normal";
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Save Normal Map", this.SaveFile );
		}

		if (_NormalMap == null || QuicksavePathNormal == "") { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Quick Save
		if (GUI.Button (new Rect(offsetX + spacingX * 2 + 15, offsetY + 160, 80, 20), "Quick Save")) {
			textureToSave = _NormalMap;
			mapType = "_normal";
			SaveFile(QuicksavePathNormal);
		}
		
		if (_NormalMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 2 + 15, offsetY + 190, 80, 20), "Preview")) {
			SetPreviewMaterial( _NormalMap );
		}

		if (_HeightMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX + spacingX * 2 + 5, offsetY + 220, 50, 20), "Create")) {
			CloseWindows ();
			FixSize ();
			NormalFromHeightGuiObject.SetActive (true);
			NormalFromHeightGuiScript.NewTexture ();
			NormalFromHeightGuiScript.DoStuff ();
		}

		if (_NormalMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 2 + 60, offsetY + 220, 45, 20), "Clear")) {
			ClearTexture(MapType.normal);
			CloseWindows ();
			SetMaterialValues ();
			FixSize ();
		}
		GUI.enabled = true;


		//==============================//
		// 			Metallic Map		//
		//==============================//
		
		GUI.Box( new Rect (offsetX + spacingX * 3, offsetY, 110, 250), "Metallic Map" );
		
		if ( _MetallicMap != null ) {
			GUI.DrawTexture (new Rect(offsetX + spacingX * 3 + 5, offsetY + 25, 100, 100), _MetallicMap );
		}

		// Paste 
		if (GUI.Button (new Rect(offsetX + spacingX * 3  + 5, offsetY + 130, 20, 20), "P")) {
			mapTypeToLoad = MapType.metallic;
			PasteFile ();
		}

		if (_MetallicMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Copy
		if (GUI.Button (new Rect(offsetX + spacingX * 3  + 30, offsetY + 130, 20, 20), "C")) {
			textureToSave = _MetallicMap;
			CopyFile ();
		}

		GUI.enabled = true;

		//Open
		if (GUI.Button (new Rect(offsetX + spacingX * 3 + 60, offsetY + 130, 20, 20), "O")) {
			mapTypeToLoad = MapType.metallic;
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Open Metallic Map", this.OpenFile );
			//UniFileBrowser.use.OpenFileWindow (OpenFile);
		}

		if (_MetallicMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Save
		if (GUI.Button (new Rect(offsetX + spacingX * 3 + 85, offsetY + 130, 20, 20), "S")) {
			textureToSave = _MetallicMap;
			mapType = "_metallic";
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Save Metallic Map", this.SaveFile );
			//UniFileBrowser.use.SaveFileWindow (SaveFile);
		}

		if (_MetallicMap == null || QuicksavePathMetallic == "") { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Quick Save
		if (GUI.Button (new Rect(offsetX + spacingX * 3 + 15, offsetY + 160, 80, 20), "Quick Save")) {
			textureToSave = _MetallicMap;
			mapType = "_metallic";
			SaveFile(QuicksavePathMetallic);
		}
		
		if (_MetallicMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 3 + 15, offsetY + 190, 80, 20), "Preview")) {
			SetPreviewMaterial( _MetallicMap );
		}

		if ( _DiffuseMapOriginal == null && _DiffuseMap == null ){ GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX + spacingX * 3 + 5, offsetY + 220, 50, 20), "Create")) {
			CloseWindows();
			FixSize();

			MetallicGuiObject.SetActive(true);
			MetallicGuiScript.NewTexture();
			MetallicGuiScript.DoStuff();
		}

		if (_MetallicMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 3 + 60, offsetY + 220, 45, 20), "Clear")) {
			ClearTexture(MapType.metallic);
			CloseWindows ();
			SetMaterialValues ();
			FixSize ();
		}
		GUI.enabled = true;


		//==============================//
		// 		Smoothness Map			//
		//==============================//
		
		GUI.Box( new Rect (offsetX + spacingX * 4, offsetY, 110, 250), "Smoothness Map" );
		
		if ( _SmoothnessMap != null ) {
			GUI.DrawTexture (new Rect(offsetX + spacingX * 4 + 5, offsetY + 25, 100, 100), _SmoothnessMap );
		}

		// Paste 
		if (GUI.Button (new Rect(offsetX + spacingX * 4  + 5, offsetY + 130, 20, 20), "P")) {
			mapTypeToLoad = MapType.smoothness;
			PasteFile ();
		}

		if (_SmoothnessMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Copy
		if (GUI.Button (new Rect(offsetX + spacingX * 4  + 30, offsetY + 130, 20, 20), "C")) {
			textureToSave = _SmoothnessMap;
			CopyFile ();
		}

		GUI.enabled = true;

		//Open
		if (GUI.Button (new Rect(offsetX + spacingX * 4 + 60, offsetY + 130, 20, 20), "O")) {
			mapTypeToLoad = MapType.smoothness;
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Open Smoothness Map", this.OpenFile );
		}
		
		if (_SmoothnessMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Save
		if (GUI.Button (new Rect(offsetX + spacingX * 4 + 85, offsetY + 130, 20, 20), "S")) {
			textureToSave = _SmoothnessMap;
			mapType = "_smoothness";
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Save Smoothness Map", this.SaveFile );
		}

		if (_SmoothnessMap == null || QuicksavePathSmoothness == "") { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Quick Save
		if (GUI.Button (new Rect(offsetX + spacingX * 4 + 15, offsetY + 160, 80, 20), "Quick Save")) {
			textureToSave = _SmoothnessMap;
			mapType = "_smoothness";
			SaveFile(QuicksavePathSmoothness);
		}
		
		if (_SmoothnessMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 4 + 15, offsetY + 190, 80, 20), "Preview")) {
			SetPreviewMaterial( _SmoothnessMap );
		}

		if ( _DiffuseMapOriginal == null && _DiffuseMap == null ){ GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX + spacingX * 4 + 5, offsetY + 220, 50, 20), "Create")) {
			CloseWindows();
			FixSize();
			SmoothnessGuiObject.SetActive(true);
			SmoothnessGuiScript.NewTexture();
			SmoothnessGuiScript.DoStuff();
		}

		if (_SmoothnessMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 4 + 60, offsetY + 220, 45, 20), "Clear")) {
			ClearTexture(MapType.smoothness);
			CloseWindows ();
			SetMaterialValues ();
			FixSize ();
		}
		GUI.enabled = true;


		//==============================//
		// 			Edge Map			//
		//==============================//
		
		GUI.Box( new Rect (offsetX + spacingX * 5, offsetY, 110, 250), "Edge Map" );
		
		if ( _EdgeMap != null ) {
			GUI.DrawTexture (new Rect(offsetX + spacingX * 5 + 5, offsetY + 25, 100, 100), _EdgeMap );
		}

		// Paste 
		if (GUI.Button (new Rect(offsetX + spacingX * 5  + 5, offsetY + 130, 20, 20), "P")) {
			mapTypeToLoad = MapType.edge;
			PasteFile ();
		}

		if (_EdgeMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Copy
		if (GUI.Button (new Rect(offsetX + spacingX * 5  + 30, offsetY + 130, 20, 20), "C")) {
			textureToSave = _EdgeMap;
			CopyFile ();
		}

		GUI.enabled = true;
		
		//Open
		if (GUI.Button (new Rect(offsetX + spacingX * 5 +60, offsetY + 130, 20, 20), "O")) {
			mapTypeToLoad = MapType.edge;
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Open Edge Map", this.OpenFile );
		}

		if (_EdgeMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Save
		if (GUI.Button (new Rect(offsetX + spacingX * 5 + 85, offsetY + 130, 20, 20), "S")) {
			textureToSave = _EdgeMap;
			mapType = "_edge";
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Save Edge Map", this.SaveFile );
		}

		if (_EdgeMap == null || QuicksavePathEdge == "") { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Quick Save
		if (GUI.Button (new Rect(offsetX + spacingX * 5 + 15, offsetY + 160, 80, 20), "Quick Save")) {
			textureToSave = _EdgeMap;
			mapType = "_edge";
			SaveFile(QuicksavePathEdge);
		}
		
		if (_EdgeMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 5 + 15, offsetY + 190, 80, 20), "Preview")) {
			SetPreviewMaterial( _EdgeMap );
		}

		if ( _NormalMap == null ){ GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 5 + 5, offsetY + 220, 50, 20), "Create")) {
			CloseWindows();
			FixSize();
			EdgeFromNormalGuiObject.SetActive(true);
			EdgeFromNormalGuiScript.NewTexture();
			EdgeFromNormalGuiScript.DoStuff();
		}

		if (_EdgeMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 5 + 60, offsetY + 220, 45, 20), "Clear")) {
			ClearTexture(MapType.edge);
			CloseWindows ();
			SetMaterialValues ();
			FixSize ();
		}
		GUI.enabled = true;

		//==============================//
		// 			AO Map				//
		//==============================//
		
		GUI.Box( new Rect (offsetX + spacingX * 6, offsetY, 110, 250), "AO Map" );
		
		if ( _AOMap != null ) {
			GUI.DrawTexture (new Rect(offsetX + spacingX * 6 + 5, offsetY + 25, 100, 100), _AOMap );
		}


		// Paste 
		if (GUI.Button (new Rect(offsetX + spacingX * 6  + 5, offsetY + 130, 20, 20), "P")) {
			mapTypeToLoad = MapType.ao;
			PasteFile ();
		}

		if (_AOMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Copy
		if (GUI.Button (new Rect(offsetX + spacingX * 6  + 30, offsetY + 130, 20, 20), "C")) {
			textureToSave = _AOMap;
			CopyFile ();
		}

		GUI.enabled = true;
		
		//Open
		if (GUI.Button (new Rect(offsetX + spacingX * 6 + 60, offsetY + 130, 20, 20), "O")) {
			mapTypeToLoad = MapType.ao;
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Open AO Map", this.OpenFile );
		}
		
		if (_AOMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }

		// Save
		if (GUI.Button (new Rect(offsetX + spacingX * 6 + 85, offsetY + 130, 20, 20), "S")) {
			textureToSave = _AOMap;
			mapType = "_ao";
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Save AO Map", this.SaveFile );
		}

		if (_AOMap == null || QuicksavePathAO == "") { GUI.enabled = false; } else { GUI.enabled = true; }
		
		// Quick Save
		if (GUI.Button (new Rect(offsetX + spacingX * 6 + 15, offsetY + 160, 80, 20), "Quick Save")) {
			textureToSave = _AOMap;
			mapType = "_ao";
			SaveFile(QuicksavePathAO);
		}
		
		if (_AOMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 6 + 15, offsetY + 190, 80, 20), "Preview")) {
			SetPreviewMaterial( _AOMap );
		}

		if ( _NormalMap == null && _HeightMap == null ){ GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 6 + 5, offsetY + 220, 50, 20), "Create")) {
			CloseWindows();
			FixSize();
			AOFromNormalGuiObject.SetActive(true);
			AOFromNormalGuiScript.NewTexture();
			AOFromNormalGuiScript.DoStuff();
		}

		if (_AOMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + spacingX * 6 + 60, offsetY + 220, 45, 20), "Clear")) {
			ClearTexture(MapType.ao);
			CloseWindows ();
			SetMaterialValues ();
			FixSize ();
		}
		GUI.enabled = true;


		//==============================//
		// 		Map Saving Options		//
		//==============================//

		offsetX = offsetX + spacingX * 7;

		GUI.Box( new Rect (offsetX, offsetY, 230, 250), "Saving Options" );

		GUI.Label (new Rect (offsetX + 20, offsetY + 20, 100, 25), "File Format");

		bmpSelected = GUI.Toggle (new Rect (offsetX + 30, offsetY + 40, 80, 20), bmpSelected, "BMP");
		if (bmpSelected) {
			SetFormat (FileFormat.bmp);
		}

		jpgSelected = GUI.Toggle (new Rect (offsetX + 30, offsetY + 60, 80, 20), jpgSelected, "JPG");
		if (jpgSelected) {
			SetFormat (FileFormat.jpg);
		}

		pngSelected = GUI.Toggle (new Rect (offsetX + 30, offsetY + 80, 80, 20), pngSelected, "PNG");
		if (pngSelected) {
			SetFormat (FileFormat.png);
		}

		tgaSelected = GUI.Toggle (new Rect (offsetX + 30, offsetY + 100, 80, 20), tgaSelected, "TGA");
		if (tgaSelected) {
			SetFormat (FileFormat.tga);
		}

		tiffSelected = GUI.Toggle (new Rect (offsetX + 30, offsetY + 120, 80, 20), tiffSelected, "TIFF");
		if (tiffSelected) {
			SetFormat (FileFormat.tiff);
		}

		// Flip Normal Map Y
		if ( _NormalMap == null ){ GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX + 10, offsetY + 145, 100, 25), "Flip Normal Y")) {
			FlipNormalMapY();
		}
		GUI.enabled = true;

		//Save Project
		if (GUI.Button (new Rect(offsetX + 10, offsetY + 180, 100, 25), "Save Project")) {
			SetFileMaskProject();
			fileBrowser.ShowBrowser( "Save Project", this.SaveProject );
		}

		//Load Project
		if (GUI.Button (new Rect(offsetX + 10, offsetY + 215, 100, 25), "Load Project")) {
			SetFileMaskProject();
			fileBrowser.ShowBrowser( "Load Project", this.LoadProject );
		}

		//======================================//
		//			Property Map Settings		//
		//======================================//

		GUI.Label (new Rect (offsetX + 130, offsetY + 20, 100, 25), "Property Map");

		if (propRedChoose) { GUI.enabled = false; } else { GUI.enabled = true; }
		GUI.Label( new Rect (offsetX + 100, offsetY + 45, 20, 20), "R:" );
		if (GUI.Button ( new Rect (offsetX + 120, offsetY + 45, 100, 25), PCM2String( propRed, "Red None" ) ) ) {
			propRedChoose = true;
			propGreenChoose = false;
			propBlueChoose = false;
		}

		if (propGreenChoose) { GUI.enabled = false; } else { GUI.enabled = true; }
		GUI.Label( new Rect (offsetX + 100, offsetY + 80, 20, 20), "G:" );
		if (GUI.Button ( new Rect (offsetX + 120, offsetY + 80, 100, 25), PCM2String( propGreen, "Green None" ) ) ) {
			propRedChoose = false;
			propGreenChoose = true;
			propBlueChoose = false;
		}

		if (propBlueChoose) { GUI.enabled = false; } else { GUI.enabled = true; }
		GUI.Label( new Rect (offsetX + 100, offsetY + 115, 20, 20), "B:" );
		if (GUI.Button ( new Rect (offsetX + 120, offsetY + 115, 100, 25), PCM2String( propBlue, "Blue None" ) ) ) {
			propRedChoose = false;
			propGreenChoose = false;
			propBlueChoose = true;
		}

		GUI.enabled = true;

		int propBoxOffsetX = offsetX + 250;
		int propBoxOffsetY = 20;
		if (propRedChoose || propGreenChoose || propBlueChoose) {
			GUI.Box( new Rect (propBoxOffsetX, propBoxOffsetY, 150, 245), "Map for Channel" );
			bool chosen = false;
			PropChannelMap chosenPCM = PropChannelMap.None;

			if (GUI.Button (new Rect (propBoxOffsetX + 10, propBoxOffsetY + 30, 130, 25), "None")) {
				chosen = true;
				chosenPCM = PropChannelMap.None;
			}
			if (GUI.Button (new Rect (propBoxOffsetX + 10, propBoxOffsetY + 60, 130, 25), "Height")) {
				chosen = true;
				chosenPCM = PropChannelMap.Height;
			}
			if (GUI.Button (new Rect (propBoxOffsetX + 10, propBoxOffsetY + 90, 130, 25), "Metallic")) {
				chosen = true;
				chosenPCM = PropChannelMap.Metallic;
			}
			if (GUI.Button (new Rect (propBoxOffsetX + 10, propBoxOffsetY + 120, 130, 25), "Smoothness")) {
				chosen = true;
				chosenPCM = PropChannelMap.Smoothness;
			}
			if (GUI.Button (new Rect (propBoxOffsetX + 10, propBoxOffsetY + 150, 130, 25), "Edge")) {
				chosen = true;
				chosenPCM = PropChannelMap.Edge;
			}
			if (GUI.Button (new Rect (propBoxOffsetX + 10, propBoxOffsetY + 180, 130, 25), "Ambient Occlusion")) {
				chosen = true;
				chosenPCM = PropChannelMap.Ao;
			}
			if (GUI.Button (new Rect (propBoxOffsetX + 10, propBoxOffsetY + 210, 130, 25), "AO + Edge")) {
				chosen = true;
				chosenPCM = PropChannelMap.AoEdge;
			}

			if( chosen ){
				if( propRedChoose ){
					propRed = chosenPCM;
				}
				if( propGreenChoose ){
					propGreen = chosenPCM;
				}
				if( propBlueChoose ){
					propBlue = chosenPCM;
				}
				propRedChoose = false;
				propGreenChoose = false;
				propBlueChoose = false;
			}
		}

		if (GUI.Button (new Rect(offsetX + 120, offsetY + 150, 100, 40), "Save\r\nProperty Map")) {
			ProcessPropertyMap();
			textureToSave = _PropertyMap;
			mapType = "_msao";
			SetFileMaskImage();
			fileBrowser.ShowBrowser( "Save Property Map", this.SaveFile );
		}

		if( QuicksavePathProperty == "" ){ GUI.enabled = false; }
		if (GUI.Button (new Rect(offsetX + 120, offsetY + 200, 100, 40), "Quick Save\r\nProperty Map")) {
			ProcessPropertyMap();
			textureToSave = _PropertyMap;
			mapType = "_msao";
			SaveFile(QuicksavePathProperty);
		}
		GUI.enabled = true;


		//==========================//
		// 		View Buttons		//
		//==========================//

		offsetX = 430;
		offsetY = 280;

		if (GUI.Button (new Rect(offsetX, offsetY, 100, 40), "Post Process")) {
			if( PostProcessGuiObject.activeSelf == true ){
				PostProcessGuiObject.SetActive(false);
			}else{
				PostProcessGuiObject.SetActive(true);
			}
		}

		offsetX += 110;

		if (GUI.Button (new Rect(offsetX, offsetY, 80, 40), "Show Full\r\nMaterial")) {
			CloseWindows();
			FixSize();
			MaterialGuiObject.SetActive(true);
			MaterialGuiScript.Initialize();
		}

		offsetX += 90;

		if (GUI.Button (new Rect(offsetX, offsetY, 80, 40), "Next\r\nCube Map")) {
			selectedCubemap += 1;
			if( selectedCubemap >= CubeMaps.Length ){
				selectedCubemap = 0;
			}

            //skyboxMaterial.SetTexture ("_Tex", CubeMaps[selectedCubemap] );
			Shader.SetGlobalTexture ("_GlobalCubemap", CubeMaps[selectedCubemap] );
			reflectionProbe.RenderProbe();
		}

		offsetX += 90;

		if (_HeightMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX, offsetY, 60, 40), "Tile\r\nMaps")) {
			CloseWindows();
			FixSize();
			TilingTextureMakerGuiObject.SetActive(true);
			TilingTextureMakerGuiScript.Initialize();
		}
		GUI.enabled = true;

		offsetX += 70;

		if (_HeightMap == null && _DiffuseMapOriginal == null && _MetallicMap == null && _SmoothnessMap == null && _EdgeMap == null && _AOMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect(offsetX, offsetY, 90, 40), "Adjust\r\nAlignment")) {
			CloseWindows();
			FixSize();
			//AlignmentGuiScript.gameObject.SetActive(true);
			AlignmentGuiScript.Initialize();
		}
		GUI.enabled = true;

		offsetX += 100;

		if (GUI.Button (new Rect(offsetX, offsetY, 120, 40), "Clear All\r\nTexture Maps")) {
			clearTextures = true;
		}

		if (clearTextures) {

			offsetY += 60;

			GUI.Box( new Rect (offsetX, offsetY, 120, 60), "Are You Sure?" );

			if (GUI.Button (new Rect (offsetX + 10, offsetY + 30, 45, 20), "Yes")) {
				clearTextures = false;
				ClearAllTextures ();
				CloseWindows ();
				SetMaterialValues ();
				FixSizeSize( 1024.0f, 1024.0f );
			}

			if (GUI.Button (new Rect (offsetX + 65, offsetY + 30, 45, 20), "No")) {
				clearTextures = false;
			}
		}

		GUI.enabled = true;

	}

	string PCM2String ( PropChannelMap pcm, string defaultName ){

		string returnString = defaultName;

		switch (pcm) {
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
		}

		return returnString;

	}

	public void FlipNormalMapY(){

		if( _NormalMap != null ){
			Color pixelColor = Color.black;
			//UnityEngine.Debug.Log (_NormalMap.GetPixel (0, 0).b);
			for (int i = 0; i < _NormalMap.width; i ++) {
				for (int j = 0; j < _NormalMap.height; j ++) {
					pixelColor = _NormalMap.GetPixel (i, j);
					pixelColor.g = 1.0f - pixelColor.g;
					_NormalMap.SetPixel (i, j, pixelColor);
				}
			}
			_NormalMap.Apply ();
		}

		/*
		bool SPM = false;
		if (SampleMaterial.GetTexture ("_MainTex") == _NormalMap) {
			SPM = true;
		}

		Shader FlipNormalYShader = Shader.Find ("Hidden/Blit_FlipNormalY");
		Material FlipNormalYMaterial = new Material (FlipNormalYShader);

		RenderTexture _TempMap = RenderTexture.GetTemporary (_NormalMap.width, _NormalMap.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		Graphics.Blit (_NormalMap, _TempMap, FlipNormalYMaterial, 0);
		RenderTexture.active = _TempMap;
		
		if (_NormalMap != null) {
			Destroy (_NormalMap);
			_NormalMap = null;
		}
		
		_NormalMap = new Texture2D (_TempMap.width, _TempMap.height);
		_NormalMap.ReadPixels (new Rect (0, 0, _TempMap.width, _TempMap.height), 0, 0);
		_NormalMap.Apply ();

		Destroy (FlipNormalYShader);
		Destroy (FlipNormalYMaterial);
		
		RenderTexture.ReleaseTemporary (_TempMap);
		_TempMap = null;

		if (MaterialGuiObject.activeSelf) {
			SetMaterialValues ();
			FullMaterial.SetTexture ("_Normal", _NormalMap);
			MaterialGuiScript.Initialize();
		}else if (SPM) {
			SetPreviewMaterial (_NormalMap);
		}
		*/

	}

	void ClearTexture( Texture2D textureToClear ){

		if (textureToClear) {
			Destroy (textureToClear);
			textureToClear = null;
		}

		Resources.UnloadUnusedAssets();
	}

	void ClearTexture( MapType mapType ){
		
		switch (mapType) {
		case MapType.height:
			if (_HeightMap) {
				Destroy (_HeightMap);
				_HeightMap = null;
			}
			if (_HDHeightMap) {
				Destroy (_HDHeightMap);
				_HDHeightMap = null;
			}
			break;
		case MapType.diffuse:
			if (_DiffuseMap) {
				Destroy (_DiffuseMap);
				_DiffuseMap = null;
			}
			if (_DiffuseMapOriginal) {
				Destroy (_DiffuseMapOriginal);
				_DiffuseMapOriginal = null;
			}
			break;
		case MapType.normal:
			if (_NormalMap) {
				Destroy (_NormalMap);
				_NormalMap = null;
			}
			break;
		case MapType.metallic:
			if (_MetallicMap) {
				Destroy (_MetallicMap);
				_MetallicMap = null;
			}
			break;
		case MapType.smoothness:
			if (_SmoothnessMap) {
				Destroy (_SmoothnessMap);
				_SmoothnessMap = null;
			}
			break;
		case MapType.edge:
			if (_EdgeMap) {
				Destroy (_EdgeMap);
				_EdgeMap = null;
			}
			break;
		case MapType.ao:
			if (_AOMap) {
				Destroy (_AOMap);
				_AOMap = null;
			}
			break;
		}

		Resources.UnloadUnusedAssets();
	}

	public void ClearAllTextures() {

		ClearTexture( MapType.height );
		ClearTexture( MapType.diffuse );
		ClearTexture( MapType.normal );
		ClearTexture( MapType.metallic );
		ClearTexture( MapType.smoothness );
		ClearTexture( MapType.edge );
		ClearTexture( MapType.ao );

	}

	public void SetFormat ( FileFormat newFormat ) {

		bmpSelected = false;
		jpgSelected = false;
		pngSelected = false;
		tgaSelected = false;
		tiffSelected = false;

		switch (newFormat) {
		case FileFormat.bmp:
			bmpSelected = true;
			break;
		case FileFormat.jpg:
			jpgSelected = true;
			break;
		case FileFormat.png:
			pngSelected = true;
			break;
		case FileFormat.tga:
			tgaSelected = true;
			break;
		case FileFormat.tiff:
			tiffSelected = true;
			break;
		}

		selectedFormat = newFormat;
	}

	public void SetFormat ( string newFormat ) {
		
		bmpSelected = false;
		jpgSelected = false;
		pngSelected = false;
		tgaSelected = false;
		tiffSelected = false;
		
		switch (newFormat) {
		case "bmp":
			bmpSelected = true;
			selectedFormat = FileFormat.bmp;
			break;
		case "jpg":
			jpgSelected = true;
			selectedFormat = FileFormat.jpg;
			break;
		case "png":
			pngSelected = true;
			selectedFormat = FileFormat.png;
			break;
		case "tga":
			tgaSelected = true;
			selectedFormat = FileFormat.tga;
			break;
		case "tiff":
			tiffSelected = true;
			selectedFormat = FileFormat.tiff;
			break;
		}

	}

	public void SetLoadedTexture( MapType loadedTexture ){

		//SetMaterialValues ();

		switch( loadedTexture ){
		case MapType.height:
			SetPreviewMaterial (_HeightMap);
			break;
		case MapType.diffuse:
			SetPreviewMaterial (_DiffuseMap);
			break;
		case MapType.diffuseOriginal:
			SetPreviewMaterial (_DiffuseMapOriginal);
			break;
		case MapType.normal:
			SetPreviewMaterial (_NormalMap);
			break;
		case MapType.metallic:
			SetPreviewMaterial (_MetallicMap);
			break;
		case MapType.smoothness:
			SetPreviewMaterial (_SmoothnessMap);
			break;
		case MapType.edge:
			SetPreviewMaterial (_EdgeMap);
			break;
		case MapType.ao:
			SetPreviewMaterial (_AOMap);
			break;
		default:
			break;
		}

		FixSize ();
	}

	string SwitchFormats(  FileFormat selectedFormat ) {

		string extension = "bmp";
		switch (selectedFormat) {
		case FileFormat.bmp:
			extension = "bmp";
			break;
		case FileFormat.jpg:
			extension = "jpg";
			break;
		case FileFormat.png:
			extension = "png";
			break;
		case FileFormat.tga:
			extension = "tga";
			break;
		case FileFormat.tiff:
			extension = "tiff";
			break;
		}

		return extension;
	}

	//==================================================//
	//					Property Map					//
	//==================================================//

	void SetPropertyTexture ( string texPrefix, Texture2D texture, Texture2D overlayTexture ){

		if (texture != null) {
			PropertyCompMaterial.SetTexture (texPrefix + "Tex", texture);
		} else {
			PropertyCompMaterial.SetTexture (texPrefix + "Tex", _TextureBlack);
		}

		PropertyCompMaterial.SetTexture (texPrefix + "OverlayTex", overlayTexture);

	}

	void SetPropertyMapChannel ( string texPrefix, PropChannelMap pcm ){

		switch (pcm) {
		case PropChannelMap.Height:
			SetPropertyTexture (texPrefix, _HeightMap, _TextureGrey);
			break;
		case PropChannelMap.Metallic:
			SetPropertyTexture (texPrefix, _MetallicMap, _TextureGrey);
			break;
		case PropChannelMap.Smoothness:
			SetPropertyTexture (texPrefix, _SmoothnessMap, _TextureGrey);
			break;
		case PropChannelMap.Edge:
			SetPropertyTexture (texPrefix, _EdgeMap, _TextureGrey);
			break;
		case PropChannelMap.Ao:
			SetPropertyTexture (texPrefix, _AOMap, _TextureGrey);
			break;
		case PropChannelMap.AoEdge:
			SetPropertyTexture (texPrefix, _AOMap, _EdgeMap);
			break;
		case PropChannelMap.None:
			SetPropertyTexture (texPrefix, _TextureBlack, _TextureGrey);
			break;
		}

	}

	public void ProcessPropertyMap () {

		SetPropertyMapChannel ("_Red", propRed);
		SetPropertyMapChannel ("_Green", propGreen);
		SetPropertyMapChannel ("_Blue", propBlue);

		Vector2 size = GetSize ();
		RenderTexture _TempMap = RenderTexture.GetTemporary ((int)size.x, (int)size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		Graphics.Blit (_MetallicMap, _TempMap, PropertyCompMaterial, 0);
		RenderTexture.active = _TempMap;
		
		if (_PropertyMap != null) {
			Destroy (_PropertyMap);
			_PropertyMap = null;
		}

		_PropertyMap = new Texture2D (_TempMap.width, _TempMap.height, TextureFormat.RGB24, false);
		_PropertyMap.ReadPixels (new Rect (0, 0, _TempMap.width, _TempMap.height), 0, 0);
		_PropertyMap.Apply ();

		RenderTexture.ReleaseTemporary (_TempMap);
		_TempMap = null;

	}

	//==================================================//
	//					Project Saving					//
	//==================================================//

	void SaveProject (string pathToFile) {
		SaveLoadProjectScript.SaveProject (pathToFile, selectedFormat);
	}

	void LoadProject (string pathToFile) {
		SaveLoadProjectScript.LoadProject (pathToFile);
	}
	
	void SaveFile (string pathToFile) {
		SaveLoadProjectScript.SaveFile(pathToFile,selectedFormat,textureToSave, "" );
	}

	void CopyFile() {
		SaveLoadProjectScript.CopyFile (textureToSave);
	}

	void PasteFile() {
		ClearTexture (mapTypeToLoad);
		SaveLoadProjectScript.PasteFile (mapTypeToLoad);
	}

	void OpenFile (string pathToFile) {
		if (pathToFile == null) {
			return;
		}

		// clear the existing texture we are loading
		ClearTexture(mapTypeToLoad);

		StartCoroutine ( SaveLoadProjectScript.LoadTexture( mapTypeToLoad, pathToFile ) );
	}

	//==================================================//
	//			Fix the size of the test model			//
	//==================================================//

	
	public Vector2 GetSize() {

			Texture2D mapToUse = null;

			Vector2 size = new Vector2 (1024, 1024);
			
			if (_HeightMap != null) {
				mapToUse = _HeightMap;
			} else if (_DiffuseMap != null) {
				mapToUse = _DiffuseMap;
			} else if (_DiffuseMapOriginal != null) {
				mapToUse = _DiffuseMapOriginal;
			} else if (_NormalMap != null) {
				mapToUse = _NormalMap;
			} else if (_MetallicMap != null) {
				mapToUse = _MetallicMap;
			} else if (_SmoothnessMap != null) {
				mapToUse = _SmoothnessMap;
			} else if (_EdgeMap != null) {
				mapToUse = _EdgeMap;
			} else if (_AOMap != null) {
				mapToUse = _AOMap;
			}
			
			if (mapToUse != null) {
				size.x = mapToUse.width;
				size.y = mapToUse.height;
			} 

			return size;
	}

	public void FixSize() {
		
		Vector2 size = GetSize ();
		FixSizeSize( size.x, size.y );
		
	}

	void FixSizeMap( Texture2D mapToUse ) {
		FixSizeSize ( (float)mapToUse.width, (float)mapToUse.height );
	}

	void FixSizeMap( RenderTexture mapToUse ) {
		FixSizeSize ( (float)mapToUse.width, (float)mapToUse.height );
	}

	void FixSizeSize( float width, float height ) {
		
		Vector3 testObjectScale = new Vector3(1,1,1);
		float area = 1.0f;
		
		testObjectScale.x = width / height;
		
		float newArea = testObjectScale.x * testObjectScale.y;
		float areaScale = Mathf.Sqrt( area / newArea );

		testObjectScale.x *= areaScale;
		testObjectScale.y *= areaScale;
		
		testObject.transform.localScale = testObjectScale;
		
	}
}