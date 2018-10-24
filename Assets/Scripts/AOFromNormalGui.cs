using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class AOSettings {

	[DefaultValueAttribute(5.0f)]
	public float Spread;
	[DefaultValueAttribute("50")]
	public string SpreadText;

	[DefaultValueAttribute(0.0f)]
	public float FinalBias;
	[DefaultValueAttribute("0")]
	public string FinalBiasText;

	[DefaultValueAttribute(1.0f)]
	public float FinalContrast;
	[DefaultValueAttribute("1")]
	public string FinalContrastText;

	[DefaultValueAttribute(100.0f)]
	public float Depth;
	[DefaultValueAttribute("100")]
	public string DepthText;

	[DefaultValueAttribute(1.0f)]
	public float Blend;
	[DefaultValueAttribute("1")]
	public string BlendText;

	[DefaultValueAttribute(1.0f)]
	public float BlendAmount;

	public AOSettings(){
		
		this.Spread = 50.0f;
		this.SpreadText = "50";

		this.Depth = 100.0f;
		this.DepthText = "100";

		this.FinalBias = 0.0f;
		this.FinalBiasText = "0";

		this.FinalContrast = 1.0f;
		this.FinalContrastText = "1";

		this.Blend = 1.0f;
		this.BlendText = "1";

		this.BlendAmount = 1.0f;
	}
}

public class AOFromNormalGui : MonoBehaviour {

	public MainGui MainGuiScript;

	public Texture2D defaultNormal;
	public Texture2D defaultHeight;
	
	Texture2D _AOMap;
	RenderTexture _WorkingAOMap;
	RenderTexture _BlendedAOMap;
	RenderTexture _TempAOMap;

	int imageSizeX = 1024;
	int imageSizeY = 1024;

	AOSettings AOS;

	public Material thisMaterial;
	Material blitMaterial;
	
	public GameObject testObject;
	bool doStuff = false;
	bool newTexture = false;

	Rect windowRect = new Rect (30, 300, 300, 230);

	bool settingsInitialized = false;

	public bool busy = false;
	
