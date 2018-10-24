using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System;


public class EditDiffuseSettings {

	[DefaultValueAttribute(50)]
	public int AvgColorBlurSize;
	[DefaultValueAttribute("50")]
	public string AvgColorBlurSizeText;

	[DefaultValueAttribute(20)]
	public int BlurSize;
	[DefaultValueAttribute("20")]
	public string BlurSizeText;

	[DefaultValueAttribute(0.0f)]
	public float BlurContrast;
	[DefaultValueAttribute("0")]
	public string BlurContrastText;

	[DefaultValueAttribute(0.5f)]
	public float LightMaskPow;
	[DefaultValueAttribute("0.5")]
	public string LightMaskPowText;
	[DefaultValueAttribute(0f)]
	public float LightPow;
	[DefaultValueAttribute("0")]
	public string LightPowText;

	[DefaultValueAttribute(0.5f)]
	public float DarkMaskPow;
	[DefaultValueAttribute("0.5")]
	public string DarkMaskPowText;
	[DefaultValueAttribute(0.0f)]
	public float DarkPow;
	[DefaultValueAttribute("0")]
	public string DarkPowText;

	[DefaultValueAttribute(0.0f)]
	public float HotSpot;
	[DefaultValueAttribute("0")]
	public string HotSpotText;
	[DefaultValueAttribute(0.0f)]
	public float DarkSpot;
	[DefaultValueAttribute("0")]
	public string DarkSpotText;

	[DefaultValueAttribute(1.0f)]
	public float FinalContrast;
	[DefaultValueAttribute("1")]
	public string FinalContrastText;

	[DefaultValueAttribute(0.0f)]
	public float FinalBias;
	[DefaultValueAttribute("0")]
	public string FinalBiasText;

	[DefaultValueAttribute(0.5f)]
	public float ColorLerp;
	[DefaultValueAttribute("0.5")]
	public string ColorLerpText;

	[DefaultValueAttribute(1.0f)]
	public float Saturation;
	[DefaultValueAttribute("1")]
	public string SaturationText;


	public EditDiffuseSettings(){
		this.AvgColorBlurSize = 50;
		this.AvgColorBlurSizeText = "50";

		this.BlurSize = 20;
		this.BlurSizeText = "20";

		this.BlurContrast = 0.0f;
		this.BlurContrastText = "0";

		this.LightMaskPow = 0.5f;
		this.LightMaskPowText = "0";

		this.LightPow = 0.0f;
		this.LightPowText = "0";

		this.DarkMaskPow = 0.5f;
		this.DarkMaskPowText = "0.5";

		this.DarkPow = 0.0f;
		this.DarkPowText = "0";

		this.HotSpot = 0.0f;
		this.HotSpotText = "0";

		this.DarkSpot = 0.0f;
		this.DarkSpotText = "0";

		this.FinalContrast = 1.0f;
		this.FinalContrastText = "1";

		this.FinalBias = 0.0f;
		this.FinalBiasText = "0";

		this.ColorLerp = 0.5f;
		this.ColorLerpText = "0.5";

		this.Saturation = 1.0f;
		this.SaturationText = "1";
	}

}

public class EditDiffuseGui : MonoBehaviour {
	
	public MainGui MainGuiScript;

	Texture2D _DiffuseMap;
	Texture2D _DiffuseMapOriginal;

	RenderTexture _TempMap;
	RenderTexture _BlurMap;
	RenderTexture _AvgTempMap;
	RenderTexture _AvgMap;

	public Material thisMaterial;
	Material blitMaterial;

	int imageSizeX;
	int imageSizeY;

	EditDiffuseSettings  EDS;

	public GameObject testObject;
	bool doStuff = false;
	bool newTexture = false;

	Rect windowRect = new Rect (30, 300, 300, 450);
	bool settingsInitialized = false;

	float Slider = 0.5f;

