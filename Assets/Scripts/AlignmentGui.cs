using UnityEngine;
using System.Collections;

public class AlignmentGui : MonoBehaviour {
	
	RenderTexture _LensMap;
	RenderTexture _AlignMap;
	RenderTexture _PerspectiveMap;

	Texture2D textureToAlign;

	Material blitMaterial;

	public Material thisMaterial;
	public GameObject testObject;

	MainGui MGS;
	public bool newTexture = false;

	Rect windowRect = new Rect (30, 300, 300, 530);
	
	bool doStuff = false;

	Vector2 pointTL = new Vector2(0.0f,1.0f);
	Vector2 pointTR = new Vector2(1.0f,1.0f);
	Vector2 pointBL = new Vector2(0.0f,0.0f);
	Vector2 pointBR = new Vector2(1.0f,0.0f);
	

	int GrabbedPoint = 0;
	Vector2 StartOffset = Vector2.zero;

	float Slider = 0.5f;

	float LensDistort = 0.0f;
	string LensDistortText = "0.0";

	float PerspectiveX = 0.0f;
	string PerspectiveXText = "0.0";

	float PerspectiveY = 0.0f;
	string PerspectiveYText = "0.0";

	// Use this for initialization
	void Start () {

	}

	public void Initialize() {
		this.gameObject.SetActive(true);
		MGS = MainGui.instance;
		testObject.GetComponent<Renderer>().sharedMaterial = thisMaterial;
		blitMaterial = new Material (Shader.Find ("Hidden/Blit_Alignment"));
		blitMaterial.hideFlags = HideFlags.HideAndDontSave;

		if (MGS._DiffuseMapOriginal != null) {
			textureToAlign = MGS._DiffuseMapOriginal;
		}else if (MGS._HeightMap != null) {
			textureToAlign = MGS._HeightMap;
		}else if (MGS._MetallicMap != null) {
			textureToAlign = MGS._MetallicMap;
		}else if (MGS._SmoothnessMap != null) {
			textureToAlign = MGS._SmoothnessMap;
		}else if (MGS._EdgeMap != null) {
			textureToAlign = MGS._EdgeMap;
		}else if (MGS._AOMap != null) {
			textureToAlign = MGS._AOMap;
		}


		doStuff = true;
	}


	void CleanupTexture( RenderTexture _Texture ) {
		
		if (_Texture != null) {
			_Texture.Release();
			_Texture = null;
		}
	}

	public void Close(){
		CleanupTexture (_LensMap );
		CleanupTexture (_AlignMap );
		CleanupTexture (_PerspectiveMap );
		this.gameObject.SetActive (false);
	}