	public void GetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		projectObject.AOS = AOS;
	}
	
	public void SetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		if (projectObject.AOS != null) {
			AOS = projectObject.AOS;
		} else {
			settingsInitialized = false;
			InitializeSettings ();
		}
		doStuff = true;
	}
	
	void InitializeSettings() {
		
		if (settingsInitialized == false) {
			
			AOS = new AOSettings ();

			if (MainGuiScript._HeightMap != null) {
				AOS.Blend = 1.0f;
				AOS.BlendText = "1.0";
			} else {
				AOS.Blend = 0.0f;
				AOS.BlendText = "0.0";
			}
			
			settingsInitialized = true;
		}
		
	}

	// Use this for initialization
	void Start () {
		
		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		
		blitMaterial = new Material (Shader.Find ("Hidden/Blit_Shader"));

		//blitMaterial = new Material (Shader.Find ("Hidden/Blit_Height_From_Normal"));

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

			StopAllCoroutines();

			StartCoroutine( ProcessNormalDepth () );
			doStuff = false;
		}

		thisMaterial.SetFloat ("_FinalContrast", AOS.FinalContrast);
		thisMaterial.SetFloat ("_FinalBias", AOS.FinalBias);
		thisMaterial.SetFloat ("_AOBlend", AOS.Blend);
		
	}

	void DoMyWindow ( int windowID ) {

		int spacingX = 0;
		int spacingY = 50;
		
		int offsetX = 10;
		int offsetY = 30;
		
		if( GuiHelper.Slider( new Rect (offsetX, offsetY, 280, 50), "AO pixel Spread", AOS.Spread, AOS.SpreadText, out AOS.Spread, out AOS.SpreadText, 10.0f, 100.0f ) ) {
			doStuff = true;
		}
		offsetY += 40;

		if( GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), "Pixel Depth", AOS.Depth, AOS.DepthText, out AOS.Depth, out AOS.DepthText, 0.0f, 256.0f ) ) {
			doStuff = true;
		}
		offsetY += 40;

		GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Blend Normal AO and Depth AO", AOS.Blend, AOS.BlendText, out AOS.Blend, out AOS.BlendText, 0.0f, 1.0f);
		offsetY += 40;
		
		GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), "AO Power", AOS.FinalContrast, AOS.FinalContrastText, out AOS.FinalContrast, out AOS.FinalContrastText, 0.1f, 10.0f );
		offsetY += 40;

		GuiHelper.Slider( new Rect( offsetX, offsetY, 280, 50 ), "AO Bias", AOS.FinalBias, AOS.FinalBiasText, out AOS.FinalBias, out AOS.FinalBiasText, -1.0f, 1.0f );
		offsetY += 50;

		if (busy) { GUI.enabled = false; } else { GUI.enabled = true; }
		if( GUI.Button (new Rect (offsetX + 150, offsetY, 130, 30), "Set as AO Map" ) ){
			StartCoroutine( ProcessAO () );
		}
		GUI.enabled = true;
		GUI.DragWindow();
	}

	void OnGUI () {
		
		windowRect.width = 300;
		windowRect.height = 280;
		
		windowRect = GUI.Window(10, windowRect, DoMyWindow, "Normal + Depth to AO");

	}

	public void InitializeTextures() {
		
		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		
		CleanupTextures ();

		if (MainGuiScript._NormalMap != null) {
			imageSizeX = MainGuiScript._NormalMap.width;
			imageSizeY = MainGuiScript._NormalMap.height;
		} else {
			imageSizeX = MainGuiScript._HeightMap.width;
			imageSizeY = MainGuiScript._HeightMap.height;
		}
		
		Debug.Log ( "Initializing Textures of size: " + imageSizeX.ToString() + "x" + imageSizeY.ToString() );
		
		_WorkingAOMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
		_WorkingAOMap.wrapMode = TextureWrapMode.Repeat;
		_BlendedAOMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
		_BlendedAOMap.wrapMode = TextureWrapMode.Repeat;
		
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
		
		CleanupTexture( _WorkingAOMap );
		CleanupTexture( _BlendedAOMap );
		CleanupTexture( _TempAOMap );
		
	}

	public IEnumerator ProcessAO() {
		
		busy = true;

		Debug.Log ("Processing AO Map");

		CleanupTexture (_TempAOMap);
		_TempAOMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		_TempAOMap.wrapMode = TextureWrapMode.Repeat;
		
		blitMaterial.SetFloat ("_FinalBias", AOS.FinalBias);
		blitMaterial.SetFloat ("_FinalContrast", AOS.FinalContrast);
		blitMaterial.SetTexture ("_MainTex", _BlendedAOMap);
		blitMaterial.SetFloat ("_AOBlend", AOS.Blend);

		Graphics.Blit(_BlendedAOMap, _TempAOMap, blitMaterial, 8);
		
		if (MainGuiScript._AOMap != null) {
			Destroy (MainGuiScript._AOMap);
		}

		RenderTexture.active = _TempAOMap;
		MainGuiScript._AOMap = new Texture2D( _TempAOMap.width, _TempAOMap.height, TextureFormat.ARGB32, true, true );
		MainGuiScript._AOMap.ReadPixels(new Rect(0, 0, _TempAOMap.width, _TempAOMap.height), 0, 0);
		MainGuiScript._AOMap.Apply();
		
		yield return new WaitForSeconds(0.1f);
		
		CleanupTexture ( _TempAOMap );

		busy = false;
	}

	public IEnumerator ProcessNormalDepth () {

		busy = true;
		
		Debug.Log ("Processing Normal Depth to AO");

		blitMaterial.SetVector ("_ImageSize", new Vector4 (imageSizeX, imageSizeY, 0, 0));
		blitMaterial.SetFloat ("_Spread", AOS.Spread);

		if (MainGuiScript._NormalMap != null) {
			blitMaterial.SetTexture ("_MainTex", MainGuiScript._NormalMap);
		} else {
			blitMaterial.SetTexture ("_MainTex", defaultNormal);
		}

		if (MainGuiScript._HDHeightMap != null) {
			blitMaterial.SetTexture ("_HeightTex", MainGuiScript._HDHeightMap);
		} else if (MainGuiScript._HeightMap != null) {
			blitMaterial.SetTexture ("_HeightTex", MainGuiScript._HeightMap); 
		} else {
			blitMaterial.SetTexture ("_HeightTex", defaultHeight);
		}
			
		blitMaterial.SetTexture ("_BlendTex", _BlendedAOMap);
		blitMaterial.SetFloat ("_Depth", AOS.Depth);
		thisMaterial.SetTexture ("_MainTex", _BlendedAOMap);

		int yieldCountDown = 5;

		for( int i = 1; i < 100; i++ ) {

			blitMaterial.SetFloat ("_BlendAmount", 1.0f / (float)i );
			blitMaterial.SetFloat ("_Progress", (float)i / 100.0f );

			Graphics.Blit (MainGuiScript._NormalMap, _WorkingAOMap, blitMaterial, 7);
			Graphics.Blit (_WorkingAOMap, _BlendedAOMap);


			yieldCountDown -= 1;
			if( yieldCountDown <= 0 ){
				yieldCountDown = 5;
				yield return new WaitForSeconds(0.01f);
			}
		}

		yield return new WaitForSeconds(0.01f);

		busy = false;

	}
}
