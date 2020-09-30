using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Post Process/Opaque Post Process")]
[RequireComponent (typeof(Camera))]
public class OpaquePostProcess : MonoBehaviour {
		
	// Tomporal Anti Aliasing
	[Header("Tomporal Anti Aliasing:")]
	public bool useTAA = true;
	
	Material taaMaterial;
	
	RenderTexture _CompedFrame;
	
	RenderTexture _MotionVector;
	RenderTexture _AccumulatedFrames;
	RenderTexture _AccumulatedFramesAlt;
	
	Camera thisCamera;
	
	HaltonSequence positionsequence = new HaltonSequence();
	int hspos = 0;
	
	Matrix4x4 _LAST_VP_MATRIX;
	Matrix4x4 _CURRENT_VP_MATRIX;
	
	Matrix4x4 _LAST_BG_VP_MATRIX;
	
	Matrix4x4 _CURRENT_P_MATRIX;
	
	Matrix4x4 _CURRENT_V_MATRIX;
	Matrix4x4 _CURRENT_VP_INVERSE_MATRIX;

	Material PostProcessMaterial;

	float AutoFocalDepth = 0.0f;

	int screenX = 0;
	int screenY = 0;

	PostProcess thisPP;

    public bool overideBlend = false; 
	
	[Header("Debug Stuffs:")]
	//public bool showBlendMask = false;
	public bool showMotion = false;
	public bool showDofPrepass = false;
	public bool passThrough = false;

	bool initialized = false;
	
	// Use this for initialization
	void Start () {
		
		thisCamera = this.GetComponent<Camera>();
		thisPP = this.GetComponent<PostProcess>();
		
		taaMaterial = new Material( Shader.Find ("Hidden/TAA") );
		taaMaterial.hideFlags = HideFlags.HideAndDontSave;

		PostProcessMaterial = new Material( Shader.Find ("Hidden/PostProcess") );
		PostProcessMaterial.hideFlags = HideFlags.HideAndDontSave;

		ResetBuffers ();
		
		positionsequence.Reset();

		initialized = true;
	}
	
	public RenderTexture GetMotionTexture() {
		return _MotionVector;
	}

	void ResetBuffers(){
		if (_MotionVector) {
			_MotionVector.Release ();
		}
		if (_AccumulatedFrames) {
			_AccumulatedFrames.Release ();
		}
			
		screenX = thisCamera.pixelWidth;
		screenY = thisCamera.pixelHeight;

		_MotionVector = new RenderTexture( screenX, screenY, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Default );
		_AccumulatedFrames = new RenderTexture( screenX, screenY, 0, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default );

		taaMaterial.SetTexture ("_MotionVector", _MotionVector );
		taaMaterial.SetTexture ("_AccumulatedFrames", _AccumulatedFrames );
	}

	#if UNITY_EDITOR
	void OnApplicationFocus(bool focusStatus) {
		if (focusStatus && initialized) {
			ResetBuffers();
		}
	}
	#endif