	public void GetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		projectObject.EDS = EDS;
	}
	
	public void SetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		if (projectObject.EDS != null) {
			EDS = projectObject.EDS;
		} else {
			settingsInitialized = false;
			InitializeSettings ();
		}
		doStuff = true;
	}

	void InitializeSettings() {
		
		if (settingsInitialized == false) {
			Debug.Log ("Initializing Edit Diffuse Settings");
			EDS = new EditDiffuseSettings ();
			settingsInitialized = true;
		}
		
	}


	// Use this for initialization
	void Start () {

		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		
		blitMaterial = new Material (Shader.Find ("Hidden/Blit_Shader"));

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
		
		if (newTexture) {
			InitializeTextures();
			newTexture = false;
		}
		
		if (doStuff) {
			StartCoroutine( ProcessBlur () );
			doStuff = false;
		}


		thisMaterial.SetFloat ( "_Slider", Slider);

		thisMaterial.SetFloat ("_BlurContrast", EDS.BlurContrast);

		thisMaterial.SetFloat ("_LightMaskPow", EDS.LightMaskPow );
		thisMaterial.SetFloat ("_LightPow", EDS.LightPow );

		thisMaterial.SetFloat ("_DarkMaskPow", EDS.DarkMaskPow );
		thisMaterial.SetFloat ("_DarkPow", EDS.DarkPow );

		thisMaterial.SetFloat ("_HotSpot", EDS.HotSpot );
		thisMaterial.SetFloat ("_DarkSpot", EDS.DarkSpot );

		thisMaterial.SetFloat ("_FinalContrast", EDS.FinalContrast);
		thisMaterial.SetFloat ("_FinalBias", EDS.FinalBias); 

		thisMaterial.SetFloat ("_ColorLerp", EDS.ColorLerp);

		thisMaterial.SetFloat ("_Saturation", EDS.Saturation);
		
	}

	string FloatToString ( float num, int length ) {

		string numString = num.ToString ();
		int numStringLength = numString.Length;
		int lastIndex = Mathf.FloorToInt( Mathf.Min ( (float)numStringLength , (float)length ) );

		return numString.Substring (0, lastIndex);
	}

	void DoMyWindow ( int windowID ) {

		int spacingX = 0;
		int spacingY = 50;
		int spacing2Y = 70;
		
		int offsetX = 10;
		int offsetY = 30;
		
		//GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Contrast", EDS.DiffuseContrast, EDS.DiffuseContrastText, out EDS.DiffuseContrast, out EDS.DiffuseContrastText, -1.0f, 1.0f );
		//offsetY += 30;
		//GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Bias", EDS.DiffuseBias, EDS.DiffuseBiasText, out EDS.DiffuseBias, out EDS.DiffuseBiasText, -0.5f, 0.5f);		
		//offsetY += 50;

		GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Diffuse Reveal Slider" );
		Slider = GUI.HorizontalSlider( new Rect( offsetX, offsetY + 20, 280, 10 ),Slider,0.0f, 1.0f );		
		offsetY += 50;

		if( GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Average Color Blur Size", EDS.AvgColorBlurSize, EDS.AvgColorBlurSizeText, out EDS.AvgColorBlurSize, out EDS.AvgColorBlurSizeText, 5, 100) ) {
			doStuff = true;
		}
		offsetY += 50;
		
		if (GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Overlay Blur Size", EDS.BlurSize, EDS.BlurSizeText, out EDS.BlurSize, out EDS.BlurSizeText, 5, 100)) {
			doStuff = true;
		}
		offsetY += 30;
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Overlay Blur Contrast", EDS.BlurContrast, EDS.BlurContrastText, out EDS.BlurContrast, out EDS.BlurContrastText, -1.0f, 1.0f );		
		offsetY += 50;

		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Light Mask Power", EDS.LightMaskPow, EDS.LightMaskPowText, out EDS.LightMaskPow, out EDS.LightMaskPowText, 0.0f, 1.0f );		
		offsetY += 30;
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Remove Light", EDS.LightPow, EDS.LightPowText, out EDS.LightPow, out EDS.LightPowText, 0.0f, 1.0f );		
		offsetY += 50;
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Shadow Mask Power", EDS.DarkMaskPow, EDS.DarkMaskPowText, out EDS.DarkMaskPow, out EDS.DarkMaskPowText, 0.0f, 1.0f );
		offsetY += 30;
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Remove Shadow", EDS.DarkPow, EDS.DarkPowText, out EDS.DarkPow, out EDS.DarkPowText, 0.0f, 1.0f );	
		offsetY += 50;

		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Hot Spot Removal", EDS.HotSpot, EDS.HotSpotText, out EDS.HotSpot, out EDS.HotSpotText, 0.0f, 1.0f );
		offsetY += 30;
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Dark Spot Removal", EDS.DarkSpot, EDS.DarkSpotText, out EDS.DarkSpot, out EDS.DarkSpotText, 0.0f, 1.0f );	
		offsetY += 50;

		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Final Contrast", EDS.FinalContrast, EDS.FinalContrastText, out EDS.FinalContrast, out EDS.FinalContrastText, -2.0f, 2.0f );		
		offsetY += 30;
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Final Bias", EDS.FinalBias, EDS.FinalBiasText, out EDS.FinalBias, out EDS.FinalBiasText, -0.5f, 0.5f );		
		offsetY += 50;

		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Keep Original Color", EDS.ColorLerp, EDS.ColorLerpText, out EDS.ColorLerp, out EDS.ColorLerpText, 0.0f, 1.0f );		
		offsetY += 30;
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Saturation", EDS.Saturation, EDS.SaturationText, out EDS.Saturation, out EDS.SaturationText, 0.0f, 1.0f );		
		offsetY += 50;
		
		if( GUI.Button (new Rect (offsetX + 150, offsetY, 130, 30), "Set as Diffuse" ) ){
			StartCoroutine( ProcessDiffuse ( MapType.diffuse ) );
		}

		GUI.DragWindow();
	}

	void OnGUI () {
		
		windowRect.width = 300;
		windowRect.height = 650;

		windowRect = GUI.Window(12, windowRect, DoMyWindow, "Edit Diffuse");

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
		CleanupTexture( _TempMap );
		CleanupTexture (_AvgMap);
		CleanupTexture (_AvgTempMap);
	}

	void InitializeTextures() {
		
		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		
		CleanupTextures ();
		
		_DiffuseMapOriginal = MainGuiScript._DiffuseMapOriginal;

		thisMaterial.SetTexture ("_MainTex", _DiffuseMapOriginal);
		
		imageSizeX = _DiffuseMapOriginal.width;
		imageSizeY = _DiffuseMapOriginal.height;
		
		Debug.Log ( "Initializing Textures of size: " + imageSizeX.ToString() + "x" + imageSizeY.ToString() );

		_BlurMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_BlurMap.wrapMode = TextureWrapMode.Repeat;
		_AvgMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_AvgMap.wrapMode = TextureWrapMode.Repeat;
		
	}

	IEnumerator ProcessDiffuse( MapType whichTexture ) {
		
		Debug.Log ("Processing Diffuse");
		
		blitMaterial.SetVector ("_ImageSize", new Vector4 (imageSizeX, imageSizeY, 0, 0));
		
		blitMaterial.SetTexture ("_MainTex", _DiffuseMapOriginal);

		blitMaterial.SetTexture ("_BlurTex", _BlurMap);
		blitMaterial.SetFloat ("_BlurContrast", EDS.BlurContrast);

		blitMaterial.SetTexture ("_AvgTex", _AvgMap);
		
		blitMaterial.SetFloat ("_LightMaskPow", EDS.LightMaskPow );
		blitMaterial.SetFloat ("_LightPow", EDS.LightPow );
		
		blitMaterial.SetFloat ("_DarkMaskPow", EDS.DarkMaskPow );
		blitMaterial.SetFloat ("_DarkPow", EDS.DarkPow );

		blitMaterial.SetFloat ("_HotSpot", EDS.HotSpot );
		blitMaterial.SetFloat ("_DarkSpot", EDS.DarkSpot );
		
		blitMaterial.SetFloat ("_FinalContrast", EDS.FinalContrast);
		
		blitMaterial.SetFloat ("_FinalBias", EDS.FinalBias); 
		
		blitMaterial.SetFloat ("_ColorLerp", EDS.ColorLerp);
		
		blitMaterial.SetFloat ("_Saturation", EDS.Saturation);

		CleanupTexture ( _TempMap );
		_TempMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		_TempMap.wrapMode = TextureWrapMode.Repeat;
		
		Graphics.Blit(_DiffuseMapOriginal, _TempMap, blitMaterial, 11);
		
		RenderTexture.active = _TempMap;

		if (MainGuiScript._DiffuseMap != null) {
			Destroy (MainGuiScript._DiffuseMap);
			MainGuiScript._DiffuseMap = null;
		}

		MainGuiScript._DiffuseMap = new Texture2D (_TempMap.width, _TempMap.height, TextureFormat.ARGB32, true, true );
		MainGuiScript._DiffuseMap.ReadPixels (new Rect (0, 0, _TempMap.width, _TempMap.height), 0, 0);
		MainGuiScript._DiffuseMap.Apply ();
		
		yield return new WaitForSeconds(0.1f);
		
		CleanupTexture ( _TempMap );
		
	}

	IEnumerator ProcessBlur () {
		
		Debug.Log ("Processing Blur");

		CleanupTexture ( _TempMap );
		_TempMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_TempMap.wrapMode = TextureWrapMode.Repeat;
		
		blitMaterial.SetVector ("_ImageSize", new Vector4( imageSizeX, imageSizeY, 0, 0 ) );
		blitMaterial.SetFloat ("_BlurContrast", 1.0f);
		blitMaterial.SetFloat ("_BlurSpread", 1.0f);
		
		// Blur the image 1
		blitMaterial.SetInt ("_BlurSamples", EDS.BlurSize);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_DiffuseMapOriginal, _TempMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempMap, _BlurMap, blitMaterial, 1);
		thisMaterial.SetTexture ("_BlurTex", _BlurMap);


		blitMaterial.SetTexture ("_MainTex", _DiffuseMapOriginal);
		blitMaterial.SetInt ("_BlurSamples", EDS.AvgColorBlurSize);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_DiffuseMapOriginal, _TempMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempMap, _AvgMap, blitMaterial, 1);

		blitMaterial.SetFloat ("_BlurSpread", EDS.AvgColorBlurSize / 5);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_AvgMap, _TempMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempMap, _AvgMap, blitMaterial, 1);

		thisMaterial.SetTexture ("_AvgTex", _AvgMap);

		CleanupTexture ( _TempMap );
		CleanupTexture ( _AvgTempMap );

		yield return new WaitForSeconds(0.01f);
		
	}
}
