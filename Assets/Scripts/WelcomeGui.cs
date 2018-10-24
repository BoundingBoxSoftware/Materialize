using UnityEngine;
using System.Collections;

public class WelcomeGui : MonoBehaviour {

	public bool skipWelcomeScreen;

	public Texture2D logo;
	public Texture2D background;
	public Cubemap startCubeMap;

	public GameObject MainGuiObject;
	public GameObject testObject;
	public GameObject SettingsGuiObject;
	public GameObject ControlsGuiObject;
	public GameObject CommandListExecutorObject;


	float backgroundFade = 1.0f;
	float logoFade = 1.0f;

	// Use this for initialization
	void Start () {

		Application.runInBackground = true;

		float backgroundFade = 1.0f;
		float logoFade = 0.0f;

		if ( skipWelcomeScreen || Application.isEditor ) {
			ActivateObjects();
			this.gameObject.SetActive(false);
		} else {
			StartCoroutine (Intro ());
		}

		Shader.SetGlobalTexture ("_GlobalCubemap", startCubeMap );
	}
	
	// Update is called once per frame
	void Update () {

	}

	void ActivateObjects() {
		testObject.SetActive(true);
		MainGuiObject.SetActive(true);
		SettingsGuiObject.SetActive(true);
		ControlsGuiObject.SetActive(true);
		CommandListExecutorObject.SetActive(true);
	}

	void OnGUI () {

		GUI.color = new Color(1,1,1,backgroundFade);

		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), background);

		int logoWidth = Mathf.FloorToInt (Screen.width * 0.75f);
		int logoHeight = Mathf.FloorToInt (logoWidth * 0.5f);
		int logoPosX = Mathf.FloorToInt (Screen.width * 0.5f - logoWidth * 0.5f);
		int logoPosY = Mathf.FloorToInt (Screen.height * 0.5f - logoHeight * 0.5f);

		GUI.color = new Color(1,1,1,logoFade);

		GUI.DrawTexture (new Rect (logoPosX, logoPosY, logoWidth, logoHeight), logo);
	}

	IEnumerator FadeLogo ( float target, float overTime ) {

		float timer = overTime;
		float original = logoFade;

		while( timer > 0.0f ){
			timer -= Time.deltaTime;
			logoFade = Mathf.Lerp( target, original, timer / overTime );
			yield return new WaitForEndOfFrame();
		}

		logoFade = target;

		//yield return new WaitForEndOfFrame();

	}

	IEnumerator FadeBackground ( float target, float overTime ) {
		
		float timer = overTime;
		float original = backgroundFade;
		
		while( timer > 0.0f ){
			timer -= Time.deltaTime;
			backgroundFade = Mathf.Lerp( target, original, timer / overTime );
			yield return new WaitForEndOfFrame();
		}
		
		backgroundFade = target;

		this.gameObject.SetActive (false);
	}

	IEnumerator Intro () {

		StartCoroutine ( FadeLogo ( 1.0f, 0.5f) );

		yield return new WaitForSeconds(3.0f);

		StartCoroutine ( FadeLogo ( 0.0f, 1.0f) );

		yield return new WaitForSeconds(1.0f);

		StartCoroutine ( FadeBackground ( 0.0f, 1.0f) );

		ActivateObjects();
	}
}
