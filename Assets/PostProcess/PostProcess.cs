using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Post Process/Post Process")]
[RequireComponent (typeof(Camera))]

public class PostProcess : MonoBehaviour {
	
	private RenderTexture mippedSource; //256
	private RenderTexture mippedSource1; //128
	private RenderTexture mippedSource2; //64
	private RenderTexture mippedSource3; //32
	private RenderTexture mippedSource4; //16
	private RenderTexture mippedSource5; //8
	private RenderTexture mippedSource6; //4
	private RenderTexture mippedSource7; //2
	
	private RenderTexture prepassDOFTexture;
	private RenderTexture screenDOFTexture;
	private RenderTexture screenDOFTexture2;
	
	private RenderTexture averageColorTexture;
	private RenderTexture averageColorBlendTexture;
	
	private RenderTexture bloomThresholdTexture;
	private RenderTexture bloomThresholdTextureMip1;
	private RenderTexture bloomThresholdTextureMip2;
	private RenderTexture bloomBlurX;
	private RenderTexture bloomBlurY;
	
	private RenderTexture prepassMBlurTexture1;
	private RenderTexture prepassMBlurTexture2;
	private RenderTexture prepassMBlurTexture3;
	
	[Header("Bloom:")]
	[Range( 0.0f, 3.0f)]
	public float bloomThreshold = 1.0f;
	[Range( 0.0f, 10.0f)]
	public float bloomAmount = 4.0f;
	[Range( 1.0f, 8.0f)]
	public float bloomSpread = 6.0f;
	
	[Header("Lense Flares:")]
	public Texture flareMask;
	[Range( 0.0f, 5.0f)]
	public float lensFlareAmount = 2.0f;
	
	[Header("Lense Dirt:")]
	public Texture lensDirt;
	[Range( 0.0f, 5.0f)]
	public float lensDirtAmount = 2.0f;
	
	[Header("Vignette:")]
	public Texture vignetteTexture;
	[Range( 0.0f, 4.0f)]
	public float vignetteAmount = 1.0f;
	
	[Header("Noise:")]
	public Texture noiseTexture;
	[Range( 0.0f, 1.0f)]
	public float noiseAmount = 0.2f;
	
	[Header("Depth of Field:")]
	public float focalDepth = 2.0f;
	public float DOFMaxDistance = 10.0f;
	[Range( 0.0f, 16.0f)]
	public float DOFMaxBlur = 4.0f;
	public bool AutoFocus = true;
	
	[Header("Motion Blur:")]
	[Range( 0.0f, 10.0f)]
	public float motionBlurAmount = 1.0f;


	
	public Shader PostProcessShader;
	private Material PostProcessMaterial;
	private int ScreenX = 1280, ScreenY = 720;
	
	private Camera thisCamera;
	
	private Matrix4x4 _LAST_VP_MATRIX;
	private Matrix4x4 _CURRENT_VP_MATRIX;	
	
	OpaquePostProcess opaquePostProcess;
	
	enum Pass
	{   
		Compose = 0,
		Mip = 1,
		Threshold = 2,      
		Blur = 3,
		SSFlare = 4,
		PrepassDOF = 5,
		DOF = 6,
		DOFComp = 7,
		GodRays = 8,
		GodRayClip = 9,
		Clear = 10,
		AdaptiveGlare = 11,
		AO = 12,
		AOComp = 13,
		AvgColor = 14,
		MotionBlur = 15
	};
	
	void OnActivate() {
		OnEnable();
	}
	
	void Start() {
		OnEnable();
	}
	
