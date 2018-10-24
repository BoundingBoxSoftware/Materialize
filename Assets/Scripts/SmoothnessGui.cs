using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class SmoothnessSettings {
	
	//[DefaultValueAttribute(Color.black)]
	public Color SampleColor1;
	//[DefaultValueAttribute(Vector2.zero)]
	public Vector2 SampleUV1;

	//[DefaultValueAttribute(Color.black)]
	public Color SampleColor2;
	//[DefaultValueAttribute(Vector2.zero)]
	public Vector2 SampleUV2;
	
	//[DefaultValueAttribute(Color.black)]
	public Color SampleColor3;
	//[DefaultValueAttribute(Vector2.zero)]
	public Vector2 SampleUV3;

	[DefaultValueAttribute(0.7f)]
	public float MetalSmoothness;
	[DefaultValueAttribute("0.7")]
	public string MetalSmoothnessText;
	
	[DefaultValueAttribute(false)]
	public bool UseSample1;
	[DefaultValueAttribute(false)]
	public bool IsolateSample1;
	[DefaultValueAttribute(1.0f)]
	public float HueWeight1;
	[DefaultValueAttribute(0.5f)]
	public float SatWeight1;
	[DefaultValueAttribute(0.2f)]
	public float LumWeight1;
	[DefaultValueAttribute(0.0f)]
	public float MaskLow1;
	[DefaultValueAttribute(1.0f)]
	public float MaskHigh1;
	[DefaultValueAttribute(0.5f)]
	public float Sample1Smoothness;
	
	[DefaultValueAttribute(false)]
	public bool UseSample2;
	[DefaultValueAttribute(false)]
	public bool IsolateSample2;
	[DefaultValueAttribute(1.0f)]
	public float HueWeight2;
	[DefaultValueAttribute(0.5f)]
	public float SatWeight2;
	[DefaultValueAttribute(0.2f)]
	public float LumWeight2;
	[DefaultValueAttribute(0.0f)]
	public float MaskLow2;
	[DefaultValueAttribute(1.0f)]
	public float MaskHigh2;
	[DefaultValueAttribute(0.3f)]
	public float Sample2Smoothness;

	[DefaultValueAttribute(false)]
	public bool UseSample3;
	[DefaultValueAttribute(false)]
	public bool IsolateSample3;
	[DefaultValueAttribute(1.0f)]
	public float HueWeight3;
	[DefaultValueAttribute(0.5f)]
	public float SatWeight3;
	[DefaultValueAttribute(0.2f)]
	public float LumWeight3;
	[DefaultValueAttribute(0.0f)]
	public float MaskLow3;
	[DefaultValueAttribute(1.0f)]
	public float MaskHigh3;
	[DefaultValueAttribute(0.2f)]
	public float Sample3Smoothness;

	[DefaultValueAttribute(0.1f)]
	public float BaseSmoothness;
	[DefaultValueAttribute("0.1")]
	public string BaseSmoothnessText;

	[DefaultValueAttribute(0)]
	public int BlurSize;
	[DefaultValueAttribute("0")]
	public string BlurSizeText;

	[DefaultValueAttribute(30)]
	public int OverlayBlurSize;
	[DefaultValueAttribute("30")]
	public string OverlayBlurSizeText;

	[DefaultValueAttribute(3.0f)]
	public float BlurOverlay;
	[DefaultValueAttribute("3")]
	public string BlurOverlayText;
	
	[DefaultValueAttribute(1.0f)]
	public float FinalContrast;
	[DefaultValueAttribute("1")]
	public string FinalContrastText;

	[DefaultValueAttribute(0.0f)]
	public float FinalBias;
	[DefaultValueAttribute("0")]
	public string FinalBiasText;

	[DefaultValueAttribute(false)]
	public bool useAdjustedDiffuse;
	[DefaultValueAttribute(true)]
	public bool useOriginalDiffuse;

	public SmoothnessSettings(){

		this.SampleColor1 = Color.black;
		this.SampleUV1 = Vector2.zero;

		this.SampleColor2 = Color.black;
		this.SampleUV2 = Vector2.zero;

		this.SampleColor3 = Color.black;
		this.SampleUV3 = Vector2.zero;

		this.MetalSmoothness = 0.7f;
		this.MetalSmoothnessText = "0.7";

		this.UseSample1 = false;
		this.IsolateSample1 = false;
		this.HueWeight1 = 1.0f;
		this.SatWeight1 = 0.5f;
		this.LumWeight1 = 0.2f;
		this.MaskLow1 = 0.0f;
		this.MaskHigh1 = 1.0f;
		this.Sample1Smoothness = 0.5f;

		this.UseSample2 = false;
		this.IsolateSample2 = false;
		this.HueWeight2 = 1.0f;
		this.SatWeight2 = 0.5f;
		this.LumWeight2 = 0.2f;
		this.MaskLow2 = 0.0f;
		this.MaskHigh2 = 1.0f;
		this.Sample2Smoothness = 0.3f;

		this.UseSample3 = false;
		this.IsolateSample3 = false;
		this.HueWeight3 = 1.0f;
		this.SatWeight3 = 0.5f;
		this.LumWeight3 = 0.2f;
		this.MaskLow3 = 0.0f;
		this.MaskHigh3 = 1.0f;
		this.Sample3Smoothness = 0.2f;

		this.BaseSmoothness = 0.1f;
		this.BaseSmoothnessText = "0.1";

		this.BlurSize = 0;
		this.BlurSizeText = "0";

		this.OverlayBlurSize = 30;
		this.OverlayBlurSizeText = "30";

		this.BlurOverlay = 3.0f;
		this.BlurOverlayText = "3";

		this.FinalContrast = 1.0f;
		this.FinalContrastText = "1";

		this.FinalBias = 0.0f;
		this.FinalBiasText = "0";

		this.useAdjustedDiffuse = false;
		this.useOriginalDiffuse = true;
	}
	
}

