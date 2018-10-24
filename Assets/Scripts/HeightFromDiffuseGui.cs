using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class HeightFromDiffuseSettings {

	[DefaultValueAttribute(true)]
	public bool useAdjustedDiffuse;
	[DefaultValueAttribute(false)]
	public bool useOriginalDiffuse; 
	[DefaultValueAttribute(false)]
	public bool useNormal;


	[DefaultValueAttribute(0.15f)]
	public float Blur0Weight;
	[DefaultValueAttribute(0.19f)]
	public float Blur1Weight;
	[DefaultValueAttribute(0.3f)]
	public float Blur2Weight;
	[DefaultValueAttribute(0.5f)]
	public float Blur3Weight;
	[DefaultValueAttribute(0.7f)]
	public float Blur4Weight;
	[DefaultValueAttribute(0.9f)]
	public float Blur5Weight;
	[DefaultValueAttribute(1.0f)]
	public float Blur6Weight;
	
	[DefaultValueAttribute(1.0f)]
	public float Blur0Contrast;
	[DefaultValueAttribute(1.0f)]
	public float Blur1Contrast;
	[DefaultValueAttribute(1.0f)]
	public float Blur2Contrast;
	[DefaultValueAttribute(1.0f)]
	public float Blur3Contrast;
	[DefaultValueAttribute(1.0f)]
	public float Blur4Contrast;
	[DefaultValueAttribute(1.0f)]
	public float Blur5Contrast;
	[DefaultValueAttribute(1.0f)]
	public float Blur6Contrast;

	[DefaultValueAttribute(1.5f)]
	public float FinalContrast;
	[DefaultValueAttribute("1.5")]
	public string FinalContrastText;

	[DefaultValueAttribute(0.0f)]
	public float FinalBias;
	[DefaultValueAttribute("0")]
	public string FinalBiasText;

	[DefaultValueAttribute(0.0f)]
	public float FinalGain;
	[DefaultValueAttribute("0")]
	public string FinalGainText;

	//[DefaultValueAttribute(Color.black)]
	public Color SampleColor1;
	//[DefaultValueAttribute(Vector2.zero)]
	public Vector2 SampleUV1;
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
	public float Sample1Height;
	
	//[DefaultValueAttribute(Color.black)]
	public Color SampleColor2;
	//[DefaultValueAttribute(Vector2.zero)]
	public Vector2 SampleUV2;
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
	[DefaultValueAttribute(0.5f)]
	public float Sample2Height;
	
	[DefaultValueAttribute(0.5f)]
	public float SampleBlend;
	[DefaultValueAttribute("0.5")]
	public string SampleBlendText;

	[DefaultValueAttribute(50.0f)]
    public float Spread;
	[DefaultValueAttribute("50")]
    public string SpreadText;

	[DefaultValueAttribute(1.0f)]
	public float SpreadBoost;
	[DefaultValueAttribute("1")]
	public string SpreadBoostText;

	public HeightFromDiffuseSettings(){

		this.useAdjustedDiffuse = true;
		this.useOriginalDiffuse = false;
		this.useNormal = false;

		this.Blur0Weight = 0.15f;
		this.Blur1Weight = 0.19f;
		this.Blur2Weight = 0.3f;
		this.Blur3Weight = 0.5f;
		this.Blur4Weight = 0.7f;
		this.Blur5Weight = 0.9f;
		this.Blur6Weight = 1.0f;

		this.Blur0Contrast = 1.0f;
		this.Blur1Contrast = 1.0f;
		this.Blur2Contrast = 1.0f;
		this.Blur3Contrast = 1.0f;
		this.Blur4Contrast = 1.0f;
		this.Blur5Contrast = 1.0f;
		this.Blur6Contrast = 1.0f;

		this.SampleColor1 = Color.black;
		this.SampleUV1 = Vector2.zero;
		this.UseSample1 = false;
		this.IsolateSample1 = false;
		this.HueWeight1 = 1.0f;
		this.SatWeight1 = 0.5f;
		this.LumWeight1 = 0.2f;
		this.MaskLow1 = 0.0f;
		this.MaskHigh1 = 1.0f;
		this.Sample1Height = 0.5f;

		this.SampleColor2 = Color.black;
		this.SampleUV2 = Vector2.zero;
		this.UseSample2 = false;
		this.IsolateSample2 = false;
		this.HueWeight2 = 1.0f;
		this.SatWeight2 = 0.5f;
		this.LumWeight2 = 0.2f;
		this.MaskLow2 = 0.0f;
		this.MaskHigh2 = 1.0f;
		this.Sample2Height = 0.3f;

		this.FinalContrast = 1.5f;
		this.FinalContrastText = "1.5";

		this.FinalBias = 0.0f;
		this.FinalBiasText = "0.0";

		this.FinalGain = 0.0f;
		this.FinalGainText = "0.0";

		this.SampleBlend = 0.5f;
		this.SampleBlendText = "0.5";

		this.Spread = 50.0f;
		this.SpreadText = "50";

		this.SpreadBoost = 1.0f;
		this.SpreadBoostText = "1";
	}
}

public class HeightFromDiffuseGui : MonoBehaviour {
	
	public MainGui MainGuiScript;

    RenderTexture _TempBlurMap;
	RenderTexture _BlurMap0;
	RenderTexture _BlurMap1;
	RenderTexture _BlurMap2;
	RenderTexture _BlurMap3;
	RenderTexture _BlurMap4;
	RenderTexture _BlurMap5;
	RenderTexture _BlurMap6;
	RenderTexture _TempHeightMap;