	void OnEnable()
	{		
		
		thisCamera = this.GetComponent<Camera>();
		
		//thisCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
		
		//Create Post Process Material
		if( PostProcessShader == null ){
			PostProcessShader = Shader.Find ("Hidden/PostProcess");
		}
		if (PostProcessShader == null)
			Debug.Log ("#ERROR# Hidden/PostProcess Shader not found");
		PostProcessMaterial = new Material (PostProcessShader);
		PostProcessMaterial.hideFlags = HideFlags.HideAndDontSave;
		
		
		//Apply Default Textures
		if( vignetteTexture == null ){
			vignetteTexture = ( Texture2D )Resources.Load( "vignette", typeof( Texture2D ) );
		}
		
		if( noiseTexture == null ){
			noiseTexture = ( Texture2D )Resources.Load( "noise", typeof( Texture2D ) );
		}
		
		if( lensDirt == null ){
			lensDirt = ( Texture2D )Resources.Load( "lensDirt_01", typeof( Texture2D ) );
		}
		
		if( flareMask == null ){
			flareMask = ( Texture2D )Resources.Load( "flareMask", typeof( Texture2D ) );
		}
		
		Texture2D RoughnessNoiseTexture = ( Texture2D )Resources.Load( "RoughnessNoiseTexture", typeof( Texture2D ) );
		Shader.SetGlobalTexture( "_RoughnessNoiseTexture", RoughnessNoiseTexture );
		
		_LAST_VP_MATRIX = thisCamera.projectionMatrix * thisCamera.worldToCameraMatrix;
		
		averageColorBlendTexture = new RenderTexture( 1, 1, 0, RenderTextureFormat.ARGBFloat );
		
		opaquePostProcess = this.gameObject.GetComponent<OpaquePostProcess>();
		
	}
	
	
	void CleanUpTextures()
	{
		
		if (mippedSource) {
			RenderTexture.ReleaseTemporary(mippedSource);
			mippedSource = null;
		}
		
		
		if (averageColorTexture) {
			RenderTexture.ReleaseTemporary(averageColorTexture);
			averageColorTexture = null;
		}
		
		
		if (averageColorTexture) {
			RenderTexture.ReleaseTemporary(averageColorTexture);
			averageColorTexture = null;
		}
		
		
		if (bloomThresholdTexture) {
			RenderTexture.ReleaseTemporary(bloomThresholdTexture);
			bloomThresholdTexture = null;
		}
		
		
		if (bloomThresholdTextureMip1) {
			RenderTexture.ReleaseTemporary(bloomThresholdTextureMip1);
			bloomThresholdTextureMip1 = null;
		}
		
		
		if (bloomThresholdTextureMip2) {
			RenderTexture.ReleaseTemporary(bloomThresholdTextureMip2);
			bloomThresholdTextureMip2 = null;
		}
		
		
		if (bloomBlurX) {
			RenderTexture.ReleaseTemporary(bloomBlurX);
			bloomBlurX = null;
		}
		
		
		if (bloomBlurY) {
			RenderTexture.ReleaseTemporary(bloomBlurY);
			bloomBlurY = null;
		}
		
		
		if (prepassDOFTexture) {
			RenderTexture.ReleaseTemporary(prepassDOFTexture);
			prepassDOFTexture = null;
		}
		
		
		if (screenDOFTexture) {
			RenderTexture.ReleaseTemporary(screenDOFTexture);
			screenDOFTexture = null;
		}
		
		if (screenDOFTexture2) {
			RenderTexture.ReleaseTemporary(screenDOFTexture2);
			screenDOFTexture2 = null;
		}
		
		if (prepassMBlurTexture1) {
			RenderTexture.ReleaseTemporary(prepassMBlurTexture1);
			prepassMBlurTexture1 = null;
		}
		
		if (prepassMBlurTexture2) {
			RenderTexture.ReleaseTemporary(prepassMBlurTexture2);
			prepassMBlurTexture2 = null;
		}
		
		if (prepassMBlurTexture3) {
			RenderTexture.ReleaseTemporary(prepassMBlurTexture3);
			prepassMBlurTexture3 = null;
		}
		
		
	}
	
	void OnDisable () {
		CleanUpTextures();
		if (averageColorBlendTexture) {
			averageColorBlendTexture.Release();
			averageColorBlendTexture = null;
		}
		
	}
	
	
	void Update () {
		
	}
	
	
	public void setDofParms( Vector3 DOFInfo ){
		//Debug.Log ("Receiving DOF Parms");
		DOFMaxDistance = DOFInfo.x;
		DOFMaxBlur = DOFInfo.y;
		focalDepth = DOFInfo.z;
	}
	
	public void setDofParms( float DOFMaxDistanceInput, float DOFMaxBlurInput, float focalDepthInput ){
		//Debug.Log ("Receiving DOF Parms");
		DOFMaxDistance = DOFMaxDistanceInput;
		DOFMaxBlur = DOFMaxBlurInput;
		focalDepth = focalDepthInput;
	}
	
	
	void OnPostRender (){
		
	}
	
