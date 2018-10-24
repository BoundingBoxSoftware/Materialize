using UnityEngine;
using System.Collections;

//=============================================================================
public class SoundViewer : MonoBehaviour
{
    // Visualization rectangle, in normalized coordinates (0-1)
    public Rect viewRect = new Rect(0.25f, 0.25f, 0.5f, 0.5f);
	
	[System.Serializable]
	public class SoundViewerConfig {
		public bool previewSounds = true;
	
	    // An AudioSource object so the music can be played
	    public AudioSource aSource;
	    // Speed in which the dots fall (in view rect height per second)
	    public float fallSpeed = 1f;
	    // Number of point viewers (power of two)
	    public int samplesSize = 128;
	    
	    // Customize viewing colors
	    public Color bgColor      = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	    public Color borderColor  = Color.black;
	    public Color samplesColor = Color.blue;
	    public Color markersColor = Color.black;
	    
	    public FFTWindow fftWindow = FFTWindow.BlackmanHarris;
	}
	
	public SoundViewerConfig configs;
	
    // A float array that stores the audio samples
    private float[] samples;
	
	// FTTWindow view
    private Vector2[] vertices;		// Vertices of the actual position of the samples
    private Vector2[] viewers;		// Vertices with With falling inertia
    
    // Clip view
    private Vector2[] verticesLeft;	// Vertices of the actual position of the samples
    private Vector2[] verticesRight;
    private Vector2[] playingBar;	// Vertical bar showing the play location of the current clip
    
   	//=============================================================================
   	// Returns an actual screen visualization rect, from the normalized rect area in this.viewRect
   	// The total view rect is divided in viewParts parts, and this function returns the part pertaining to partIndex (zero-based)
   	Rect __GetRect(float partIndex, float parts, float myPart) {
		Rect view = this.viewRect;
   		int partH = (int)(view.height * Screen.height * (1.0f / parts));
   		int y     = (int)(view.y      * Screen.height + partH * partIndex);
   		return new Rect((int)(view.x * Screen.width), y, (int)(view.width * Screen.width), (int)(partH * myPart));
   	}
   	Rect GetLeftRect(bool stereo) { return this.__GetRect(1.1f, 3.1f, stereo ? 1.0f : 2.0f); }
   	Rect GetRightRect()           { return this.__GetRect(2.1f, 3.1f, 1.0f); }
   	Rect GetFFTRect()             { return this.__GetRect(0.0f, 3.1f, 1.0f); }

	//=============================================================================
	// Plays the audio clip one time, then disable the previewer when done playing
	public void PreviewClip(AudioClip clip) {
		this.StopCoroutine("__PreviewClip");
		this.configs.aSource.Stop();
		this.configs.aSource.loop = false;
		this.configs.aSource.clip = clip;
		this.StartCoroutine("__PreviewClip");
	}
	IEnumerator __PreviewClip() {
		while(!this.configs.aSource.clip.isReadyToPlay) {
			yield return null;
		}
		this.ResetClipView(this.configs.aSource.clip);
		this.configs.aSource.Play();
	}
	
	//=============================================================================
	// Use this instead of setting .enable directly, to stop any playing samples
	public void Disable() {
		if (this.configs.aSource && this.configs.aSource.clip != null) {
			this.configs.aSource.Stop();
		}
		this.enabled = false;
	}

	//=============================================================================
    public void Start() {
    	if (!this.configs.previewSounds) return;
		this.StartCoroutine(this.__Start());
	}
    private IEnumerator __Start()
    {
        if (!this.configs.aSource) this.configs.aSource = this.GetComponent<AudioSource>();
        if (!this.configs.aSource) this.configs.aSource = this.GetComponentInChildren<AudioSource>();
        if (!this.configs.aSource) this.configs.aSource = GameObject.FindObjectOfType(typeof(AudioSource)) as AudioSource;
		if (!this.configs.aSource) {
			Debug.LogError("No Audio Source in the scene found to use for Sound Previews");
			yield break;
		}
        
        if (this.configs.aSource.clip && this.enabled) {
        	this.ResetClipView(this.configs.aSource.clip);
        }
        
        while(true)
		{
			yield return new WaitForEndOfFrame();
			if (!enabled) continue;
			
			if (this.configs.aSource.isPlaying) {
		        GLUtils.RenderLines   (this.vertices, this.configs.samplesColor);
		        GLUtils.RenderVertices(this.viewers,  this.configs.markersColor);
		        GLUtils.RenderRect (this.GetFFTRect(), this.configs.bgColor, this.configs.borderColor);
			}
	        bool stereo = this.verticesRight != null;
	        GLUtils.RenderLines(this.verticesLeft, this.configs.samplesColor);
	        GLUtils.RenderRect (this.GetLeftRect(stereo), this.configs.bgColor, this.configs.borderColor);
	        if (stereo) {
		        GLUtils.RenderLines(this.verticesRight, this.configs.samplesColor);
		        GLUtils.RenderRect (this.GetRightRect(), this.configs.bgColor, this.configs.borderColor);
	        }
	        GLUtils.RenderLines(this.playingBar, this.configs.markersColor);
        }
    }