public class SmoothnessGui : MonoBehaviour {

	public MainGui MainGuiScript;

	public Texture2D _DefaultMetallicMap;

	Texture2D _DiffuseMap;
	Texture2D _DiffuseMapOriginal;

	Texture2D _MetallicMap;
	Texture2D _SmoothnessMap;

	Texture2D _SampleColorMap1;
	Texture2D _SampleColorMap2;
	Texture2D _SampleColorMap3;

	RenderTexture _TempMap;
	RenderTexture _BlurMap;
	RenderTexture _OverlayBlurMap;

	public Material thisMaterial;
	Material blitMaterial;
	Material blitSmoothnessMaterial;

	int imageSizeX;
	int imageSizeY;

	int currentSelection = 0;

	SmoothnessSettings SS;
	
	float Slider = 0.5f;
	bool lastUseAdjustedDiffuse = false;

	Rect windowRect = new Rect (30, 300, 300, 530);
	
	public GameObject testObject;
	bool doStuff = false;
	bool newTexture = false;

	bool selectingColor = false;
	bool mouseButtonDown = false;

	bool settingsInitialized = false;

	public bool busy = false;

	public void GetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		projectObject.SS = SS;
	}
	
	public void SetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		if (projectObject.SS != null) {
			SS = projectObject.SS;
		} else {
			settingsInitialized = false;
			InitializeSettings ();
		}

		_SampleColorMap1.SetPixel (1, 1, SS.SampleColor1);
		_SampleColorMap1.Apply ();

		_SampleColorMap2.SetPixel (1, 1, SS.SampleColor2);
		_SampleColorMap2.Apply ();

		_SampleColorMap3.SetPixel (1, 1, SS.SampleColor3);
		_SampleColorMap3.Apply ();

		doStuff = true;

	}
	
	void InitializeSettings() {
		
		if (settingsInitialized == false) {
			
			SS = new SmoothnessSettings ();

			_SampleColorMap1 = new Texture2D (1, 1, TextureFormat.ARGB32, false, true);
			_SampleColorMap1.SetPixel (1, 1, SS.SampleColor1);
			_SampleColorMap1.Apply ();
			
			_SampleColorMap2 = new Texture2D (1, 1, TextureFormat.ARGB32, false, true);
			_SampleColorMap2.SetPixel (1, 1, SS.SampleColor2);
			_SampleColorMap2.Apply ();
			
			_SampleColorMap3 = new Texture2D (1, 1, TextureFormat.ARGB32, false, true);
			_SampleColorMap3.SetPixel (1, 1, SS.SampleColor3);
			_SampleColorMap3.Apply ();
			
			settingsInitialized = true;
		}
		
	}

	// Use this for initialization
	void Start () {
		
		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		
		blitMaterial = new Material (Shader.Find ("Hidden/Blit_Shader"));
		blitSmoothnessMaterial = new Material (Shader.Find ("Hidden/Blit_Smoothness"));

		InitializeSettings ();
		
	}

	public void DoStuff() {
		doStuff = true;
	}
	
	public void NewTexture() {
		newTexture = true;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (selectingColor) {
			SelectColor();
		}

		if (newTexture) {
			InitializeTextures();
			newTexture = false;
		}

		if (SS.useAdjustedDiffuse != lastUseAdjustedDiffuse) {
			lastUseAdjustedDiffuse = SS.useAdjustedDiffuse;
			doStuff = true;
		}
		
		if (doStuff) {
			StartCoroutine( ProcessBlur () );
			doStuff = false;
		}
		
		//thisMaterial.SetFloat ("_BlurWeight", BlurWeight);

		thisMaterial.SetFloat ( "_MetalSmoothness", SS.MetalSmoothness);

		if (SS.IsolateSample1) {
			thisMaterial.SetInt ("_IsolateSample1", 1);
		} else {
			thisMaterial.SetInt ("_IsolateSample1", 0);
		}
		if( SS.UseSample1){
			thisMaterial.SetInt ("_UseSample1", 1);
		} else {
			thisMaterial.SetInt ("_UseSample1", 0);
		}
		thisMaterial.SetColor ( "_SampleColor1", SS.SampleColor1);
		thisMaterial.SetVector ( "_SampleUV1", new Vector4 (SS.SampleUV1.x, SS.SampleUV1.y, 0, 0));
		thisMaterial.SetFloat ( "_HueWeight1", SS.HueWeight1 );
		thisMaterial.SetFloat ( "_SatWeight1", SS.SatWeight1 );
		thisMaterial.SetFloat ( "_LumWeight1", SS.LumWeight1 );
		thisMaterial.SetFloat ( "_MaskLow1", SS.MaskLow1 );
		thisMaterial.SetFloat ( "_MaskHigh1", SS.MaskHigh1 );
		thisMaterial.SetFloat ( "_Sample1Smoothness", SS.Sample1Smoothness);

		if (SS.IsolateSample2) {
			thisMaterial.SetInt ("_IsolateSample2", 1);
		} else {
			thisMaterial.SetInt ("_IsolateSample2", 0);
		}
		if( SS.UseSample2){
			thisMaterial.SetInt ("_UseSample2", 1);
		} else {
			thisMaterial.SetInt ("_UseSample2", 0);
		}
		thisMaterial.SetColor ( "_SampleColor2", SS.SampleColor2);
		thisMaterial.SetVector ( "_SampleUV2", new Vector4 (SS.SampleUV2.x, SS.SampleUV2.y, 0, 0));
		thisMaterial.SetFloat ( "_HueWeight2", SS.HueWeight2 );
		thisMaterial.SetFloat ( "_SatWeight2", SS.SatWeight2 );
		thisMaterial.SetFloat ( "_LumWeight2", SS.LumWeight2 );
		thisMaterial.SetFloat ( "_MaskLow2", SS.MaskLow2 );
		thisMaterial.SetFloat ( "_MaskHigh2", SS.MaskHigh2 );
		thisMaterial.SetFloat ( "_Sample2Smoothness", SS.Sample2Smoothness);

		if (SS.IsolateSample3) {
			thisMaterial.SetInt ("_IsolateSample3", 1);
		} else {
			thisMaterial.SetInt ("_IsolateSample3", 0);
		}
		if( SS.UseSample3){
			thisMaterial.SetInt ("_UseSample3", 1);
		} else {
			thisMaterial.SetInt ("_UseSample3", 0);
		}
		thisMaterial.SetColor ( "_SampleColor3", SS.SampleColor3);
		thisMaterial.SetVector ( "_SampleUV3", new Vector4 (SS.SampleUV3.x, SS.SampleUV3.y, 0, 0));
		thisMaterial.SetFloat ( "_HueWeight3", SS.HueWeight3 );
		thisMaterial.SetFloat ( "_SatWeight3", SS.SatWeight3 );
		thisMaterial.SetFloat ( "_LumWeight3", SS.LumWeight3 );
		thisMaterial.SetFloat ( "_MaskLow3", SS.MaskLow3 );
		thisMaterial.SetFloat ( "_MaskHigh3", SS.MaskHigh3 );
		thisMaterial.SetFloat ( "_Sample3Smoothness", SS.Sample3Smoothness);

		thisMaterial.SetFloat ( "_BaseSmoothness", SS.BaseSmoothness);
		
		thisMaterial.SetFloat ( "_Slider", Slider);
		thisMaterial.SetFloat ( "_BlurOverlay", SS.BlurOverlay);	
		thisMaterial.SetFloat ( "_FinalContrast", SS.FinalContrast);
		thisMaterial.SetFloat ( "_FinalBias", SS.FinalBias); 

		if ( SS.useAdjustedDiffuse ) {
			thisMaterial.SetTexture ("_MainTex", _DiffuseMap);
		} else {
			thisMaterial.SetTexture ("_MainTex", _DiffuseMapOriginal);
		}
		
	}

	void SelectColor() {
		if ( Input.GetMouseButton(0) ) {
			
			mouseButtonDown = true;
			
			RaycastHit hit;
			if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
				return;
			
			Renderer rend = hit.transform.GetComponent<Renderer>();
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
				return;
			
			Vector2 pixelUV = hit.textureCoord;

			Color sampledColor = Color.black;
			if( SS.useAdjustedDiffuse){
				sampledColor = _DiffuseMap.GetPixelBilinear(pixelUV.x, pixelUV.y);
			}else{
				sampledColor = _DiffuseMapOriginal.GetPixelBilinear(pixelUV.x, pixelUV.y);
			}
			
			if( currentSelection == 1 ){
				SS.SampleUV1 = pixelUV;
				SS.SampleColor1 = sampledColor;
				_SampleColorMap1.SetPixel (1, 1, SS.SampleColor1);
				_SampleColorMap1.Apply ();
			}

			if( currentSelection == 2 ){
				SS.SampleUV2 = pixelUV;
				SS.SampleColor2 = sampledColor;
				_SampleColorMap2.SetPixel (1, 1, SS.SampleColor2);
				_SampleColorMap2.Apply ();
			}

			if( currentSelection == 3 ){
				SS.SampleUV3 = pixelUV;
				SS.SampleColor3 = sampledColor;
				_SampleColorMap3.SetPixel (1, 1, SS.SampleColor3);
				_SampleColorMap3.Apply ();
			}
			
		}
		
		if ( Input.GetMouseButtonUp(0) && mouseButtonDown ) {
			
			mouseButtonDown = false;
			selectingColor = false;
			currentSelection = 0;
			
		}
		
	}

	void DoMyWindow ( int windowID ) {
		
		int spacingX = 0;
		int spacingY = 50;
		int spacing2Y = 70;
		
		int offsetX = 10;
		int offsetY = 30;

		if (_DiffuseMap != null) { GUI.enabled = true; } else { GUI.enabled = false; }
		if( GUI.Toggle (new Rect (offsetX, offsetY, 140, 30), SS.useAdjustedDiffuse, " Use Edited Diffuse")) {
			SS.useAdjustedDiffuse = true;
			SS.useOriginalDiffuse = false;
		}
		GUI.enabled = true;
		if (GUI.Toggle (new Rect (offsetX + 150, offsetY, 140, 30), SS.useOriginalDiffuse, " Use Original Diffuse")) {
			SS.useAdjustedDiffuse = false;
			SS.useOriginalDiffuse = true;
		}

		offsetY += 30;

		GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Smoothness Reveal Slider" );
		Slider = GUI.HorizontalSlider( new Rect( offsetX, offsetY + 20, 280, 10 ),Slider,0.0f, 1.0f );
		offsetY += 40;

		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Metal Smoothness", SS.MetalSmoothness, SS.MetalSmoothnessText, out SS.MetalSmoothness, out SS.MetalSmoothnessText, 0.0f, 1.0f );
		offsetY += 40;

		SS.UseSample1 = GUI.Toggle (new Rect (offsetX, offsetY, 150, 20), SS.UseSample1, "Use Color Sample 1");
		if (SS.UseSample1) {

			SS.IsolateSample1 = GUI.Toggle (new Rect (offsetX + 180, offsetY, 150, 20), SS.IsolateSample1, "Isolate Mask");
			if( SS.IsolateSample1 ){
				SS.IsolateSample2 = false;
				SS.IsolateSample3 = false;
			}
			offsetY += 30;

			if ( GUI.Button (new Rect (offsetX, offsetY + 5, 80, 20), "Pick Color") ) {
				selectingColor = true;
				currentSelection = 1;
			}
		
			GUI.DrawTexture (new Rect (offsetX + 10, offsetY + 35, 60, 60), _SampleColorMap1);
		
			GUI.Label (new Rect (offsetX + 90, offsetY, 250, 30), "Hue");
			SS.HueWeight1 = GUI.VerticalSlider (new Rect (offsetX + 95, offsetY + 30, 10, 70), SS.HueWeight1, 1.0f, 0.0f);
		
			GUI.Label (new Rect (offsetX + 120, offsetY, 250, 30), "Sat");
			SS.SatWeight1 = GUI.VerticalSlider (new Rect (offsetX + 125, offsetY + 30, 10, 70), SS.SatWeight1, 1.0f, 0.0f);
		
			GUI.Label (new Rect (offsetX + 150, offsetY, 250, 30), "Lum");
			SS.LumWeight1 = GUI.VerticalSlider (new Rect (offsetX + 155, offsetY + 30, 10, 70), SS.LumWeight1, 1.0f, 0.0f);
		
			GUI.Label (new Rect (offsetX + 180, offsetY, 250, 30), "Low");
			SS.MaskLow1 = GUI.VerticalSlider (new Rect (offsetX + 185, offsetY + 30, 10, 70), SS.MaskLow1, 1.0f, 0.0f);
		
			GUI.Label (new Rect (offsetX + 210, offsetY, 250, 30), "High");
			SS.MaskHigh1 = GUI.VerticalSlider (new Rect (offsetX + 215, offsetY + 30, 10, 70), SS.MaskHigh1, 1.0f, 0.0f);

			GUI.Label (new Rect (offsetX + 240, offsetY, 250, 30), "Smooth");
			SS.Sample1Smoothness = GUI.VerticalSlider (new Rect (offsetX + 255, offsetY + 30, 10, 70), SS.Sample1Smoothness, 1.0f, 0.0f);
		
			offsetY += 110;
		} else {
			offsetY += 30;
			SS.IsolateSample1 = false;
		}


		SS.UseSample2 = GUI.Toggle (new Rect (offsetX, offsetY, 150, 20), SS.UseSample2, "Use Color Sample 2");
		if (SS.UseSample2) {
			
			SS.IsolateSample2 = GUI.Toggle (new Rect (offsetX + 180, offsetY, 150, 20), SS.IsolateSample2, "Isolate Mask");
			if( SS.IsolateSample2 ){
				SS.IsolateSample1 = false;
				SS.IsolateSample3 = false;
			}
			offsetY += 30;
			
			if ( GUI.Button (new Rect (offsetX, offsetY + 5, 80, 20), "Pick Color") ) {
				selectingColor = true;
				currentSelection = 2;
			}
			
			GUI.DrawTexture (new Rect (offsetX + 10, offsetY + 35, 60, 60), _SampleColorMap2);
			
			GUI.Label (new Rect (offsetX + 90, offsetY, 250, 30), "Hue");
			SS.HueWeight2 = GUI.VerticalSlider (new Rect (offsetX + 95, offsetY + 30, 10, 70), SS.HueWeight2, 1.0f, 0.0f);
			
			GUI.Label (new Rect (offsetX + 120, offsetY, 250, 30), "Sat");
			SS.SatWeight2 = GUI.VerticalSlider (new Rect (offsetX + 125, offsetY + 30, 10, 70), SS.SatWeight2, 1.0f, 0.0f);
			
			GUI.Label (new Rect (offsetX + 150, offsetY, 250, 30), "Lum");
			SS.LumWeight2 = GUI.VerticalSlider (new Rect (offsetX + 155, offsetY + 30, 10, 70), SS.LumWeight2, 1.0f, 0.0f);
			
			GUI.Label (new Rect (offsetX + 180, offsetY, 250, 30), "Low");
			SS.MaskLow2 = GUI.VerticalSlider (new Rect (offsetX + 185, offsetY + 30, 10, 70), SS.MaskLow2, 1.0f, 0.0f);
			
			GUI.Label (new Rect (offsetX + 210, offsetY, 250, 30), "High");
			SS.MaskHigh2 = GUI.VerticalSlider (new Rect (offsetX + 215, offsetY + 30, 10, 70), SS.MaskHigh2, 1.0f, 0.0f);
			
			GUI.Label (new Rect (offsetX + 240, offsetY, 250, 30), "Smooth");
			SS.Sample2Smoothness = GUI.VerticalSlider (new Rect (offsetX + 255, offsetY + 30, 10, 70), SS.Sample2Smoothness, 1.0f, 0.0f);
			
			offsetY += 110;
		} else {
			offsetY += 30;
			SS.IsolateSample2 = false;
		}

		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Base Smoothness", SS.BaseSmoothness, SS.BaseSmoothnessText, out SS.BaseSmoothness, out SS.BaseSmoothnessText, 0.0f, 1.0f );
		offsetY += 40;

		if (GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Sample Blur Size", SS.BlurSize, SS.BlurSizeText, out SS.BlurSize, out SS.BlurSizeText, 0, 100)) {
			doStuff = true;
		}
		offsetY += 40;

		if (GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "High Pass Blur Size", SS.OverlayBlurSize, SS.OverlayBlurSizeText, out SS.OverlayBlurSize, out SS.OverlayBlurSizeText, 10, 100)) {
			doStuff = true;
		}
		offsetY += 40;
		
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "High Pass Overlay", SS.BlurOverlay, SS.BlurOverlayText, out SS.BlurOverlay, out SS.BlurOverlayText, -10.0f, 10.0f );
		offsetY += 40;
		
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Final Contrast", SS.FinalContrast, SS.FinalContrastText, out SS.FinalContrast, out SS.FinalContrastText, -2.0f, 2.0f );
		offsetY += 40;
		
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Final Bias", SS.FinalBias, SS.FinalBiasText, out SS.FinalBias, out SS.FinalBiasText, -0.5f, 0.5f );
		offsetY += 50;
		
		if( GUI.Button (new Rect (offsetX, offsetY, 130, 30), "Set as Smoothness" ) ){
			StartCoroutine( ProcessSmoothness() );
		}

		GUI.DragWindow();
		
	}

	void OnGUI () {
		
		windowRect.width = 300;
		windowRect.height = 490;

		if (SS.UseSample1) {
			windowRect.height += 110;
		}

		if (SS.UseSample2) {
			windowRect.height += 110;
		}
		
		windowRect = GUI.Window(17, windowRect, DoMyWindow, "Smoothness From Diffuse");
		
	}

	public void Close(){
		CleanupTextures ();
		this.gameObject.SetActive (false);
	}

	void CleanupTexture( RenderTexture _Texture ) {
		
		if (_Texture != null) {
			_Texture.Release();
			_Texture = null;
		}
		
	}
	
	void CleanupTextures() {
		
		Debug.Log ("Cleaning Up Textures");
		
		CleanupTexture( _BlurMap );
		CleanupTexture( _OverlayBlurMap );
		CleanupTexture( _TempMap );
		
	}

	public void InitializeTextures() {
		
		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		
		CleanupTextures ();

		_DiffuseMap = MainGuiScript._DiffuseMap;
		_DiffuseMapOriginal = MainGuiScript._DiffuseMapOriginal;

		_MetallicMap = MainGuiScript._MetallicMap;
		if (_MetallicMap != null) {
			thisMaterial.SetTexture ("_MetallicTex", _MetallicMap);
		} else {
			thisMaterial.SetTexture ("_MetallicTex", _DefaultMetallicMap);
		}

		if (_DiffuseMap) {
			thisMaterial.SetTexture ("_MainTex", _DiffuseMap);
			imageSizeX = _DiffuseMap.width;
			imageSizeY = _DiffuseMap.height;
		} else {
			thisMaterial.SetTexture ("_MainTex", _DiffuseMapOriginal);
			imageSizeX = _DiffuseMapOriginal.width;
			imageSizeY = _DiffuseMapOriginal.height;

			SS.useAdjustedDiffuse = false;
			SS.useOriginalDiffuse = true;
		}
		
		Debug.Log ( "Initializing Textures of size: " + imageSizeX.ToString() + "x" + imageSizeY.ToString() );
		
		_TempMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear );
		_TempMap.wrapMode = TextureWrapMode.Repeat;
		_BlurMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear );
		_BlurMap.wrapMode = TextureWrapMode.Repeat;
		_OverlayBlurMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear );
		_OverlayBlurMap.wrapMode = TextureWrapMode.Repeat;
		
	}

	public IEnumerator ProcessSmoothness() {

		busy = true;
		
		Debug.Log ("Processing Height");
		
		blitSmoothnessMaterial.SetVector ("_ImageSize", new Vector4 (imageSizeX, imageSizeY, 0, 0));

		if (_MetallicMap != null) {
			blitSmoothnessMaterial.SetTexture ("_MetallicTex", _MetallicMap);
		} else {
			blitSmoothnessMaterial.SetTexture ("_MetallicTex", _DefaultMetallicMap);
		}


		blitSmoothnessMaterial.SetTexture ("_BlurTex", _BlurMap);

		blitSmoothnessMaterial.SetTexture ("_OverlayBlurTex", _OverlayBlurMap);

		blitSmoothnessMaterial.SetFloat ( "_MetalSmoothness", SS.MetalSmoothness);

		if( SS.UseSample1){
			blitSmoothnessMaterial.SetInt ("_UseSample1", 1);
		} else {
			blitSmoothnessMaterial.SetInt ("_UseSample1", 0);
		}
		blitSmoothnessMaterial.SetColor ( "_SampleColor1", SS.SampleColor1);
		blitSmoothnessMaterial.SetVector ( "_SampleUV1", new Vector4 (SS.SampleUV1.x, SS.SampleUV1.y, 0, 0));
		blitSmoothnessMaterial.SetFloat ( "_HueWeight1", SS.HueWeight1 );
		blitSmoothnessMaterial.SetFloat ( "_SatWeight1", SS.SatWeight1 );
		blitSmoothnessMaterial.SetFloat ( "_LumWeight1", SS.LumWeight1 );
		blitSmoothnessMaterial.SetFloat ( "_MaskLow1", SS.MaskLow1 );
		blitSmoothnessMaterial.SetFloat ( "_MaskHigh1", SS.MaskHigh1 );
		blitSmoothnessMaterial.SetFloat ( "_Sample1Smoothness", SS.Sample1Smoothness);
		
		if( SS.UseSample2){
			blitSmoothnessMaterial.SetInt ("_UseSample2", 1);
		} else {
			blitSmoothnessMaterial.SetInt ("_UseSample2", 0);
		}
		blitSmoothnessMaterial.SetColor ( "_SampleColor2", SS.SampleColor2);
		blitSmoothnessMaterial.SetVector ( "_SampleUV2", new Vector4 (SS.SampleUV2.x, SS.SampleUV2.y, 0, 0));
		blitSmoothnessMaterial.SetFloat ( "_HueWeight2", SS.HueWeight2 );
		blitSmoothnessMaterial.SetFloat ( "_SatWeight2", SS.SatWeight2 );
		blitSmoothnessMaterial.SetFloat ( "_LumWeight2", SS.LumWeight2 );
		blitSmoothnessMaterial.SetFloat ( "_MaskLow2", SS.MaskLow2 );
		blitSmoothnessMaterial.SetFloat ( "_MaskHigh2", SS.MaskHigh2 );
		blitSmoothnessMaterial.SetFloat ( "_Sample2Smoothness", SS.Sample2Smoothness);

		if( SS.UseSample3){
			blitSmoothnessMaterial.SetInt ("_UseSample3", 1);
		} else {
			blitSmoothnessMaterial.SetInt ("_UseSample3", 0);
		}
		blitSmoothnessMaterial.SetColor ( "_SampleColor3", SS.SampleColor3);
		blitSmoothnessMaterial.SetVector ( "_SampleUV3", new Vector4 (SS.SampleUV3.x, SS.SampleUV3.y, 0, 0));
		blitSmoothnessMaterial.SetFloat ( "_HueWeight3", SS.HueWeight3 );
		blitSmoothnessMaterial.SetFloat ( "_SatWeight3", SS.SatWeight3 );
		blitSmoothnessMaterial.SetFloat ( "_LumWeight3", SS.LumWeight3 );
		blitSmoothnessMaterial.SetFloat ( "_MaskLow3", SS.MaskLow3 );
		blitSmoothnessMaterial.SetFloat ( "_MaskHigh3", SS.MaskHigh3 );
		blitSmoothnessMaterial.SetFloat ( "_Sample3Smoothness", SS.Sample3Smoothness);
		
		blitSmoothnessMaterial.SetFloat ( "_BaseSmoothness", SS.BaseSmoothness);

		blitSmoothnessMaterial.SetFloat ( "_BlurOverlay", SS.BlurOverlay);	
		blitSmoothnessMaterial.SetFloat ( "_FinalContrast", SS.FinalContrast);
		blitSmoothnessMaterial.SetFloat ( "_FinalBias", SS.FinalBias); 
		
		CleanupTexture (_TempMap);
		_TempMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		_TempMap.wrapMode = TextureWrapMode.Repeat;

		if ( SS.useAdjustedDiffuse ) {
			Graphics.Blit( _DiffuseMap, _TempMap, blitSmoothnessMaterial, 0 );
		} else {
			Graphics.Blit( _DiffuseMapOriginal, _TempMap, blitSmoothnessMaterial, 0 );
		}
		
		RenderTexture.active = _TempMap;

		if (MainGuiScript._SmoothnessMap != null) {
			Destroy (MainGuiScript._SmoothnessMap);
		}

		MainGuiScript._SmoothnessMap = new Texture2D (_TempMap.width, _TempMap.height, TextureFormat.ARGB32, true, true);
		MainGuiScript._SmoothnessMap.ReadPixels (new Rect (0, 0, _TempMap.width, _TempMap.height), 0, 0);
		MainGuiScript._SmoothnessMap.Apply ();

		yield return new WaitForSeconds(0.01f);
		
		CleanupTexture ( _TempMap );

		busy = false;
		
	}

	public IEnumerator ProcessBlur ( ) {

		busy = true;
		
		Debug.Log ("Processing Blur");
		
		blitMaterial.SetVector ("_ImageSize", new Vector4( imageSizeX, imageSizeY, 0, 0 ) );
		blitMaterial.SetFloat ("_BlurContrast", 1.0f);
		blitMaterial.SetFloat ("_BlurSpread", 1.0f);
		
		// Blur the image for selection
		blitMaterial.SetInt ("_BlurSamples", SS.BlurSize);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		if ( SS.useAdjustedDiffuse ) {
			if (SS.BlurSize == 0) {
				Graphics.Blit (_DiffuseMap, _TempMap);
			} else {
				Graphics.Blit (_DiffuseMap, _TempMap, blitMaterial, 1);
			}
		} else {
			if (SS.BlurSize == 0) {
				Graphics.Blit (_DiffuseMapOriginal, _TempMap);
			} else {
				Graphics.Blit (_DiffuseMapOriginal, _TempMap, blitMaterial, 1);
			}
		}
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		if (SS.BlurSize == 0) {
			Graphics.Blit (_TempMap, _BlurMap);
		} else {
			Graphics.Blit (_TempMap, _BlurMap, blitMaterial, 1);
		}
		thisMaterial.SetTexture ("_BlurTex", _BlurMap);

		// Blur the image for overlay
		blitMaterial.SetInt ("_BlurSamples", SS.OverlayBlurSize);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		if ( SS.useAdjustedDiffuse ) {
			Graphics.Blit( _DiffuseMap, _TempMap, blitMaterial, 1 );
		} else {
			Graphics.Blit( _DiffuseMapOriginal, _TempMap, blitMaterial, 1 );
		}
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempMap, _OverlayBlurMap, blitMaterial, 1);
		thisMaterial.SetTexture ("_OverlayBlurTex", _OverlayBlurMap);
		
		yield return new WaitForSeconds(0.01f);

		busy = false;
		
	}
}