	void OnRenderImage (RenderTexture source, RenderTexture destination){
		
		ScreenX = source.width;
		ScreenY = source.height;
		
		Vector2 ScreenSize = new Vector2( source.width, source.height );
		Vector2 ScreenSizeHalf = new Vector2( source.width/2, source.height/2 );
		Vector2 ScreenSizeQuarter = new Vector2( source.width/4, source.height/4);
		Vector2 ScreenSizeEighth = new Vector2( source.width/8, source.height/8 );
		
		PostProcessMaterial.SetFloat ( "_BlurSpread", 1.0f );
		
		//==============================================================//
		// 		Get mips of screen down to solid color for 1080p		//
		//==============================================================//
		
		mippedSource = RenderTexture.GetTemporary( 256, 256, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit( source, mippedSource );
		
		PostProcessMaterial.SetVector ( "_OneOverScreenSize", new Vector4( 1.0f / 256.0f, 1.0f / 256.0f, 0, 0 ) );
		mippedSource1 = RenderTexture.GetTemporary( 128, 128, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit( mippedSource, mippedSource1, PostProcessMaterial, (int)Pass.Mip );
		RenderTexture.ReleaseTemporary( mippedSource );
		mippedSource = null;
		
		PostProcessMaterial.SetVector ( "_OneOverScreenSize", new Vector4( 1.0f / 128.0f, 1.0f / 128.0f, 0, 0 ) );
		mippedSource2 = RenderTexture.GetTemporary( 64, 64, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit( mippedSource1, mippedSource2, PostProcessMaterial, (int)Pass.Mip  );
		RenderTexture.ReleaseTemporary( mippedSource1 );
		mippedSource1 = null;
		
		PostProcessMaterial.SetVector ( "_OneOverScreenSize", new Vector4( 1.0f / 64.0f, 1.0f / 64.0f, 0, 0 ) );
		mippedSource3 = RenderTexture.GetTemporary( 32, 32, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit( mippedSource2, mippedSource3, PostProcessMaterial, (int)Pass.Mip  );
		RenderTexture.ReleaseTemporary( mippedSource2 );
		mippedSource2 = null;
		
		PostProcessMaterial.SetVector ( "_OneOverScreenSize", new Vector4( 1.0f / 32.0f, 1.0f / 32.0f, 0, 0 ) );
		mippedSource4 = RenderTexture.GetTemporary( 16, 16, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit( mippedSource3, mippedSource4, PostProcessMaterial, (int)Pass.Mip  );
		RenderTexture.ReleaseTemporary( mippedSource3 );
		mippedSource3 = null;
		
		PostProcessMaterial.SetVector ( "_OneOverScreenSize", new Vector4( 1.0f / 16.0f, 1.0f / 16.0f, 0, 0 ) );
		mippedSource5 = RenderTexture.GetTemporary( 8, 8, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit( mippedSource4, mippedSource5, PostProcessMaterial, (int)Pass.Mip  );
		RenderTexture.ReleaseTemporary( mippedSource4 );
		mippedSource4 = null;
		
		PostProcessMaterial.SetVector ( "_OneOverScreenSize", new Vector4( 1.0f / 8.0f, 1.0f / 8.0f, 0, 0 ) );
		mippedSource6 = RenderTexture.GetTemporary( 4, 4, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit( mippedSource5, mippedSource6, PostProcessMaterial, (int)Pass.Mip  );
		RenderTexture.ReleaseTemporary( mippedSource5 );
		mippedSource5 = null;
		
		PostProcessMaterial.SetVector ( "_OneOverScreenSize", new Vector4( 1.0f / 4.0f, 1.0f / 4.0f, 0, 0 ) );
		mippedSource7 = RenderTexture.GetTemporary( 2, 2, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit( mippedSource6, mippedSource7, PostProcessMaterial, (int)Pass.Mip  );
		RenderTexture.ReleaseTemporary( mippedSource6 );
		mippedSource6 = null;
		
		PostProcessMaterial.SetVector ( "_OneOverScreenSize", new Vector4( 1.0f / 2.0f, 1.0f / 2.0f, 0, 0 ) );
		averageColorTexture = RenderTexture.GetTemporary( 1, 1, 0, RenderTextureFormat.ARGBFloat );
		Graphics.Blit( mippedSource7, averageColorTexture, PostProcessMaterial, (int)Pass.Mip  );
		RenderTexture.ReleaseTemporary( mippedSource7 );
		mippedSource7 = null;
		
		
		if( Application.isPlaying ) {
			// blend between the old average color and the new average color
			PostProcessMaterial.SetFloat ( "_AGBlendSpeed", 0.03f );
			Graphics.Blit( averageColorTexture, averageColorBlendTexture, PostProcessMaterial, (int)Pass.AdaptiveGlare );
		}else{
			Graphics.Blit( averageColorTexture, averageColorBlendTexture );
		}
		
		PostProcessMaterial.SetTexture ( "_ScreenMip11", averageColorBlendTexture );
		
		
		//==================================================================//
		// 		Calculate threshold and bloom based on screen color			//
		//==================================================================//	
		
		PostProcessMaterial.SetFloat ( "_BloomThreshold", bloomThreshold );
		PostProcessMaterial.SetFloat ( "_ScreenX", ScreenSizeHalf.x );
		PostProcessMaterial.SetFloat ( "_ScreenY", ScreenSizeHalf.y );
		
		bloomThresholdTexture = RenderTexture.GetTemporary( ScreenX/2, ScreenY/2, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit(source, bloomThresholdTexture, PostProcessMaterial, (int)Pass.Threshold );
		
		PostProcessMaterial.SetFloat ( "_ScreenX", ScreenSizeQuarter.x );
		PostProcessMaterial.SetFloat ( "_ScreenY", ScreenSizeQuarter.y );
		
		PostProcessMaterial.SetFloat ( "_BlurSpread", 1.0f ); 
		
		bloomThresholdTextureMip1 = RenderTexture.GetTemporary( ScreenX/4, ScreenY/4, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit(bloomThresholdTexture, bloomThresholdTextureMip1, PostProcessMaterial, (int)Pass.Mip );
		
		PostProcessMaterial.SetFloat ( "_ScreenX", ScreenSizeEighth.x );
		PostProcessMaterial.SetFloat ( "_ScreenY", ScreenSizeEighth.y );
		
		bloomThresholdTextureMip2 = RenderTexture.GetTemporary( ScreenX/8, ScreenY/8, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit(bloomThresholdTextureMip1, bloomThresholdTextureMip2, PostProcessMaterial, (int)Pass.Mip );
		
		PostProcessMaterial.SetVector ( "_BlurDir", new Vector4(1.0f, 0.0f, 0.0f, 0.0f ) );
		
		bloomBlurX = RenderTexture.GetTemporary( (int)ScreenSizeEighth.x, (int)ScreenSizeEighth.y, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit(bloomThresholdTextureMip1, bloomBlurX, PostProcessMaterial, (int)Pass.Blur );
		
		PostProcessMaterial.SetVector ( "_BlurDir", new Vector4(0.0f, 1.0f, 0.0f, 0.0f ) );
		
		bloomBlurY = RenderTexture.GetTemporary( (int)ScreenSizeEighth.x, (int)ScreenSizeEighth.y, 0, RenderTextureFormat.DefaultHDR );
		Graphics.Blit(bloomBlurX, bloomBlurY, PostProcessMaterial, (int)Pass.Blur );
		
		PostProcessMaterial.SetFloat ( "_BlurSpread", bloomSpread );
		
		PostProcessMaterial.SetVector ( "_BlurDir", new Vector4(1.0f, 0.0f, 0.0f, 0.0f ) );
		Graphics.Blit(bloomBlurY, bloomBlurX, PostProcessMaterial, (int)Pass.Blur );
		
		PostProcessMaterial.SetVector ( "_BlurDir", new Vector4(0.0f, 1.0f, 0.0f, 0.0f ) );
		Graphics.Blit(bloomBlurX, bloomBlurY, PostProcessMaterial, (int)Pass.Blur );
		
		// save a masked version of the bloom for screen space lense flares
		PostProcessMaterial.SetTexture ( "_FlareMaskTex", flareMask );
		Graphics.Blit(bloomBlurY, bloomBlurX, PostProcessMaterial, (int)Pass.SSFlare );

		
		//==========================================================================================//
		// 									Comp it all together									//
		//==========================================================================================//
		
		PostProcessMaterial.SetTexture ( "_VignetteTex", vignetteTexture );
		PostProcessMaterial.SetFloat ( "_VignetteAmount", vignetteAmount );
		
		PostProcessMaterial.SetTexture ( "_NoiseTex", noiseTexture );
		PostProcessMaterial.SetFloat ("_NoiseAmount", noiseAmount );
		
		PostProcessMaterial.SetTexture ( "_BloomTex", bloomBlurY );
		PostProcessMaterial.SetFloat ("_BloomAmount", bloomAmount );
		
		PostProcessMaterial.SetTexture ( "_FlareTex", bloomBlurX );
		PostProcessMaterial.SetFloat ( "_LensFlareAmount", lensFlareAmount );
		
		PostProcessMaterial.SetTexture ( "_LensDirtTex", lensDirt );
		PostProcessMaterial.SetFloat ("_LensDirtAmount", lensDirtAmount );
		
		PostProcessMaterial.SetTexture ( "_ScreenMip11", averageColorBlendTexture );
		
		Graphics.Blit( source, destination, PostProcessMaterial, (int)Pass.Compose );
		
		//Tests
		//Graphics.Blit( source, destination );
		//Graphics.Blit( source, destination, PostProcessMaterial, (int)Pass.Compose );
		//Graphics.Blit( averageColorBlendTexture, destination );
		//Graphics.Blit( prepassDOFTexture, destination );
		//Graphics.Blit( screenDOFTexture2, destination );
		//Graphics.Blit ( prepassMBlurTexture3, destination );
		//Graphics.Blit ( bloomThresholdTexture, destination );
		
		CleanUpTextures();
		
	}
	
}