	//=============================================================================
	// Calculates the 'clip view' for the given AudioClip
	// This is an intensive operation, but only gets done once per clip
	//
	// TODO_WISH: Save this to a texture instead of vertices, for faster and drawing, and with more precision
	void ResetClipView(AudioClip clip)
	{
    	int size = Mathf.NextPowerOfTwo(this.configs.samplesSize);
    	if (size < 64) size = 64;
    	
    	this.samples    = new float[size];
        this.vertices   = new Vector2[size];
        this.viewers    = new Vector2[size];
        this.playingBar = new Vector2[4];
        
        bool stereo = clip.channels > 1;
    	Rect screenViewRect = this.GetLeftRect(stereo);
    	float viewX = screenViewRect.x;
    	float viewY = screenViewRect.y;
    	float viewW = screenViewRect.width;
    	float viewH = screenViewRect.height;

    	// A sample rate of 100Hz should give us close enough results while keeping acceptable performance on long samples
    	// Ideally we should render this info baked in a texture for this, but that'd be too much trouble to go to using unity
    	int sampleRate = 1000;
    	if (clip.length > 5)     sampleRate = 300;
    	if (clip.length > 30)    sampleRate = 100;
    	if (clip.length < 0.5f)  sampleRate = 3000;
    	if (clip.length < 0.05f) sampleRate = clip.frequency;
    	int sampleSize = (int) (sampleRate * clip.length);
		
		// Read samples info
		float[] clipSamples = new float[clip.samples * clip.channels];
		clip.GetData(clipSamples, 0);
		
		this.verticesLeft = new Vector2[sampleSize];
		this.verticesRight = null;
		if (stereo) {
			this.verticesRight = new Vector2[sampleSize];
		}
		
		// Plot wave form
        for (int i = 0; i < sampleSize; i++)
        {
        	int sampleIdx = (int)(clip.samples / (float)sampleSize * i * clip.channels);
            float x = viewW / sampleSize * i + viewX;
            
        	float normalVal = clipSamples[sampleIdx] * 0.5f + 0.5f;
            float y = normalVal * viewH + viewY;
            this.verticesLeft[i] = new Vector2(x, y);
            
            if (stereo) {
	        	normalVal = clipSamples[sampleIdx+1] * 0.5f + 0.5f;
	            float y2 = normalVal * viewH + viewY + viewH;
	            this.verticesRight[i] = new Vector2(x, y2);
            }
    	}
    	
	}

	//=============================================================================
	// Updates the frequency profile for the current playing frame
    void Update()
    {
		if (!enabled || !this.configs.previewSounds) return;
		
		AudioSource aSource = this.configs.aSource;
		
    	Rect screenViewRect = this.GetFFTRect();	// Get lower-part rectangle
    	float viewX = screenViewRect.x;
    	float viewY = screenViewRect.y + 5;
    	float viewW = screenViewRect.width;
    	float viewH = screenViewRect.height - 10;
    	
		if (aSource.isPlaying)
		{
	        // Obtain the samples from the frequency bands of the attached AudioSource
	        aSource.GetSpectrumData(this.samples, 0, this.configs.fftWindow);
			
	        Vector2 gravity = new Vector3(0.0f, this.configs.fallSpeed * viewH);
	        for (int i = 0; i < this.samples.Length; i++)
	        {
	            // Change this viewers position Y according to the current sample (0 - 50)
	            float val = Mathf.Clamp(samples[i] * (50 + i * i), 0, 25) * 2;
	            if (val < 0.5f) val = 0;   // Avoids flickering at the bottom bar
	
	            float x = viewW / this.samples.Length * (i + 0.5f) + viewX;
	            float y = val / 50.0f * viewH + viewY;
	            Vector2 newPos = new Vector3(x, y);
	
	            this.vertices[i] = newPos;
	            
	            if (y >= this.viewers[i].y) {
	                this.viewers[i] = newPos;
	            } else {
	                this.viewers[i] -= gravity * Time.deltaTime;
	                this.viewers[i].y = Mathf.Max(viewY, this.viewers[i].y);
	            }
	        }
		}
		
    	Rect playRect = this.GetLeftRect(false);
    	viewY = playRect.y + 1;
    	viewH = playRect.height - 1;
    	
    	// Update the position of the 'play bar'
    	float t = 0;
    	if (aSource.isPlaying) {
    		t = aSource.time / aSource.clip.length;
    	}
    	this.playingBar[0] = new Vector2((int)(viewX + t * viewW), (int)(viewY));
    	this.playingBar[1] = new Vector2((int)(viewX + t * viewW), (int)(viewY + viewH));
    	this.playingBar[2] = new Vector2(this.playingBar[0].x + 1, this.playingBar[0].y);	// two pixels
    	this.playingBar[3] = new Vector2(this.playingBar[1].x + 1, this.playingBar[1].y);
		
		// If the user clicks over the play bar, jump to that position
		if (Input.GetMouseButtonDown(0)) {
			Vector2 mousePos = Input.mousePosition;
			if (mousePos.x > viewX && mousePos.x < (viewX + viewW) &&
			    mousePos.y > viewY && mousePos.y < (viewY + viewH))
			{
				// Note: time wouldn't replect actual time on compressed audio - timeSamples is better
			    aSource.timeSamples = (int) ((mousePos.x - viewX) / viewW * aSource.clip.samples);
			}
		}
		
    }
    
}