	RenderTexture _AvgTempMap;
	RenderTexture _AvgMap;

	float _BlurScale = 1.0f;
	int imageSizeX = 1024;
	int imageSizeY = 1024;

	public Material thisMaterial;
	Material blitMaterial;
	Material blitMaterialSample;
    Material blitMaterialNormal;

    HeightFromDiffuseSettings  HFDS;

	float LastBlur0Contrast = 1.0f;

	int currentSelection = 0;
	bool selectingColor = false;
	bool mouseButtonDown = false;

	Texture2D _SampleColorMap1;
	Texture2D _SampleColorMap2;

	float Slider = 0.5f;

    bool lastUseDiffuse = false;
    bool lastUseOriginalDiffuse = false; 
    bool lastUseNormal = false;

	public GameObject testObject;
	bool doStuff = false;
	bool newTexture = false;

	Rect windowRect = new Rect (30, 300, 300, 480);
	bool settingsInitialized = false;

	public bool busy = false;
	
	public void GetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		projectObject.HFDS = HFDS;
	}

	public void SetValues( ProjectObject projectObject ) {
		InitializeSettings ();
		if (projectObject.HFDS != null) {
			HFDS = projectObject.HFDS;
		} else {
			settingsInitialized = false;
			InitializeSettings ();
		}

		_SampleColorMap1.SetPixel (1, 1, HFDS.SampleColor1);
		_SampleColorMap1.Apply ();

		_SampleColorMap2.SetPixel (1, 1, HFDS.SampleColor2);
		_SampleColorMap2.Apply ();

		doStuff = true;
	}

	void InitializeSettings() {

		if (settingsInitialized == false) {
			Debug.Log ("Initializing Height From Diffuse Settings");

			HFDS = new HeightFromDiffuseSettings ();

			if (_SampleColorMap1) {
				Destroy (_SampleColorMap1);
			}
			_SampleColorMap1 = new Texture2D (1, 1, TextureFormat.ARGB32, false, true);
			_SampleColorMap1.SetPixel (1, 1, HFDS.SampleColor1);
			_SampleColorMap1.Apply ();

			if (_SampleColorMap2) {
				Destroy (_SampleColorMap2);
			}
			_SampleColorMap2 = new Texture2D (1, 1, TextureFormat.ARGB32, false, true);
			_SampleColorMap2.SetPixel (1, 1, HFDS.SampleColor2);
			_SampleColorMap2.Apply ();

			settingsInitialized = true;
		}

	}

	// Use this for initialization
	void Start () {

        Resources.UnloadUnusedAssets();

		//MainGuiScript = MainGui.instance;

		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		blitMaterial = new Material (Shader.Find ("Hidden/Blit_Shader"));
		blitMaterialSample = new Material (Shader.Find ("Hidden/Blit_Sample"));
        blitMaterialNormal = new Material(Shader.Find("Hidden/Blit_Height_From_Normal"));

		InitializeSettings();

		if (newTexture)
        {
            InitializeTextures();
            newTexture = false;
        }
			
		FixUseMaps ();

		lastUseDiffuse = HFDS.useAdjustedDiffuse;
		lastUseOriginalDiffuse = HFDS.useOriginalDiffuse;
		lastUseNormal = HFDS.useNormal;
		LastBlur0Contrast = HFDS.Blur0Contrast;

		SetMaterialValues();
	}

	void FixUseMaps(){

		if (MainGuiScript._DiffuseMapOriginal == null && HFDS.useOriginalDiffuse) {
			HFDS.useAdjustedDiffuse = true;
			HFDS.useOriginalDiffuse = false;
			HFDS.useNormal = false;
		}

		if (MainGuiScript._DiffuseMap == null && HFDS.useAdjustedDiffuse) {
			HFDS.useAdjustedDiffuse = false;
			HFDS.useOriginalDiffuse = true;
			HFDS.useNormal = false;
		}

		if (MainGuiScript._NormalMap == null && HFDS.useNormal) {
			HFDS.useAdjustedDiffuse = true;
			HFDS.useOriginalDiffuse = false;
			HFDS.useNormal = false;
		}

		if (MainGuiScript._DiffuseMapOriginal == null & MainGuiScript._NormalMap == null)
		{
			HFDS.useAdjustedDiffuse = true;
			HFDS.useOriginalDiffuse = false;
			HFDS.useNormal = false;
		}

		if (MainGuiScript._DiffuseMap == null && MainGuiScript._NormalMap == null)
		{
			HFDS.useAdjustedDiffuse = false;
			HFDS.useOriginalDiffuse = true;
			HFDS.useNormal = false;
		}

		if (MainGuiScript._DiffuseMap == null && MainGuiScript._DiffuseMapOriginal == null)
		{
			HFDS.useAdjustedDiffuse = false;
			HFDS.useOriginalDiffuse = false;
			HFDS.useNormal = true;
		}
	}

	public void DoStuff() {
		doStuff = true;
	}

	public void NewTexture() {
		newTexture = true;
	}

	void SetMaterialValues() {
		
		thisMaterial.SetFloat ("_BlurScale", _BlurScale);
		thisMaterial.SetVector ("_ImageSize", new Vector4( imageSizeX, imageSizeY, 0, 0 ));
		
	}

	void SetWeightEQDefault() {
		HFDS.Blur0Weight = 0.15f;
		HFDS.Blur1Weight = 0.19f;
		HFDS.Blur2Weight = 0.3f;
		HFDS.Blur3Weight = 0.5f;
		HFDS.Blur4Weight = 0.7f;
		HFDS.Blur5Weight = 0.9f;
		HFDS.Blur6Weight = 1.0f;
		doStuff = true;
	}

	void SetWeightEQDetail() {
		HFDS.Blur0Weight = 0.7f;
		HFDS.Blur1Weight = 0.4f;
		HFDS.Blur2Weight = 0.3f;
		HFDS.Blur3Weight = 0.5f;
		HFDS.Blur4Weight = 0.8f;
		HFDS.Blur5Weight = 0.9f;
		HFDS.Blur6Weight = 0.7f;
		doStuff = true;
	}

	void SetWeightEQDisplace() {
		HFDS.Blur0Weight = 0.02f;
		HFDS.Blur1Weight = 0.03f;
		HFDS.Blur2Weight = 0.1f;
		HFDS.Blur3Weight = 0.35f;
		HFDS.Blur4Weight = 0.7f;
		HFDS.Blur5Weight = 0.9f;
		HFDS.Blur6Weight = 1.0f;
		doStuff = true;
	}

	void SetContrastEQDefault() {
		HFDS.Blur0Contrast = 1.0f;
		HFDS.Blur1Contrast = 1.0f;
		HFDS.Blur2Contrast = 1.0f;
		HFDS.Blur3Contrast = 1.0f;
		HFDS.Blur4Contrast = 1.0f;
		HFDS.Blur5Contrast = 1.0f;
		HFDS.Blur6Contrast = 1.0f;
		doStuff = true;
	}

	void SetContrastEQCrackedMud() {
		HFDS.Blur0Contrast = 1.0f;
		HFDS.Blur1Contrast = 1.0f;
		HFDS.Blur2Contrast = 1.0f;
		HFDS.Blur3Contrast = 1.0f;
		HFDS.Blur4Contrast = -0.2f;
		HFDS.Blur5Contrast = -2.0f;
		HFDS.Blur6Contrast = -4.0f;
		doStuff = true;
	}

	void SetContrastEQFunky() {
		HFDS.Blur0Contrast = -3.0f;
		HFDS.Blur1Contrast = -1.2f;
		HFDS.Blur2Contrast = 0.30f;
		HFDS.Blur3Contrast = 1.3f;
		HFDS.Blur4Contrast = 2.0f;
		HFDS.Blur5Contrast = 2.5f;
		HFDS.Blur6Contrast = 2.0f;
		doStuff = true;
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
			if( HFDS.useAdjustedDiffuse){
				sampledColor = MainGuiScript._DiffuseMap.GetPixelBilinear(pixelUV.x, pixelUV.y);
			}else{
				sampledColor = MainGuiScript._DiffuseMapOriginal.GetPixelBilinear(pixelUV.x, pixelUV.y);
			}

			if( currentSelection == 1 ){
				HFDS.SampleUV1 = pixelUV;
				HFDS.SampleColor1 = sampledColor;
				_SampleColorMap1.SetPixel (1, 1, HFDS.SampleColor1);
				_SampleColorMap1.Apply ();
			}
			
			if( currentSelection == 2 ){
				HFDS.SampleUV2 = pixelUV;
				HFDS.SampleColor2 = sampledColor;
				_SampleColorMap2.SetPixel (1, 1, HFDS.SampleColor2);
				_SampleColorMap2.Apply ();
			}

			doStuff = true;
			
		}
		
		if ( Input.GetMouseButtonUp(0) && mouseButtonDown ) {
			
			mouseButtonDown = false;
			selectingColor = false;
			currentSelection = 0;
			
		}
		
	}
	
	// Update is called once per frame
	void Update () {

		if (selectingColor) {
			SelectColor();
		}
        
		if (HFDS.useAdjustedDiffuse != lastUseDiffuse) {
			lastUseDiffuse = HFDS.useAdjustedDiffuse;
			doStuff = true;
		}

        if (HFDS.useOriginalDiffuse != lastUseOriginalDiffuse)
        {
            lastUseOriginalDiffuse = HFDS.useOriginalDiffuse;
            doStuff = true;
        }

        if (HFDS.useNormal != lastUseNormal)
        {
            lastUseNormal = HFDS.useNormal;
            doStuff = true;
        }

        if (HFDS.Blur0Contrast != LastBlur0Contrast)
        {
			LastBlur0Contrast = HFDS.Blur0Contrast;
			doStuff = true;
		}
        
		if (newTexture)
        {
			InitializeTextures();
			newTexture = false;
		}

		if (doStuff)
        {

            if (HFDS.useNormal)
            {
                StopAllCoroutines();
                StartCoroutine(ProcessNormal());
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(ProcessDiffuse());
            }
            
			doStuff = false;
		}

		if (HFDS.IsolateSample1 || HFDS.IsolateSample2) {
			thisMaterial.SetInt ("_Isolate", 1);
		} else {
			thisMaterial.SetInt ("_Isolate", 0);
		}

		thisMaterial.SetFloat ("_Blur0Weight", HFDS.Blur0Weight);
		thisMaterial.SetFloat ("_Blur1Weight", HFDS.Blur1Weight);
		thisMaterial.SetFloat ("_Blur2Weight", HFDS.Blur2Weight);
		thisMaterial.SetFloat ("_Blur3Weight", HFDS.Blur3Weight);
		thisMaterial.SetFloat ("_Blur4Weight", HFDS.Blur4Weight);
		thisMaterial.SetFloat ("_Blur5Weight", HFDS.Blur5Weight);
		thisMaterial.SetFloat ("_Blur6Weight", HFDS.Blur6Weight);

		thisMaterial.SetFloat ("_Blur0Contrast", HFDS.Blur0Contrast);
		thisMaterial.SetFloat ("_Blur1Contrast", HFDS.Blur1Contrast);
		thisMaterial.SetFloat ("_Blur2Contrast", HFDS.Blur2Contrast);
		thisMaterial.SetFloat ("_Blur3Contrast", HFDS.Blur3Contrast);
		thisMaterial.SetFloat ("_Blur4Contrast", HFDS.Blur4Contrast);
		thisMaterial.SetFloat ("_Blur5Contrast", HFDS.Blur5Contrast);
		thisMaterial.SetFloat ("_Blur6Contrast", HFDS.Blur6Contrast);

		float realGain = HFDS.FinalGain;
		if (realGain < 0.0f) {
			realGain = Mathf.Abs( 1.0f / ( realGain - 1.0f ) );
		} else {
			realGain = realGain + 1.0f;
		}

		thisMaterial.SetFloat ("_FinalGain", realGain);
		thisMaterial.SetFloat ("_FinalContrast", HFDS.FinalContrast);
		thisMaterial.SetFloat ("_FinalBias", HFDS.FinalBias);

		thisMaterial.SetFloat ("_Slider", Slider);
	
	}

	string FloatToString ( float num, int length ) {
		
		string numString = num.ToString ();
		int numStringLength = numString.Length;
		int lastIndex = Mathf.FloorToInt( Mathf.Min ( (float)numStringLength , (float)length ) );
		
		return numString.Substring (0, lastIndex);
	}

	void DoMyWindow ( int windowID ) {
		
		int offsetX = 10;
		int offsetY = 30;

		if (MainGuiScript._DiffuseMap != null) { GUI.enabled = true; } else { GUI.enabled = false; }
		HFDS.useAdjustedDiffuse = GUI.Toggle(new Rect(offsetX, offsetY, 80, 30), HFDS.useAdjustedDiffuse, " Diffuse");
		if (HFDS.useAdjustedDiffuse)
        {
            HFDS.useOriginalDiffuse = false;
            HFDS.useNormal = false;
        }
        else if (!HFDS.useOriginalDiffuse && !HFDS.useNormal)
        {
			HFDS.useAdjustedDiffuse = true;
        }

		if (MainGuiScript._DiffuseMapOriginal != null) { GUI.enabled = true; } else { GUI.enabled = false; }
        HFDS.useOriginalDiffuse = GUI.Toggle (new Rect (offsetX + 80, offsetY, 120, 30), HFDS.useOriginalDiffuse, "Original Diffuse");
		if (HFDS.useOriginalDiffuse)
        {
			HFDS.useAdjustedDiffuse = false;
            HFDS.useNormal = false;
        }
		else if (!HFDS.useAdjustedDiffuse && !HFDS.useNormal)
        {
            HFDS.useOriginalDiffuse = true;
        }

		if (MainGuiScript._NormalMap) { GUI.enabled = true; } else { GUI.enabled = false; }
        HFDS.useNormal = GUI.Toggle(new Rect(offsetX + 210, offsetY, 80, 30), HFDS.useNormal, " Normal");
        if (HFDS.useNormal)
        {
			HFDS.useAdjustedDiffuse = false;
            HFDS.useOriginalDiffuse = false;
        }
		else if(!HFDS.useAdjustedDiffuse && !HFDS.useOriginalDiffuse)
        {
            HFDS.useNormal = true;
        }
        GUI.enabled = true;
        offsetY += 30;

		GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Height Reveal Slider");
		Slider = GUI.HorizontalSlider(new Rect(offsetX, offsetY + 20, 280, 10), Slider, 0.0f, 1.0f);
		offsetY += 40;

        if (HFDS.useNormal)
        {

			if( GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 10), "Sample Spread", HFDS.Spread, HFDS.SpreadText, out HFDS.Spread, out HFDS.SpreadText, 10.0f, 200.0f) ){
				doStuff = true;
			}

            offsetY += 40;

			if( GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 10), "Sample Spread Boost", HFDS.SpreadBoost, HFDS.SpreadBoostText, out HFDS.SpreadBoost, out HFDS.SpreadBoostText, 1.0f, 5.0f) ){
				doStuff = true;
			}

			offsetY += 40;

        } else {

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Weight Equalizer");
            GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default"))
            {
                SetWeightEQDefault();
            }
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Details"))
            {
                SetWeightEQDetail();
            }
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Displace"))
            {
                SetWeightEQDisplace();
            }

            offsetY += 30;
            offsetX += 10;
            HFDS.Blur0Weight = GUI.VerticalSlider(new Rect(offsetX + 180, offsetY, 10, 80), HFDS.Blur0Weight, 1.0f, 0.0f);
            HFDS.Blur1Weight = GUI.VerticalSlider(new Rect(offsetX + 150, offsetY, 10, 80), HFDS.Blur1Weight, 1.0f, 0.0f);
            HFDS.Blur2Weight = GUI.VerticalSlider(new Rect(offsetX + 120, offsetY, 10, 80), HFDS.Blur2Weight, 1.0f, 0.0f);
            HFDS.Blur3Weight = GUI.VerticalSlider(new Rect(offsetX + 90, offsetY, 10, 80), HFDS.Blur3Weight, 1.0f, 0.0f);
            HFDS.Blur4Weight = GUI.VerticalSlider(new Rect(offsetX + 60, offsetY, 10, 80), HFDS.Blur4Weight, 1.0f, 0.0f);
            HFDS.Blur5Weight = GUI.VerticalSlider(new Rect(offsetX + 30, offsetY, 10, 80), HFDS.Blur5Weight, 1.0f, 0.0f);
            HFDS.Blur6Weight = GUI.VerticalSlider(new Rect(offsetX + 0, offsetY, 10, 80), HFDS.Blur6Weight, 1.0f, 0.0f);
            offsetX -= 10;
            offsetY += 100;


            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Frequency Contrast Equalizer");
            GUI.Label(new Rect(offsetX + 225, offsetY, 100, 30), "Presets");
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 30, 60, 20), "Default"))
            {
                SetContrastEQDefault();
            }
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 60, 60, 20), "Cracks"))
            {
                SetContrastEQCrackedMud();
            }
            if (GUI.Button(new Rect(offsetX + 215, offsetY + 90, 60, 20), "Funky"))
            {
                SetContrastEQFunky();
            }
            offsetY += 30;
            offsetX += 10;
            HFDS.Blur0Contrast = GUI.VerticalSlider(new Rect(offsetX + 180, offsetY, 10, 80), HFDS.Blur0Contrast, 5.0f, -5.0f);
            HFDS.Blur1Contrast = GUI.VerticalSlider(new Rect(offsetX + 150, offsetY, 10, 80), HFDS.Blur1Contrast, 5.0f, -5.0f);
            HFDS.Blur2Contrast = GUI.VerticalSlider(new Rect(offsetX + 120, offsetY, 10, 80), HFDS.Blur2Contrast, 5.0f, -5.0f);
            HFDS.Blur3Contrast = GUI.VerticalSlider(new Rect(offsetX + 90, offsetY, 10, 80), HFDS.Blur3Contrast, 5.0f, -5.0f);
            HFDS.Blur4Contrast = GUI.VerticalSlider(new Rect(offsetX + 60, offsetY, 10, 80), HFDS.Blur4Contrast, 5.0f, -5.0f);
            HFDS.Blur5Contrast = GUI.VerticalSlider(new Rect(offsetX + 30, offsetY, 10, 80), HFDS.Blur5Contrast, 5.0f, -5.0f);
            HFDS.Blur6Contrast = GUI.VerticalSlider(new Rect(offsetX + 0, offsetY, 10, 80), HFDS.Blur6Contrast, 5.0f, -5.0f);
            offsetX -= 10;
            GUI.Label(new Rect(offsetX + 210, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 180, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 150, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 120, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 90, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 60, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 30, offsetY + 21, 30, 30), "-");
            GUI.Label(new Rect(offsetX + 0, offsetY + 21, 30, 30), "-");
            offsetY += 100;



            doStuff = GuiHelper.Toggle(new Rect(offsetX, offsetY, 150, 20), HFDS.UseSample1, out HFDS.UseSample1, "Use Color Sample 1", doStuff);
            if (HFDS.UseSample1)
            {

                doStuff = GuiHelper.Toggle(new Rect(offsetX + 180, offsetY, 150, 20), HFDS.IsolateSample1, out HFDS.IsolateSample1, "Isolate Mask", doStuff);
                if (HFDS.IsolateSample1)
                {
                    HFDS.IsolateSample2 = false;
                }
                offsetY += 30;

                if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
                {
                    selectingColor = true;
                    currentSelection = 1;
                }

                GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _SampleColorMap1);

                GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70), HFDS.HueWeight1, out HFDS.HueWeight1, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70), HFDS.SatWeight1, out HFDS.SatWeight1, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70), HFDS.LumWeight1, out HFDS.LumWeight1, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70), HFDS.MaskLow1, out HFDS.MaskLow1, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70), HFDS.MaskHigh1, out HFDS.MaskHigh1, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Height");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70), HFDS.Sample1Height, out HFDS.Sample1Height, 1.0f, 0.0f, doStuff);

                offsetY += 110;
            }
            else
            {
                offsetY += 30;
                HFDS.IsolateSample1 = false;
            }


            doStuff = GuiHelper.Toggle(new Rect(offsetX, offsetY, 150, 20), HFDS.UseSample2, out HFDS.UseSample2, "Use Color Sample 2", doStuff);
            if (HFDS.UseSample2)
            {

                doStuff = GuiHelper.Toggle(new Rect(offsetX + 180, offsetY, 150, 20), HFDS.IsolateSample2, out HFDS.IsolateSample2, "Isolate Mask", doStuff);
                if (HFDS.IsolateSample2)
                {
                    HFDS.IsolateSample1 = false;
                }
                offsetY += 30;

                if (GUI.Button(new Rect(offsetX, offsetY + 5, 80, 20), "Pick Color"))
                {
                    selectingColor = true;
                    currentSelection = 2;
                }

                GUI.DrawTexture(new Rect(offsetX + 10, offsetY + 35, 60, 60), _SampleColorMap2);

                GUI.Label(new Rect(offsetX + 90, offsetY, 250, 30), "Hue");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 95, offsetY + 30, 10, 70), HFDS.HueWeight2, out HFDS.HueWeight2, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 120, offsetY, 250, 30), "Sat");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 125, offsetY + 30, 10, 70), HFDS.SatWeight2, out HFDS.SatWeight2, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 150, offsetY, 250, 30), "Lum");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 155, offsetY + 30, 10, 70), HFDS.LumWeight2, out HFDS.LumWeight2, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 180, offsetY, 250, 30), "Low");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 185, offsetY + 30, 10, 70), HFDS.MaskLow2, out HFDS.MaskLow2, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 210, offsetY, 250, 30), "High");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 215, offsetY + 30, 10, 70), HFDS.MaskHigh2, out HFDS.MaskHigh2, 1.0f, 0.0f, doStuff);

                GUI.Label(new Rect(offsetX + 240, offsetY, 250, 30), "Height");
                doStuff = GuiHelper.VerticalSlider(new Rect(offsetX + 255, offsetY + 30, 10, 70), HFDS.Sample2Height, out HFDS.Sample2Height, 1.0f, 0.0f, doStuff);

                offsetY += 110;
            }
            else
            {
                offsetY += 30;
                HFDS.IsolateSample2 = false;
            }

            if (HFDS.UseSample1 || HFDS.UseSample2)
            {

                if (GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Sample Blend", HFDS.SampleBlend, HFDS.SampleBlendText, out HFDS.SampleBlend, out HFDS.SampleBlendText, 0.0f, 1.0f))
                {
                    doStuff = true;
                }
                offsetY += 40;
            }
        }

        

		GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Gain", HFDS.FinalGain, HFDS.FinalGainText, out HFDS.FinalGain, out HFDS.FinalGainText, -0.5f, 0.5f);
		offsetY += 40;

		GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Contrast", HFDS.FinalContrast, HFDS.FinalContrastText, out HFDS.FinalContrast, out HFDS.FinalContrastText, -10.0f, 10.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Final Bias", HFDS.FinalBias, HFDS.FinalBiasText, out HFDS.FinalBias, out HFDS.FinalBiasText, -1.0f, 1.0f);
        offsetY += 50;

		if (busy) { GUI.enabled = false; } else { GUI.enabled = true; }
		if( GUI.Button (new Rect (offsetX + 150, offsetY, 130, 30), "Set as Height Map" ) ){
            StartCoroutine( ProcessHeight () );
		}
		GUI.enabled = true;

		GUI.DragWindow();
	}

	void OnGUI () {

		windowRect.width = 300;
		windowRect.height = 590;

		if (HFDS.UseSample1 && !HFDS.useNormal) {
			windowRect.height += 110;
		}

		if (HFDS.UseSample2 && !HFDS.useNormal) {
			windowRect.height += 110;
		}

		if ((HFDS.UseSample1 || HFDS.UseSample2) && !HFDS.useNormal) {
			windowRect.height += 40;
		}

		windowRect = GUI.Window(13, windowRect, DoMyWindow, "Height From Diffuse");

	}

	public void InitializeTextures() {

		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;

		CleanupTextures ();

		FixUseMaps ();

		if ( HFDS.useAdjustedDiffuse )
        {
			imageSizeX = MainGuiScript._DiffuseMap.width;
			imageSizeY = MainGuiScript._DiffuseMap.height;
        }
		else if (HFDS.useOriginalDiffuse)
        {
			imageSizeX = MainGuiScript._DiffuseMapOriginal.width;
			imageSizeY = MainGuiScript._DiffuseMapOriginal.height;
        }
		else if (HFDS.useNormal)
        {
			imageSizeX = MainGuiScript._NormalMap.width;
			imageSizeY = MainGuiScript._NormalMap.height;
        }


        Debug.Log ( "Initializing Textures of size: " + imageSizeX.ToString() + "x" + imageSizeY.ToString() );

		_TempBlurMap = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_TempBlurMap.wrapMode = TextureWrapMode.Repeat;
		_BlurMap0 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_BlurMap0.wrapMode = TextureWrapMode.Repeat;
		_BlurMap1 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_BlurMap1.wrapMode = TextureWrapMode.Repeat;
		_BlurMap2 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_BlurMap2.wrapMode = TextureWrapMode.Repeat;
		_BlurMap3 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_BlurMap3.wrapMode = TextureWrapMode.Repeat;
		_BlurMap4 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_BlurMap4.wrapMode = TextureWrapMode.Repeat;
		_BlurMap5 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_BlurMap5.wrapMode = TextureWrapMode.Repeat;
		_BlurMap6 = new RenderTexture (imageSizeX, imageSizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_BlurMap6.wrapMode = TextureWrapMode.Repeat;

		_AvgMap = new RenderTexture (256, 256, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_AvgMap.wrapMode = TextureWrapMode.Repeat;

		_AvgTempMap = new RenderTexture (256, 256, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		_AvgTempMap.wrapMode = TextureWrapMode.Repeat;

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

		CleanupTexture( _TempBlurMap );
		CleanupTexture( _BlurMap0 );
		CleanupTexture( _BlurMap1 );
		CleanupTexture( _BlurMap2 );
		CleanupTexture( _BlurMap3 );
		CleanupTexture( _BlurMap4 );
		CleanupTexture( _BlurMap5 );
		CleanupTexture( _BlurMap6 );
		CleanupTexture (_TempHeightMap);
		CleanupTexture( _AvgMap );
		CleanupTexture( _AvgTempMap );

	}

	public IEnumerator ProcessHeight() {

		busy = true;

		Debug.Log ("Processing Height");

		blitMaterial.SetVector ("_ImageSize", new Vector4( imageSizeX, imageSizeY, 0, 0 ));

        CleanupTexture(_TempHeightMap);
		_TempHeightMap = new RenderTexture(imageSizeX, imageSizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		_TempHeightMap.wrapMode = TextureWrapMode.Repeat;

        blitMaterial.SetFloat("_FinalContrast", HFDS.FinalContrast);
        blitMaterial.SetFloat("_FinalBias", HFDS.FinalBias);

		float realGain = HFDS.FinalGain;
		if (realGain < 0.0f) {
			realGain = Mathf.Abs( 1.0f / ( realGain - 1.0f ) );
		} else {
			realGain = realGain + 1.0f;
		}
		blitMaterial.SetFloat("_FinalGain", realGain);

        if (HFDS.useNormal)
        {

            blitMaterial.SetTexture("_BlurTex0", _BlurMap0);
            blitMaterial.SetFloat("_HeightFromNormal", 1.0f);
            // Save low fidelity for texture 2d
            Graphics.Blit(_BlurMap0, _TempHeightMap, blitMaterial, 2);

        }
        else
        {
            blitMaterial.SetFloat("_HeightFromNormal", 0.0f);

            blitMaterial.SetFloat("_Blur0Weight", HFDS.Blur0Weight);
            blitMaterial.SetFloat("_Blur1Weight", HFDS.Blur1Weight);
            blitMaterial.SetFloat("_Blur2Weight", HFDS.Blur2Weight);
            blitMaterial.SetFloat("_Blur3Weight", HFDS.Blur3Weight);
            blitMaterial.SetFloat("_Blur4Weight", HFDS.Blur4Weight);
            blitMaterial.SetFloat("_Blur5Weight", HFDS.Blur5Weight);
            blitMaterial.SetFloat("_Blur6Weight", HFDS.Blur6Weight);

            blitMaterial.SetFloat("_Blur0Contrast", HFDS.Blur0Contrast);
            blitMaterial.SetFloat("_Blur1Contrast", HFDS.Blur1Contrast);
            blitMaterial.SetFloat("_Blur2Contrast", HFDS.Blur2Contrast);
            blitMaterial.SetFloat("_Blur3Contrast", HFDS.Blur3Contrast);
            blitMaterial.SetFloat("_Blur4Contrast", HFDS.Blur4Contrast);
            blitMaterial.SetFloat("_Blur5Contrast", HFDS.Blur5Contrast);
            blitMaterial.SetFloat("_Blur6Contrast", HFDS.Blur6Contrast);

            blitMaterial.SetTexture("_BlurTex0", _BlurMap0);
            blitMaterial.SetTexture("_BlurTex1", _BlurMap1);
            blitMaterial.SetTexture("_BlurTex2", _BlurMap2);
            blitMaterial.SetTexture("_BlurTex3", _BlurMap3);
            blitMaterial.SetTexture("_BlurTex4", _BlurMap4);
            blitMaterial.SetTexture("_BlurTex5", _BlurMap5);
            blitMaterial.SetTexture("_BlurTex6", _BlurMap6);

			blitMaterial.SetTexture ("_AvgTex", _AvgMap);

            // Save low fidelity for texture 2d
            Graphics.Blit(_BlurMap0, _TempHeightMap, blitMaterial, 2);
        }


		if (MainGuiScript._HeightMap != null) {
			Destroy (MainGuiScript._HeightMap);
		}

		RenderTexture.active = _TempHeightMap;
		MainGuiScript._HeightMap = new Texture2D( _TempHeightMap.width, _TempHeightMap.height, TextureFormat.ARGB32, true, true );
		MainGuiScript._HeightMap.ReadPixels(new Rect(0, 0, _TempHeightMap.width, _TempHeightMap.height), 0, 0);
		MainGuiScript._HeightMap.Apply();
		RenderTexture.active = null;

		// Save high fidelity for normal making
		if (MainGuiScript._HDHeightMap != null) {
			MainGuiScript._HDHeightMap.Release ();
			MainGuiScript._HDHeightMap = null;
		}
		MainGuiScript._HDHeightMap = new RenderTexture (_TempHeightMap.width, _TempHeightMap.height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
		MainGuiScript._HDHeightMap.wrapMode = TextureWrapMode.Repeat;
		Graphics.Blit(_BlurMap0, MainGuiScript._HDHeightMap, blitMaterial, 2);

		CleanupTexture (_TempHeightMap);

		yield return new WaitForSeconds(0.1f);

		busy = false;
	}

    public IEnumerator ProcessNormal()
    {
        busy = true;

        Debug.Log("Processing Normal");

        blitMaterialNormal.SetVector("_ImageSize", new Vector4(imageSizeX, imageSizeY, 0, 0));
        blitMaterialNormal.SetFloat("_Spread", HFDS.Spread);
		blitMaterialNormal.SetFloat("_SpreadBoost", HFDS.SpreadBoost);
		blitMaterialNormal.SetFloat("_Samples", (int)HFDS.Spread);
		blitMaterialNormal.SetTexture("_MainTex", MainGuiScript._NormalMap);
		blitMaterialNormal.SetTexture("_BlendTex", _BlurMap1);

        thisMaterial.SetFloat("_IsNormal", 1.0f);
        thisMaterial.SetTexture("_BlurTex0", _BlurMap0);
		thisMaterial.SetTexture("_BlurTex1", _BlurMap1);
		thisMaterial.SetTexture("_MainTex", MainGuiScript._NormalMap);

		int yieldCountDown = 5;

        for (int i = 1; i < 100; i++)
        {

			blitMaterialNormal.SetFloat("_BlendAmount", 1.0f / (float)i);
            blitMaterialNormal.SetFloat("_Progress", (float)i / 100.0f);

			Graphics.Blit(MainGuiScript._NormalMap, _BlurMap0, blitMaterialNormal, 0);
			Graphics.Blit(_BlurMap0, _BlurMap1);

			yieldCountDown -= 1;
			if( yieldCountDown <= 0 ){
				yieldCountDown = 5;
				yield return new WaitForSeconds(0.01f);
			}
        }

        busy = false;

    }

    public IEnumerator ProcessDiffuse ()
    {
		busy = true;

		Debug.Log ("Processing Diffuse");

        thisMaterial.SetFloat("_IsNormal", 0.0f);

        if (HFDS.IsolateSample1) {
			blitMaterialSample.SetInt ("_IsolateSample1", 1);
		} else {
			blitMaterialSample.SetInt ("_IsolateSample1", 0);
		}
		if (HFDS.UseSample1) {
			blitMaterialSample.SetInt ("_UseSample1", 1);
		} else {
			blitMaterialSample.SetInt ("_UseSample1", 0);
		}
		blitMaterialSample.SetColor ("_SampleColor1", HFDS.SampleColor1);
		blitMaterialSample.SetVector ("_SampleUV1", new Vector4 (HFDS.SampleUV1.x, HFDS.SampleUV1.y, 0, 0));
		blitMaterialSample.SetFloat ("_HueWeight1", HFDS.HueWeight1);
		blitMaterialSample.SetFloat ("_SatWeight1", HFDS.SatWeight1);
		blitMaterialSample.SetFloat ("_LumWeight1", HFDS.LumWeight1);
		blitMaterialSample.SetFloat ("_MaskLow1", HFDS.MaskLow1);
		blitMaterialSample.SetFloat ("_MaskHigh1", HFDS.MaskHigh1);
		blitMaterialSample.SetFloat ("_Sample1Height", HFDS.Sample1Height);
		
		if (HFDS.IsolateSample2) {
			blitMaterialSample.SetInt ("_IsolateSample2", 1);
		} else {
			blitMaterialSample.SetInt ("_IsolateSample2", 0);
		}
		if (HFDS.UseSample2) {
			blitMaterialSample.SetInt ("_UseSample2", 1);
		} else {
			blitMaterialSample.SetInt ("_UseSample2", 0);
		}
		blitMaterialSample.SetColor ("_SampleColor2", HFDS.SampleColor2);
		blitMaterialSample.SetVector ("_SampleUV2", new Vector4 (HFDS.SampleUV2.x, HFDS.SampleUV2.y, 0, 0));
		blitMaterialSample.SetFloat ("_HueWeight2", HFDS.HueWeight2);
		blitMaterialSample.SetFloat ("_SatWeight2", HFDS.SatWeight2);
		blitMaterialSample.SetFloat ("_LumWeight2", HFDS.LumWeight2);
		blitMaterialSample.SetFloat ("_MaskLow2", HFDS.MaskLow2);
		blitMaterialSample.SetFloat ("_MaskHigh2", HFDS.MaskHigh2);
		blitMaterialSample.SetFloat ("_Sample2Height", HFDS.Sample2Height);

		if (HFDS.UseSample1 == false && HFDS.UseSample2 == false) {
			blitMaterialSample.SetFloat ("_SampleBlend", 0.0f);
		} else {
			blitMaterialSample.SetFloat ("_SampleBlend", HFDS.SampleBlend);
		}

		blitMaterialSample.SetFloat ("_FinalContrast", HFDS.FinalContrast);
		blitMaterialSample.SetFloat ("_FinalBias", HFDS.FinalBias);

        if (HFDS.useOriginalDiffuse) {
			Graphics.Blit(MainGuiScript._DiffuseMapOriginal, _BlurMap0, blitMaterialSample, 0);
        } else {
			Graphics.Blit(MainGuiScript._DiffuseMap, _BlurMap0, blitMaterialSample, 0);  
        }
        
		blitMaterial.SetVector ("_ImageSize", new Vector4( imageSizeX, imageSizeY, 0, 0 ) );
		blitMaterial.SetFloat ("_BlurContrast", 1.0f);

		float extraSpread = ( (float)(_BlurMap0.width + _BlurMap0.height) * 0.5f ) / 1024.0f;
		float spread = 1.0f;

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


		// Average Color
		blitMaterial.SetInt ("_BlurSamples", 32);
		blitMaterial.SetFloat ("_BlurSpread", 64.0f * extraSpread);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(1,0,0,0) );
		Graphics.Blit(_BlurMap6, _AvgTempMap, blitMaterial, 1);
		blitMaterial.SetVector ("_BlurDirection", new Vector4(0,1,0,0) );
		Graphics.Blit(_AvgTempMap, _AvgMap, blitMaterial, 1);


		if ( HFDS.useOriginalDiffuse ) {
			thisMaterial.SetTexture("_MainTex", MainGuiScript._DiffuseMapOriginal);
        } else {
			thisMaterial.SetTexture("_MainTex", MainGuiScript._DiffuseMap);
		}

		thisMaterial.SetTexture ("_BlurTex0", _BlurMap0);
		thisMaterial.SetTexture ("_BlurTex1", _BlurMap1);
		thisMaterial.SetTexture ("_BlurTex2", _BlurMap2);
		thisMaterial.SetTexture ("_BlurTex3", _BlurMap3);
		thisMaterial.SetTexture ("_BlurTex4", _BlurMap4);
		thisMaterial.SetTexture ("_BlurTex5", _BlurMap5);
		thisMaterial.SetTexture ("_BlurTex6", _BlurMap6);
		thisMaterial.SetTexture ("_AvgTex", _AvgMap);

		yield return new WaitForSeconds(0.01f);

		busy = false;
	}
}
