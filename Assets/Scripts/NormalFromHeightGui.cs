using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class NormalFromHeightSettings {

	[DefaultValueAttribute(20.0f)]
	public float Blur0Contrast;
	[DefaultValueAttribute("20")]
	public string Blur0ContrastText;

	[DefaultValueAttribute(0.3f)]
	public float Blur0Weight;
	[DefaultValueAttribute(0.35f)]
	public float Blur1Weight;
	[DefaultValueAttribute(0.5f)]
	public float Blur2Weight;
	[DefaultValueAttribute(0.8f)]
	public float Blur3Weight;
	[DefaultValueAttribute(1.0f)]
	public float Blur4Weight;
	[DefaultValueAttribute(0.95f)]
	public float Blur5Weight;
	[DefaultValueAttribute(0.8f)]
	public float Blur6Weight;

	[DefaultValueAttribute(5.0f)]
	public float FinalContrast;
	[DefaultValueAttribute("5")]
	public string FinalContrastText;

	[DefaultValueAttribute(0.0f)]
	public float Angularity;
	[DefaultValueAttribute("0")]
	public string AngularityText;

	[DefaultValueAttribute(0.5f)]
	public float AngularIntensity;
	[DefaultValueAttribute("0.5")]
	public string AngularIntensityText;

	[DefaultValueAttribute(true)]
	public bool UseDiffuse;

	[DefaultValueAttribute(0.0f)]
	public float ShapeRecognition;
	[DefaultValueAttribute("0")]
	public string ShapeRecognitionText;

	[DefaultValueAttribute(0.0f)]
	public float LightRotation;
	[DefaultValueAttribute("0")]
	public string LightRotationText;

	[DefaultValueAttribute(50.0f)]
	public int SlopeBlur;
	[DefaultValueAttribute("50")]
	public string SlopeBlurText;

	[DefaultValueAttribute(0.5f)]
	public float ShapeBias;
	[DefaultValueAttribute("0.5")]
	public string ShapeBiasText;

	public NormalFromHeightSettings() {

		this.Blur0Weight = 0.3f;
		this.Blur1Weight = 0.35f;
		this.Blur2Weight = 0.5f;
		this.Blur3Weight = 0.8f;
		this.Blur4Weight = 1.0f;
		this.Blur5Weight = 0.95f;
		this.Blur6Weight = 0.8f;

		this.Blur0Contrast = 20.0f;
		this.Blur0ContrastText = "20";

		this.FinalContrast = 5.0f;
		this.FinalContrastText = "5";

		this.Angularity = 0.0f;
		this.AngularityText = "0";

		this.AngularIntensity = 0.5f;
		this.AngularIntensityText = "0.5";

		this.UseDiffuse = true;

		this.ShapeRecognition = 0.0f;
		this.ShapeRecognitionText = "0";

		this.LightRotation = 0.0f;
		this.LightRotationText = "0";

		this.SlopeBlur = 50;
		this.SlopeBlurText = "50";

		this.ShapeBias = 0.5f;
		this.ShapeBiasText = "0.5";
	}

}

public class NormalFromHeightGui : MonoBehaviour {

	MainGui MGS;

	public Light mainLight;

	RenderTexture _TempBlurMap;
	public RenderTexture _HeightBlurMap;
	RenderTexture _BlurMap0;
	RenderTexture _BlurMap1;
	RenderTexture _BlurMap2;
	RenderTexture _BlurMap3;
	RenderTexture _BlurMap4;
	RenderTexture _BlurMap5;
	RenderTexture _BlurMap6;
	RenderTexture _TempNormalMap;

	float Slider = 0.5f;

	float _BlurScale = 1.0f;
	int imageSizeX = 1024;
	int imageSizeY = 1024;

	NormalFromHeightSettings NFHS;

	public Material thisMaterial;
	Material blitMaterial;

	public GameObject testObject;
	bool doStuff = false;
	bool newTexture = false;

	Rect windowRect = new Rect (30, 300, 300, 450);
	bool settingsInitialized = false;

	public bool busy = false;
	