	void SelectClosestPoint() {

		if (!Input.GetMouseButton (0)) {

			RaycastHit hit;
			if (!Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit))
				return;

			Vector2 hitTC = hit.textureCoord;

			float dist1 = Vector2.Distance (hitTC, pointTL);
			float dist2 = Vector2.Distance (hitTC, pointTR);
			float dist3 = Vector2.Distance (hitTC, pointBL);
			float dist4 = Vector2.Distance (hitTC, pointBR);

			float closestDist = dist1;
			Vector2 closestPoint = pointTL;
			GrabbedPoint = 0;
			if (dist2 < closestDist) {
				closestDist = dist2;
				closestPoint = pointTR;
				GrabbedPoint = 1;
			}
			if (dist3 < closestDist) {
				closestDist = dist3;
				closestPoint = pointBL;
				GrabbedPoint = 2;
			}
			if (dist4 < closestDist) {
				closestDist = dist4;
				closestPoint = pointBR;
				GrabbedPoint = 3;
			}

			if( closestDist > 0.1f ){
				closestPoint = new Vector2(-1,-1);
				GrabbedPoint = -1;
			}

			thisMaterial.SetVector ("_TargetPoint", closestPoint);

		}

	}

	void DragPoint () {

		RaycastHit hit;
		if (!Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit))
			return;
		
		Vector2 hitTC = hit.textureCoord;

		if (Input.GetMouseButtonDown (0)) {
			StartOffset = hitTC;
		} else if(Input.GetMouseButton (0)) {

			switch(GrabbedPoint){
			case 0:
				pointTL += hitTC - StartOffset;
				thisMaterial.SetVector ("_TargetPoint", pointTL);
				break;
			case 1:
				pointTR += hitTC - StartOffset;
				thisMaterial.SetVector ("_TargetPoint", pointTR);
				break;
			case 2:
				pointBL += hitTC - StartOffset;
				thisMaterial.SetVector ("_TargetPoint", pointBL);
				break;
			case 3:
				pointBR += hitTC - StartOffset;
				thisMaterial.SetVector ("_TargetPoint", pointBR);
				break;
			}

			StartOffset = hitTC;
		}

		doStuff = true;
	}

	// Update is called once per frame
	void Update () {

		SelectClosestPoint ();
		DragPoint ();

		float aspect = (float)textureToAlign.width / (float)textureToAlign.height;
		float area = 1.0f;
		Vector2 pointScale = Vector2.one;
		pointScale.x = aspect;
		float newArea = pointScale.x * pointScale.y;
		float areaScale = Mathf.Sqrt ( area / newArea );
		
		pointScale.x *= areaScale;
		pointScale.y *= areaScale;

		thisMaterial.SetTexture ("_MainTex", _LensMap);
		thisMaterial.SetTexture ("_CorrectTex", _PerspectiveMap);

		thisMaterial.SetVector ("_PointScale", pointScale);

		thisMaterial.SetVector ("_PointTL", pointTL);
		thisMaterial.SetVector ("_PointTR", pointTR);
		thisMaterial.SetVector ("_PointBL", pointBL);
		thisMaterial.SetVector ("_PointBR", pointBR);

		float realPerspectiveX = PerspectiveX;
		if (realPerspectiveX < 0.0f) {
			realPerspectiveX = Mathf.Abs( 1.0f / ( realPerspectiveX - 1.0f ) );
		} else {
			realPerspectiveX = realPerspectiveX + 1.0f;
		}

		float realPerspectiveY = PerspectiveY;
		if (realPerspectiveY < 0.0f) {
			realPerspectiveY = Mathf.Abs( 1.0f / ( realPerspectiveY - 1.0f ) );
		} else {
			realPerspectiveY = realPerspectiveY + 1.0f;
		}

		blitMaterial.SetVector ("_PointTL", pointTL);
		blitMaterial.SetVector ("_PointTR", pointTR);
		blitMaterial.SetVector ("_PointBL", pointBL);
		blitMaterial.SetVector ("_PointBR", pointBR);

		blitMaterial.SetFloat ("_Width", textureToAlign.width);
		blitMaterial.SetFloat ("_Height", textureToAlign.height);

		blitMaterial.SetFloat ("_Lens", LensDistort);
		blitMaterial.SetFloat ("_PerspectiveX", PerspectiveX);
		blitMaterial.SetFloat ("_PerspectiveY", PerspectiveY);

		if (doStuff) {
			ProcessMap( textureToAlign );
			doStuff = false;
		}

		thisMaterial.SetFloat ("_Slider", Slider);
	
	}

	void DoMyWindow ( int windowID ) {
		
		int spacingX = 0;
		int spacingY = 50;
		int spacing2Y = 70;
		
		int offsetX = 10;
		int offsetY = 30;

		GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Alignment Reveal Slider" );
		Slider = GUI.HorizontalSlider( new Rect( offsetX, offsetY + 20, 280, 10 ),Slider,0.0f, 1.0f );
		offsetY += 40;

		GUI.Label (new Rect (offsetX, offsetY, 250, 30), "Preview Map" );
		offsetY += 30;

		if (MGS._DiffuseMapOriginal == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX, offsetY, 130, 30), "Original Diffuse Map")) {
			textureToAlign = MGS._DiffuseMapOriginal;
			doStuff = true;
		}

		if (MGS._DiffuseMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX + 150, offsetY, 130, 30), "Diffuse Map")) {
			textureToAlign = MGS._DiffuseMap;
			doStuff = true;
		}
		offsetY += 40;


		if (MGS._HeightMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX, offsetY, 130, 30), "Height Map")) {
			textureToAlign = MGS._HeightMap;
			doStuff = true;
		}
		offsetY += 40;

		if (MGS._MetallicMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX, offsetY, 130, 30), "Metallic Map")) {
			textureToAlign = MGS._MetallicMap;
			doStuff = true;
		}
		
		if (MGS._SmoothnessMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX + 150, offsetY, 130, 30), "Smoothness Map")) {
			textureToAlign = MGS._SmoothnessMap;
			doStuff = true;
		}
		offsetY += 40;

		if (MGS._EdgeMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX, offsetY, 130, 30), "Edge Map")) {
			textureToAlign = MGS._EdgeMap;
			doStuff = true;
		}
		
		if (MGS._AOMap == null) { GUI.enabled = false; } else { GUI.enabled = true; }
		if (GUI.Button (new Rect (offsetX + 150, offsetY, 130, 30), "AO Map")) {
			textureToAlign = MGS._AOMap;
			doStuff = true;
		}
		offsetY += 40;

		GUI.enabled = true;



		if (GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Lens Distort Correction", LensDistort, LensDistortText, out LensDistort, out LensDistortText, -1.0f, 1.0f)) {
			doStuff = true;
		}
		offsetY += 40;

		if (GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Perspective Correction X", PerspectiveX, PerspectiveXText, out PerspectiveX, out PerspectiveXText, -5.0f, 5.0f)) {
			doStuff = true;
		}
		offsetY += 40;

		if (GuiHelper.Slider (new Rect (offsetX, offsetY, 280, 50), "Perspective Correction Y", PerspectiveY, PerspectiveYText, out PerspectiveY, out PerspectiveYText, -5.0f, 5.0f)) {
			doStuff = true;
		}
		offsetY += 50;

		if( GUI.Button (new Rect (offsetX, offsetY, 130, 30), "Reset Points" ) ){
			pointTL = new Vector2(0.0f,1.0f);
			pointTR = new Vector2(1.0f,1.0f);
			pointBL = new Vector2(0.0f,0.0f);
			pointBR = new Vector2(1.0f,0.0f);
		}

		
		if( GUI.Button (new Rect (offsetX + 150, offsetY, 130, 30), "Set All Maps" ) ){
			StartCoroutine( SetMaps ( ) );
		}
		
		
		GUI.DragWindow();
		
	}
	
	void OnGUI () {
		
		windowRect.width = 300;
		windowRect.height = 430;
		
		windowRect = GUI.Window (21, windowRect, DoMyWindow, "Texture Alignment Adjuster");
	}

	void ProcessMap ( Texture2D textureTarget ){
		int width = textureTarget.width;
		int height = textureTarget.height;

		if (_LensMap == null) {
			_LensMap = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		}
		if (_AlignMap == null) {
			_AlignMap = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		}
		if (_PerspectiveMap == null) {
			_PerspectiveMap = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		}
		
		Graphics.Blit (textureTarget, _LensMap, blitMaterial, 0);
		Graphics.Blit (_LensMap, _AlignMap, blitMaterial, 1);
		Graphics.Blit (_AlignMap, _PerspectiveMap, blitMaterial, 2);
	}

	Texture2D SetMap ( Texture2D textureTarget ) {

		int width = textureTarget.width;
		int height = textureTarget.height;

		CleanupTexture (_LensMap );
		CleanupTexture (_AlignMap );
		CleanupTexture (_PerspectiveMap );

		_LensMap = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		_AlignMap = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		_PerspectiveMap = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

		Graphics.Blit (textureTarget, _LensMap, blitMaterial, 0);
		Graphics.Blit (_LensMap, _AlignMap, blitMaterial, 1);
		Graphics.Blit (_AlignMap, _PerspectiveMap, blitMaterial, 2);

		bool replaceTexture = false;
		if (textureToAlign == textureTarget) {
			replaceTexture = true;
		}

		Destroy (textureTarget);
		textureTarget = null;

		RenderTexture.active = _PerspectiveMap;
		textureTarget = new Texture2D (width, height, TextureFormat.ARGB32, false, true);
		textureTarget.ReadPixels (new Rect (0, 0, width, height), 0, 0);
		textureTarget.Apply ();

		RenderTexture.active = null;

		CleanupTexture (_LensMap );
		CleanupTexture (_AlignMap );
		CleanupTexture (_PerspectiveMap );

		if (replaceTexture) {
			textureToAlign = textureTarget;
		}

		doStuff = true;
		
		return textureTarget;
		
	}

	RenderTexture SetMap ( RenderTexture textureTarget ) {
		
		int width = textureTarget.width;
		int height = textureTarget.height;
		
		CleanupTexture (_LensMap );
		CleanupTexture (_AlignMap );
		CleanupTexture (_PerspectiveMap );
		
		_LensMap = new RenderTexture (width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
		_AlignMap = new RenderTexture (width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
		_PerspectiveMap = new RenderTexture (width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
		
		Graphics.Blit (textureTarget, _LensMap, blitMaterial, 0);
		Graphics.Blit (_LensMap, _AlignMap, blitMaterial, 1);
		Graphics.Blit (_AlignMap, _PerspectiveMap, blitMaterial, 2);

		if (textureTarget != null) {
			textureTarget.Release ();
			textureTarget = null;
		}
		
		Graphics.Blit (_PerspectiveMap, textureTarget);
		
		CleanupTexture (_LensMap );
		CleanupTexture (_AlignMap );
		CleanupTexture (_PerspectiveMap );

		doStuff = true;
		
		return textureTarget;
		
	}

	IEnumerator SetMaps () {

		if (MGS._HeightMap != null) { 
			Debug.Log ("Setting Height");
			MGS._HeightMap = SetMap( MGS._HeightMap ); 
		}

		if (MGS._HDHeightMap != null) { 
			Debug.Log ("Setting HD Height");
			MGS._HDHeightMap = SetMap( MGS._HDHeightMap ); 
		}
		
		yield return new WaitForSeconds(0.1f);

		if (MGS._DiffuseMap != null) { 
			Debug.Log ("Setting Diffuse");
			MGS._DiffuseMap = SetMap( MGS._DiffuseMap ); 
		}
		
		yield return new WaitForSeconds(0.1f);

		if (MGS._DiffuseMapOriginal != null) { 
			Debug.Log ("Setting Diffuse Original");
			MGS._DiffuseMapOriginal = SetMap( MGS._DiffuseMapOriginal ); 
		}
		
		yield return new WaitForSeconds(0.1f);

		if (MGS._NormalMap != null) { 
			Debug.Log ("Setting Normal");
			MGS._NormalMap = SetMap( MGS._NormalMap );
		}
		
		yield return new WaitForSeconds(0.1f);

		if (MGS._MetallicMap != null) { 
			Debug.Log ("Setting Metallic");
			MGS._MetallicMap = SetMap( MGS._MetallicMap );
		}

		yield return new WaitForSeconds(0.1f);
		
		if (MGS._SmoothnessMap != null) { 
			Debug.Log ("Setting Smoothness");
			MGS._SmoothnessMap = SetMap( MGS._SmoothnessMap );
		}
		
		yield return new WaitForSeconds(0.1f);

		if (MGS._EdgeMap != null) { 
			Debug.Log ("Setting Edge");
			MGS._EdgeMap = SetMap( MGS._EdgeMap );
		}
		
		yield return new WaitForSeconds(0.1f);

		if (MGS._AOMap != null) { 
			Debug.Log ("Setting AO");
			MGS._AOMap = SetMap( MGS._AOMap );
		}
		
		yield return new WaitForSeconds(0.1f);

		Resources.UnloadUnusedAssets ();

	}
}