	void LateUpdate() {

        thisCamera.depthTextureMode = DepthTextureMode.Depth;

        if ( thisCamera.pixelWidth != screenX || thisCamera.pixelHeight != screenY ) {
			Debug.Log ( "Resizing Screen" );
			ResetBuffers ();
		}

		// auto focus

		RaycastHit hit;
		Ray afRay;
		int hits = 0;
		int taps = 10;
		float invTaps = 1.0f / taps;
		float newAFDepth = 0;
		for( int i = 0; i <= taps; i++ ){
			for( int j = 0; j <= taps; j++ ){
				Vector3 screenPos = Vector3.zero;
				screenPos.x = ( screenX / taps ) * i;
				screenPos.y = ( screenY / taps ) * j;
				afRay = thisCamera.ScreenPointToRay ( screenPos );
				if( Physics.Raycast( afRay, out hit, 100 ) ){
					newAFDepth += Vector3.Magnitude( Vector3.Scale( thisCamera.transform.position - hit.point, thisCamera.transform.forward ) );
					hits += 1;
				}
			}
		}

		if (hits > 0) {
			AutoFocalDepth = newAFDepth / hits;
		}

		
		// Save the current camera matrixes
		_CURRENT_P_MATRIX = thisCamera.projectionMatrix;
		_CURRENT_V_MATRIX = thisCamera.worldToCameraMatrix;
		_CURRENT_VP_MATRIX = thisCamera.projectionMatrix * thisCamera.worldToCameraMatrix;
		_CURRENT_VP_INVERSE_MATRIX = _CURRENT_VP_MATRIX.inverse;
		
		// Set the view projection matrix for this camera for motion blur
		Shader.SetGlobalMatrix( "_CURRENT_VP_MATRIX", _CURRENT_VP_MATRIX );
			
		if( Application.isPlaying && useTAA ){
			
			// Jitter the current projection matrix with a halton sequence
			// This needs to be done on the camera and not the vertex shader because the shadows will flicker
			positionsequence.Increment();
			hspos += 1;
			
			Matrix4x4 JITTER_MATRIX_P = thisCamera.projectionMatrix;
            //JITTER_MATRIX_P.m02 += ( positionsequence.m_CurrentPos.x * 2.0f - 1.0f ) / thisCamera.pixelWidth;
            //JITTER_MATRIX_P.m12 += ( positionsequence.m_CurrentPos.y * 2.0f - 1.0f ) / thisCamera.pixelHeight;

            JITTER_MATRIX_P[0, 2] += (positionsequence.m_CurrentPos.x * 2.0f - 1.0f) / thisCamera.pixelWidth;
			JITTER_MATRIX_P[1, 2] += (positionsequence.m_CurrentPos.y * 2.0f - 1.0f) / thisCamera.pixelHeight;

			thisCamera.projectionMatrix = JITTER_MATRIX_P;

			if( hspos > 16 ){ 
				positionsequence.Reset(); 
				hspos = 0;
			}

		}
		
	}
	