	public void GetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		projectObject.NFHS = NFHS;
	}
	
	public void SetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		if (projectObject.NFHS != null) {
			NFHS = projectObject.NFHS;
		} else {
			settingsInitialized = false;
			InitializeSettings ();
		}
		doStuff = true;
	}
	
	void InitializeSettings() {
		
		if (settingsInitialized == false) {
			Debug.Log ("Initializing Normal From Height Settings");
			NFHS = new NormalFromHeightSettings ();
			settingsInitialized = true;
		}
		
	}

	// Use this for initialization
	void Start () {

		MGS = MainGui.instance;
		
		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		
		blitMaterial = new Material (Shader.Find ("Hidden/Blit_Shader"));

		InitializeSettings ();
		SetMaterialValues();
	
	}

	public void DoStuff() {
		doStuff = true;
	}
	
	public void NewTexture() {
		newTexture = true;
	}

	void SetMaterialValues() {
		
		//thisMaterial.SetFloat ("_BlurScale", _BlurScale);
		//thisMaterial.SetVector ("_ImageSize", new Vector4( imageSizeX, imageSizeY, 0, 0 ));
		
	}

	void SetWeightEQDefault() {
		NFHS.Blur0Weight = 0.3f;
		NFHS.Blur1Weight = 0.35f;
		NFHS.Blur2Weight = 0.5f;
		NFHS.Blur3Weight = 0.8f;
		NFHS.Blur4Weight = 1.0f;
		NFHS.Blur5Weight = 0.95f;
		NFHS.Blur6Weight = 0.8f;
		doStuff = true;
	}

	void SetWeightEQSmooth() {
		NFHS.Blur0Weight = 0.1f;
		NFHS.Blur1Weight = 0.15f;
		NFHS.Blur2Weight = 0.25f;
		NFHS.Blur3Weight = 0.6f;
		NFHS.Blur4Weight = 0.9f;
		NFHS.Blur5Weight = 1.0f;
		NFHS.Blur6Weight = 1.0f;
		doStuff = true;
	}

	void SetWeightEQCrisp() {
		NFHS.Blur0Weight = 1.0f;
		NFHS.Blur1Weight = 0.9f;
		NFHS.Blur2Weight = 0.6f;
		NFHS.Blur3Weight = 0.4f;
		NFHS.Blur4Weight = 0.25f;
		NFHS.Blur5Weight = 0.15f;
		NFHS.Blur6Weight = 0.1f;
		doStuff = true;
	}

	void SetWeightEQMids() {
		NFHS.Blur0Weight = 0.15f;
		NFHS.Blur1Weight = 0.5f;
		NFHS.Blur2Weight = 0.85f;
		NFHS.Blur3Weight = 1.0f;
		NFHS.Blur4Weight = 0.85f;
		NFHS.Blur5Weight = 0.5f;
		NFHS.Blur6Weight = 0.15f;
		doStuff = true;
	}
	
	// Update is called once per frame
	void Update () {

		if (newTexture) {
			InitializeTextures();
			newTexture = false;
		}
		
		if (doStuff) {
			StartCoroutine( ProcessHeight () );
			doStuff = false;
		}

		thisMaterial.SetFloat ("_Blur0Weight", NFHS.Blur0Weight);
		thisMaterial.SetFloat ("_Blur1Weight", NFHS.Blur1Weight);
		thisMaterial.SetFloat ("_Blur2Weight", NFHS.Blur2Weight);
		thisMaterial.SetFloat ("_Blur3Weight", NFHS.Blur3Weight);
		thisMaterial.SetFloat ("_Blur4Weight", NFHS.Blur4Weight);
		thisMaterial.SetFloat ("_Blur5Weight", NFHS.Blur5Weight);
		thisMaterial.SetFloat ("_Blur6Weight", NFHS.Blur6Weight);

		thisMaterial.SetFloat ( "_Slider", Slider);

		thisMaterial.SetFloat ("_Angularity", NFHS.Angularity);
		thisMaterial.SetFloat ("_AngularIntensity", NFHS.AngularIntensity);

		//thisMaterial.SetFloat ("_LightRotation", NFHS.LightRotation);
		//thisMaterial.SetFloat ("_ShapeRecognition", NFHS.ShapeRecognition);

		thisMaterial.SetFloat ("_FinalContrast", NFHS.FinalContrast);

		thisMaterial.SetVector ("_LightDir", mainLight.transform.forward);

	}

	void DoMyWindow ( int windowID ) {
		
		int offsetX = 10;
		int offsetY = 30;

		//GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Pre Contrast");
		//offsetY += 30;
		//Blur0Contrast = GUI.HorizontalSlider( new Rect( offsetX, offsetY, 280, 10 ),Blur0Contrast,5.0f, 50.0f );

		GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Normal Reveal Slider" );
		Slider = GUI.HorizontalSlider( new Rect( offsetX, offsetY + 20, 280, 10 ),Slider,0.0f, 1.0f );

		offsetY += 40;

		if( GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), "Pre Contrast", NFHS.Blur0Contrast, NFHS.Blur0ContrastText, out NFHS.Blur0Contrast, out NFHS.Blur0ContrastText, 0.0f, 50.0f ) ) {
			doStuff = true;
		}
		offsetY += 50;
		
		GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Frequency Equalizer" );
		GUI.Label (new Rect (offsetX + 225, offsetY, 100, 30), "Presets" );
		if ( GUI.Button (new Rect (offsetX + 215, offsetY + 30, 60, 20), "Default") ) {
			SetWeightEQDefault ();
		}
		if ( GUI.Button (new Rect (offsetX + 215, offsetY + 60, 60, 20), "Smooth") ) {
			SetWeightEQSmooth ();
		}
		if ( GUI.Button (new Rect (offsetX + 215, offsetY + 90, 60, 20), "Crisp") ) {
			SetWeightEQCrisp ();
		}
		if ( GUI.Button (new Rect (offsetX + 215, offsetY + 120, 60, 20), "Mids") ) {
			SetWeightEQMids ();
		}
		offsetY += 30;
		offsetX += 10;
		NFHS.Blur0Weight = GUI.VerticalSlider( new Rect( offsetX + 180, offsetY, 10, 100 ),NFHS.Blur0Weight,1.0f, 0.0f );
		NFHS.Blur1Weight = GUI.VerticalSlider( new Rect( offsetX + 150, offsetY, 10, 100 ),NFHS.Blur1Weight,1.0f, 0.0f );
		NFHS.Blur2Weight = GUI.VerticalSlider( new Rect( offsetX + 120, offsetY, 10, 100 ),NFHS.Blur2Weight,1.0f, 0.0f );
		NFHS.Blur3Weight = GUI.VerticalSlider( new Rect( offsetX + 90, offsetY, 10, 100 ),NFHS.Blur3Weight,1.0f, 0.0f );
		NFHS.Blur4Weight = GUI.VerticalSlider( new Rect( offsetX + 60, offsetY, 10, 100 ),NFHS.Blur4Weight,1.0f, 0.0f );
		NFHS.Blur5Weight = GUI.VerticalSlider( new Rect( offsetX + 30, offsetY, 10, 100 ),NFHS.Blur5Weight,1.0f, 0.0f );
		NFHS.Blur6Weight = GUI.VerticalSlider( new Rect( offsetX + 0, offsetY, 10, 100 ),NFHS.Blur6Weight,1.0f, 0.0f );
		offsetX -= 10;
		offsetY += 120;

		GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Angular Intensity");
		offsetY += 25;
		GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), NFHS.AngularIntensity, NFHS.AngularIntensityText, out NFHS.AngularIntensity, out NFHS.AngularIntensityText, 0.0f, 1.0f );
		offsetY += 25;
		GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Angularity Amount");
		offsetY += 25;
		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), NFHS.Angularity, NFHS.AngularityText, out NFHS.Angularity, out NFHS.AngularityText, 0.0f, 1.0f);

		offsetY += 30;

		if (MGS._DiffuseMapOriginal) {
			GUI.enabled = true;
		} else {
			GUI.enabled = false;
			NFHS.UseDiffuse = false;
		}

		bool tempBool = NFHS.UseDiffuse;
		NFHS.UseDiffuse = GUI.Toggle (new Rect (offsetX, offsetY, 280, 30), NFHS.UseDiffuse, " Shape from Diffuse (Uncheked from Height)");
		if( tempBool != NFHS.UseDiffuse ){
			doStuff = true;
		}
		offsetY += 30;

		GUI.enabled = true;

		GUI.Label (new Rect (offsetX, offsetY, 280, 30), " Shape Recognition, Rotation, Spread, Bias");
		offsetY += 30;
		if ( GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), NFHS.ShapeRecognition, NFHS.ShapeRecognitionText, out NFHS.ShapeRecognition, out NFHS.ShapeRecognitionText, 0.0f, 1.0f ) ) {
			doStuff = true;
		}
		offsetY += 25;
		if ( GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), NFHS.LightRotation, NFHS.LightRotationText, out NFHS.LightRotation, out NFHS.LightRotationText, -3.14f, 3.14f ) ) {
			doStuff = true;
		}
		offsetY += 25;
		if ( GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), NFHS.SlopeBlur, NFHS.SlopeBlurText, out NFHS.SlopeBlur, out NFHS.SlopeBlurText, 5, 100 ) ) {
			doStuff = true;
		}
		offsetY += 25;
		if ( GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), NFHS.ShapeBias, NFHS.ShapeBiasText, out NFHS.ShapeBias, out NFHS.ShapeBiasText, 0.0f, 1.0f ) ) {
			doStuff = true;
		}
		offsetY += 30;

		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Final Contrast", NFHS.FinalContrast, NFHS.FinalContrastText, out NFHS.FinalContrast, out NFHS.FinalContrastText, 0.0f, 10.0f);
		offsetY += 50;
		
		if( GUI.Button (new Rect (offsetX + 150, offsetY, 130, 30), "Set as Normal Map" ) ){
			StartCoroutine( ProcessNormal () );
		}

		GUI.DragWindow();

	}

	void OnGUI () {

		windowRect.width = 300;
		windowRect.height = 630;
		
		windowRect = GUI.Window(16, windowRect, DoMyWindow, "Normal From Height");

	}

	public void InitializeTextures() {
		
		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;

		CleanupTextures ();

		if (!MGS._HDHeightMap) {
			thisMaterial.SetTexture ("_HeightTex", MGS._HeightMap);
		} else {
			thisMaterial.SetTexture ("_HeightTex", MGS._HDHeightMap);
		}
		
		imageSizeX = MGS._HeightMap.width;
		imageSizeY = MGS._HeightMap.height;
		
		Debug.Log ( "Initializing Textures of size: " + imageSizeX.ToString() + "x" + imageSizeY.ToString() );
		
		_TempBlurMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_TempBlurMap.wrapMode = TextureWrapMode.Repeat;
		_HeightBlurMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
		_HeightBlurMap.wrapMode = TextureWrapMode.Repeat;
		_BlurMap0 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_BlurMap0.wrapMode = TextureWrapMode.Repeat;
		_BlurMap1 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_BlurMap1.wrapMode = TextureWrapMode.Repeat;
		_BlurMap2 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_BlurMap2.wrapMode = TextureWrapMode.Repeat;
		_BlurMap3 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_BlurMap3.wrapMode = TextureWrapMode.Repeat;
		_BlurMap4 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_BlurMap4.wrapMode = TextureWrapMode.Repeat;
		_BlurMap5 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_BlurMap5.wrapMode = TextureWrapMode.Repeat;
		_BlurMap6 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_BlurMap6.wrapMode = TextureWrapMode.Repeat;

		SetMaterialValues ();
		
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
		
		CleanupTexture (_TempBlurMap);
		CleanupTexture (_HeightBlurMap);
		CleanupTexture( _BlurMap0 );
		CleanupTexture( _BlurMap1 );
		CleanupTexture( _BlurMap2 );
		CleanupTexture( _BlurMap3 );
		CleanupTexture( _BlurMap4 );
		CleanupTexture( _BlurMap5 );
		CleanupTexture( _BlurMap6 );
		CleanupTexture (_TempNormalMap);
		
	}

	public IEnumerator ProcessNormal() {

		busy = true;
		
		Debug.Log ("Processing Normal");
		
		blitMaterial.SetVector ("_ImageSize", new Vector4( imageSizeX, imageSizeY, 0, 0 ));

		blitMaterial.SetFloat ("_Blur0Weight", NFHS.Blur0Weight);
		blitMaterial.SetFloat ("_Blur1Weight", NFHS.Blur1Weight);
		blitMaterial.SetFloat ("_Blur2Weight", NFHS.Blur2Weight);
		blitMaterial.SetFloat ("_Blur3Weight", NFHS.Blur3Weight);
		blitMaterial.SetFloat ("_Blur4Weight", NFHS.Blur4Weight);
		blitMaterial.SetFloat ("_Blur5Weight", NFHS.Blur5Weight);
		blitMaterial.SetFloat ("_Blur6Weight", NFHS.Blur6Weight);
		blitMaterial.SetFloat ("_FinalContrast", NFHS.FinalContrast);

		blitMaterial.SetTexture ("_HeightBlurTex", _HeightBlurMap);
		
		blitMaterial.SetTexture ("_MainTex", _BlurMap0);
		blitMaterial.SetTexture ("_BlurTex0", _BlurMap0);
		blitMaterial.SetTexture ("_BlurTex1", _BlurMap1);
		blitMaterial.SetTexture ("_BlurTex2", _BlurMap2);
		blitMaterial.SetTexture ("_BlurTex3", _BlurMap3);
		blitMaterial.SetTexture ("_BlurTex4", _BlurMap4);
		blitMaterial.SetTexture ("_BlurTex5", _BlurMap5);
		blitMaterial.SetTexture ("_BlurTex6", _BlurMap6);

		blitMaterial.SetFloat ("_Angularity", NFHS.Angularity);
		blitMaterial.SetFloat ("_AngularIntensity", NFHS.AngularIntensity);

		
		CleanupTexture (_TempNormalMap);
		_TempNormalMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		_TempNormalMap.wrapMode = TextureWrapMode.Repeat;
		Graphics.Blit(_BlurMap0, _TempNormalMap, blitMaterial, 4);
		
		if (MGS._NormalMap != null) {
			Destroy (MGS._NormalMap);
		}

		RenderTexture.active = _TempNormalMap;
		MGS._NormalMap = new Texture2D( _TempNormalMap.width, _TempNormalMap.height, TextureFormat.ARGB32, true, true );
		MGS._NormalMap.ReadPixels(new Rect(0, 0, _TempNormalMap.width, _TempNormalMap.height), 0, 0);
		MGS._NormalMap.Apply();
		
		yield return new WaitForSeconds(0.1f);
		
		CleanupTexture ( _TempNormalMap );

		busy = false;
		
	}

	public IEnumerator ProcessHeight () {

		busy = true;
		
		Debug.Log ("Processing Height");
		
		blitMaterial.SetVector ("_ImageSize", new Vector4( imageSizeX, imageSizeY, 0, 0 ));

		// Blur the height map for normal slope
		blitMaterial.SetFloat ("_BlurSpread", 1.0f);
		blitMaterial.SetInt ("_BlurSamples", NFHS.SlopeBlur);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		blitMaterial.SetFloat ("_BlurContrast", 1.0f);

		if (MGS._DiffuseMapOriginal && NFHS.UseDiffuse) {
			blitMaterial.SetInt ("_Desaturate", 1);
			Graphics.Blit (MGS._DiffuseMapOriginal, _TempBlurMap, blitMaterial, 1);
			blitMaterial.SetTexture ("_LightTex", MGS._DiffuseMapOriginal);
		} else {
			if (MGS._HDHeightMap == null) {
				blitMaterial.SetInt ("_Desaturate", 0);
				Graphics.Blit (MGS._HeightMap, _TempBlurMap, blitMaterial, 1);
				blitMaterial.SetTexture ("_LightTex", MGS._HeightMap);
			} else {
				blitMaterial.SetInt ("_Desaturate", 0);
				Graphics.Blit (MGS._HDHeightMap, _TempBlurMap, blitMaterial, 1);
				blitMaterial.SetTexture ("_LightTex", MGS._HDHeightMap);
			}
		}

		blitMaterial.SetInt ("_Desaturate", 0);

		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempBlurMap, _HeightBlurMap, blitMaterial, 1);

		blitMaterial.SetFloat ("_BlurSpread", 3.0f);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit (_HeightBlurMap, _TempBlurMap, blitMaterial, 1);

		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempBlurMap, _HeightBlurMap, blitMaterial, 1);

		blitMaterial.SetTexture ("_LightBlurTex", _HeightBlurMap);

		// Make normal from height
		blitMaterial.SetFloat ("_LightRotation", NFHS.LightRotation);
		blitMaterial.SetFloat ("_ShapeRecognition", NFHS.ShapeRecognition);
		blitMaterial.SetFloat ("_ShapeBias", NFHS.ShapeBias);
		blitMaterial.SetTexture ("_DiffuseTex", MGS._DiffuseMapOriginal);

		blitMaterial.SetFloat ("_BlurContrast", NFHS.Blur0Contrast);

		if ( MGS._HDHeightMap == null) {
			Graphics.Blit (MGS._HeightMap, _BlurMap0, blitMaterial, 3);
		} else {
			Graphics.Blit (MGS._HDHeightMap, _BlurMap0, blitMaterial, 3);
		}

		float extraSpread = ( (float)(_BlurMap0.width + _BlurMap0.height) * 0.5f ) / 1024.0f;
		float spread = 1.0f;

		blitMaterial.SetFloat ("_BlurContrast", 1.0f);
		blitMaterial.SetInt ("_Desaturate", 0);

		// Blur the image 1
		blitMaterial.SetInt ("_BlurSamples", 4);
		blitMaterial.SetFloat ("_BlurSpread", spread);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_BlurMap0, _TempBlurMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempBlurMap, _BlurMap1, blitMaterial, 1);

		spread += extraSpread;
		
		// Blur the image 2
		blitMaterial.SetFloat ("_BlurSpread", spread);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_BlurMap1, _TempBlurMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempBlurMap, _BlurMap2, blitMaterial, 1);

		spread += 2 * extraSpread;
		
		// Blur the image 3
		blitMaterial.SetFloat ("_BlurSpread", spread);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_BlurMap2, _TempBlurMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempBlurMap, _BlurMap3, blitMaterial, 1);

		spread += 4 * extraSpread;
		
		// Blur the image 4
		blitMaterial.SetFloat ("_BlurSpread", spread);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_BlurMap3, _TempBlurMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempBlurMap, _BlurMap4, blitMaterial, 1);

		spread += 8 * extraSpread;
		
		// Blur the image 5
		blitMaterial.SetFloat ("_BlurSpread", spread);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_BlurMap4, _TempBlurMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempBlurMap, _BlurMap5, blitMaterial, 1);

		spread += 16 * extraSpread;
		
		// Blur the image 6
		blitMaterial.SetFloat ("_BlurSpread", spread);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_BlurMap5, _TempBlurMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_TempBlurMap, _BlurMap6, blitMaterial, 1);

		//if( _HDHeightMap != null) {
		//	thisMaterial.SetTexture ("_MainTex", _HDHeightMap);
		//} else {
		//	thisMaterial.SetTexture ("_MainTex", MGS._HeightMap);
		//}


		thisMaterial.SetTexture ("_BlurTex0", _BlurMap0);
		thisMaterial.SetTexture ("_BlurTex1", _BlurMap1);
		thisMaterial.SetTexture ("_BlurTex2", _BlurMap2);
		thisMaterial.SetTexture ("_BlurTex3", _BlurMap3);
		thisMaterial.SetTexture ("_BlurTex4", _BlurMap4);
		thisMaterial.SetTexture ("_BlurTex5", _BlurMap5);
		thisMaterial.SetTexture ("_BlurTex6", _BlurMap6);
		
		yield return new WaitForSeconds(0.01f);

		busy = false;
		
	}
}