	void OnPostRender () {
	
		// Reset the camera's projection matrix
		thisCamera.ResetProjectionMatrix();

		taaMaterial.SetMatrix( "_LAST_VP_MATRIX", _CURRENT_VP_MATRIX );
		Shader.SetGlobalMatrix( "_LAST_VP_MATRIX", _CURRENT_VP_MATRIX );
		
	}
		
	
	[ImageEffectOpaque]
	void OnRenderImage (RenderTexture source, RenderTexture destination){		
		
		//screenX = thisCamera.pixelWidth;
		//screenY = thisCamera.pixelHeight;

		//==============================================//
		// 				Temporal Anti Aliasing			//
		//==============================================//
		
		taaMaterial.SetVector ("_RenderBufferSize", new Vector4 (screenX, screenY, 0, 0));
		taaMaterial.SetMatrix ("_INVERSE_VP_MATRIX", _CURRENT_VP_INVERSE_MATRIX);
		
        if (useTAA)
        {
            // Create motion vector buffer
            Graphics.Blit (source, _MotionVector, taaMaterial, 0);
        }

		float FocalDepth = 10.0f;

		if (thisPP.AutoFocus) {
			FocalDepth = AutoFocalDepth;
		} else {
			FocalDepth = thisPP.focalDepth;
		}
		float DOFMaxDistance = thisPP.DOFMaxDistance;
		float DOFMaxBlur = thisPP.DOFMaxBlur;

		taaMaterial.SetFloat ("_FocalDepth", FocalDepth);
		taaMaterial.SetFloat ("_DOFMaxDistance", DOFMaxDistance);
		taaMaterial.SetFloat ("_DOFMaxBlur", DOFMaxBlur);

		RenderTexture prepassDOFTexture = RenderTexture.GetTemporary (screenX, screenY, 0, RenderTextureFormat.RGHalf);
		Graphics.Blit (source, prepassDOFTexture, taaMaterial, 4);
		taaMaterial.SetTexture ("_DofPrepass", prepassDOFTexture);

		PostProcessMaterial.SetTexture ("_PrepassFocus", prepassDOFTexture);
		
		RenderTexture screenDOFTexture = RenderTexture.GetTemporary (screenX, screenY, 0, RenderTextureFormat.DefaultHDR);
		RenderTexture screenDOFTexture2 = RenderTexture.GetTemporary (screenX, screenY, 0, RenderTextureFormat.DefaultHDR);
		
		PostProcessMaterial.SetFloat ("_DOFMaxMultiplier", 16.0f);
		PostProcessMaterial.SetFloat ("_FocalDepth", FocalDepth);
		PostProcessMaterial.SetFloat ("_DOFMaxDistance", DOFMaxDistance);
		PostProcessMaterial.SetFloat ("_DOFMaxBlur", DOFMaxBlur);
		
		PostProcessMaterial.SetFloat ("_ScreenX", screenX);
		PostProcessMaterial.SetFloat ("_ScreenY", screenY);

		if (DOFMaxBlur > 0.001f) {
			if (DOFMaxBlur > 8.001f) {
				PostProcessMaterial.SetFloat ("_DOFMultiplier", 16.0f);
				Graphics.Blit (source, screenDOFTexture, PostProcessMaterial, 6);
			} else {
				Graphics.Blit (source, screenDOFTexture);
			}

			if (DOFMaxBlur > 4.001f) {
				PostProcessMaterial.SetFloat ("_DOFMultiplier", 8.0f);
				Graphics.Blit (screenDOFTexture, screenDOFTexture2, PostProcessMaterial, 6);
			} else {
				Graphics.Blit (screenDOFTexture, screenDOFTexture2);
			}

			if (DOFMaxBlur > 2.001f) {
				PostProcessMaterial.SetFloat ("_DOFMultiplier", 4.0f);
				Graphics.Blit (screenDOFTexture2, screenDOFTexture, PostProcessMaterial, 6);
			} else {
				Graphics.Blit (screenDOFTexture2, screenDOFTexture);
			}

			if (DOFMaxBlur > 1.001f) {
				PostProcessMaterial.SetFloat ("_DOFMultiplier", 2.0f);
				Graphics.Blit (screenDOFTexture, screenDOFTexture2, PostProcessMaterial, 6);
			} else {
				Graphics.Blit (screenDOFTexture, screenDOFTexture2);
			}
				
			if (DOFMaxBlur > 0.001f) {
				PostProcessMaterial.SetFloat ("_DOFMultiplier", 1.0f);
				Graphics.Blit (screenDOFTexture2, screenDOFTexture, PostProcessMaterial, 6);
			} else {
				Graphics.Blit (screenDOFTexture2, screenDOFTexture);
			}
		} else {
			Graphics.Blit (source, screenDOFTexture);
		}

		if( useTAA ){

			if( Application.isPlaying ){

                if (overideBlend)
                {
                    Shader.SetGlobalFloat("_OverideBlend", 1.0f);
                }
                else
                {
                    Shader.SetGlobalFloat("_OverideBlend", 0.0f);
                }
				
				//Blend the accumulation buffer with the new buffer
				_AccumulatedFramesAlt = RenderTexture.GetTemporary( screenX, screenY, 0, RenderTextureFormat.DefaultHDR );
				Graphics.Blit ( screenDOFTexture, _AccumulatedFramesAlt, taaMaterial, 2 );	
				Graphics.Blit ( _AccumulatedFramesAlt, _AccumulatedFrames );
				RenderTexture.ReleaseTemporary( _AccumulatedFramesAlt );
				_AccumulatedFramesAlt = null;

                Graphics.Blit ( _AccumulatedFrames, destination );
			
			}else{	
				Graphics.Blit ( source, destination );
			}

		}else{
            Graphics.Blit ( screenDOFTexture, destination );
		}

		RenderTexture.ReleaseTemporary( screenDOFTexture );
		screenDOFTexture = null;

		RenderTexture.ReleaseTemporary( screenDOFTexture2 );
		screenDOFTexture2 = null;

		RenderTexture.ReleaseTemporary( prepassDOFTexture );
		prepassDOFTexture = null;

		if( showMotion ){
			Graphics.Blit ( _MotionVector, destination );
		}
		
		if( passThrough ){
			Graphics.Blit ( source, destination );
		}
			
	}

}